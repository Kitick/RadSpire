namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using Components;
using Godot;
using InventorySystem;
using ItemSystem;
using Root;
using Services;

public interface IObjectPickup {
	public ObjectPickup ObjectPickup { get; }
}

public partial class ObjectPickup : Node3D {
	private static readonly LogService Log = new(nameof(ObjectPickup), enabled: true);
	private const float DefaultHoverTargetDistance = 4.0f;
	public InteractionArea InteractionArea = null!;
	public InventoryManager InventoryManager = null!;
	public WorldObjectManager WorldObjectManager = null!;
	public Dictionary<string, ObjectNode> ObjectNodesInRange = new Dictionary<string, ObjectNode>();
	private ObjectNode? BaseTargetObjectNode;
	private ObjectNode? HoverOverrideObjectNode;
	public ObjectNode? CurrentTargetObjectNode { get; private set; }
	// Kept for compatibility with existing call sites.
	public ObjectNode? currentTargetObjectNode => CurrentTargetObjectNode;
	public float HoverTargetDistance { get; set; } = DefaultHoverTargetDistance;
	public event Action<ObjectNode>? AddedTargetObjectNode;
	public event Action<ObjectNode>? RemovedTargetObjectNode;

	public ObjectPickup(InteractionArea interactionArea, InventoryManager inventoryManager) {
		InteractionArea = interactionArea;
		InventoryManager = inventoryManager;
		InteractionArea.BodyEntered += HandleBodyEntered;
		InteractionArea.BodyExited += HandleBodyExited;
		InteractionArea.OnAreaEnteredArea += HandleAreaEntered;
		InteractionArea.OnAreaExitedArea += HandleAreaExited;
	}

	private void HandleAreaEntered(Area3D area) => HandleBodyEntered(area);
	private void HandleAreaExited(Area3D area) => HandleBodyExited(area);

	public void HandleBodyEntered(Node objectNode) {
		ObjectNode? objNode = FindAncestorObjectNode(objectNode);
		if(objNode == null) {
			return;
		}
		if(objNode.Data == null) {
			// ObjectNode.Bind can happen just after overlap enters; retry next frame.
			CallDeferred(nameof(HandleBodyEntered), objectNode);
			return;
		}
		if(ObjectNodesInRange.ContainsKey(objNode.Data.Id)) {
			return;
		}
		ObjectNodesInRange.Add(objNode.Data.Id, objNode);
		if(BaseTargetObjectNode == null) {
			BaseTargetObjectNode = objNode;
			RecomputeEffectiveTarget();
		}
	}

	public void HandleBodyExited(Node objectNode) {
		ObjectNode? objNode = FindAncestorObjectNode(objectNode);
		if(objNode == null || objNode.Data == null) {
			return;
		}
		ObjectNodesInRange.Remove(objNode.Data.Id);
		if(BaseTargetObjectNode != null && BaseTargetObjectNode.Data != null && BaseTargetObjectNode.Data.Id == objNode.Data.Id) {
			BaseTargetObjectNode = null;
			string nextId = GetClosestObjectNodeId();
			if(!string.IsNullOrEmpty(nextId)) {
				BaseTargetObjectNode = ObjectNodesInRange[nextId];
			}
		}
		if(HoverOverrideObjectNode != null && HoverOverrideObjectNode.Data != null && HoverOverrideObjectNode.Data.Id == objNode.Data.Id) {
			HoverOverrideObjectNode = null;
		}
		RecomputeEffectiveTarget();
	}

	private static ObjectNode? FindAncestorObjectNode(Node node) {
		Node? current = node;
		while(current != null) {
			if(current is ObjectNode objectNode) {
				return objectNode;
			}
			current = current.GetParent();
		}
		return null;
	}

	private string GetClosestObjectNodeId() {
		if(ObjectNodesInRange.Count == 0) {
			return "";
		}
		string closestId = "";
		float closestDistance = float.MaxValue;
		foreach(ObjectNode obj in ObjectNodesInRange.Values) {
			Vector3 objGlobalPos = obj.Data.WorldLocation.Position;
			Vector3 InteractionAreaGlobalPos = InteractionArea.GlobalPosition;
			float distance = objGlobalPos.DistanceTo(InteractionAreaGlobalPos);
			if(distance < closestDistance) {
				closestDistance = distance;
				closestId = obj.Data.Id;
			}
		}
		return closestId;
	}

	public void SetHoverOverride(ObjectNode? hoveredObjectNode) {
		if(hoveredObjectNode == null || !IsInstanceValid(hoveredObjectNode) || hoveredObjectNode.Data == null) {
			ClearHoverOverride();
			return;
		}

		float distance = hoveredObjectNode.GlobalPosition.DistanceTo(InteractionArea.GlobalPosition);
		if(distance > HoverTargetDistance) {
			ClearHoverOverride();
			return;
		}

		if(HoverOverrideObjectNode == hoveredObjectNode) {
			return;
		}

		HoverOverrideObjectNode = hoveredObjectNode;
		RecomputeEffectiveTarget();
	}

	public void ClearHoverOverride() {
		if(HoverOverrideObjectNode == null) {
			return;
		}

		HoverOverrideObjectNode = null;
		RecomputeEffectiveTarget();
	}

	private void RecomputeEffectiveTarget() {
		ObjectNode? nextTarget = ResolveEffectiveTarget();
		if(CurrentTargetObjectNode == nextTarget) {
			return;
		}

		ObjectNode? previous = CurrentTargetObjectNode;
		CurrentTargetObjectNode = nextTarget;

		if(previous != null) {
			RemovedTargetObjectNode?.Invoke(previous);
		}
		if(CurrentTargetObjectNode != null) {
			AddedTargetObjectNode?.Invoke(CurrentTargetObjectNode);
		}
	}

	private ObjectNode? ResolveEffectiveTarget() {
		if(IsValidHoverOverride(HoverOverrideObjectNode)) {
			return HoverOverrideObjectNode;
		}
		if(BaseTargetObjectNode != null && IsInstanceValid(BaseTargetObjectNode) && BaseTargetObjectNode.Data != null) {
			return BaseTargetObjectNode;
		}
		return null;
	}

	private bool IsValidHoverOverride(ObjectNode? objectNode) {
		if(objectNode == null || !IsInstanceValid(objectNode) || objectNode.Data == null) {
			return false;
		}

		return objectNode.GlobalPosition.DistanceTo(InteractionArea.GlobalPosition) <= HoverTargetDistance;
	}

	public void AttemptPickup() {
		if(CurrentTargetObjectNode == null) {
			Log.Info("Attempted pickup but no target object node.");
			return;
		}
		string targetItemId = CurrentTargetObjectNode.Data.ItemId;
		Item targetItem = DatabaseManager.Instance.CreateItemInstanceById(targetItemId);
		if(!targetItem.Pickupable) {
			Log.Info($"Item {targetItem.Id} is not pickupable.");
			return;
		}
		ItemSlot targetItemSlot = new ItemSlot(targetItem, 1);
		ItemSlot remainSlot = InventoryManager.AddItemSlotToPlayerInventory(targetItemSlot);
		if(remainSlot.IsEmpty()) {
			HandleChestPickup();
			Log.Info("Successfully picked up item.");
			string removedId = CurrentTargetObjectNode.Data.Id;
			HandleBodyExited(CurrentTargetObjectNode);
			WorldObjectManager.RemoveWorldObject(removedId);
		}
		else {
			Log.Info("Failed to pick up item, not enough inventory space.");
		}
	}

	public void HandleChestPickup() {
		if(CurrentTargetObjectNode == null) {
			return;
		}
		if(CurrentTargetObjectNode.Data.ComponentDictionary.Has<InventoryComponent>()) {
			Inventory chestInventory = CurrentTargetObjectNode.Data.ComponentDictionary.Get<InventoryComponent>().Inventory;
			foreach(ItemSlot slot in chestInventory.ItemSlots) {
				if(slot.IsEmpty()) {
					continue;
				}
				ItemSlot remainSlot = InventoryManager.AddItemSlotToPlayerInventory(slot);
				if(remainSlot.IsEmpty()) {
					slot.ClearSlot();
				}
				else {
					InventoryManager.DropItemSlot(remainSlot);
					slot.ClearSlot();
				}
			}
			return;
		}
	}

	public override void _ExitTree() {
		base._ExitTree();
		InteractionArea.BodyEntered -= HandleBodyEntered;
		InteractionArea.BodyExited -= HandleBodyExited;
		InteractionArea.OnAreaEnteredArea -= HandleAreaEntered;
		InteractionArea.OnAreaExitedArea -= HandleAreaExited;
	}
}

