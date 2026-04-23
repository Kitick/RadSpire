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
	public InteractionArea InteractionArea = null!;
	public InventoryManager InventoryManager = null!;
	public WorldObjectManager WorldObjectManager = null!;
	public Dictionary<string, ObjectNode> ObjectNodesInRange = new Dictionary<string, ObjectNode>();
	public ObjectNode? currentTargetObjectNode = null;
	public event Action<ObjectNode>? AddedTargetObjectNode;
	public event Action<ObjectNode>? RemovedTargetObjectNode;

	public ObjectPickup(InteractionArea interactionArea, InventoryManager inventoryManager) {
		InteractionArea = interactionArea;
		InventoryManager = inventoryManager;
		InteractionArea.BodyEntered += HandleBodyEntered;
		InteractionArea.BodyExited += HandleBodyExited;
	}

	public void HandleBodyEntered(Node objectNode) {
		ObjectNode? objNode = FindAncestorObjectNode(objectNode);
		if(objNode == null || objNode.Data == null) {
			return;
		}
		if(ObjectNodesInRange.ContainsKey(objNode.Data.Id)) {
			return;
		}
		ObjectNodesInRange.Add(objNode.Data.Id, objNode);
		if(currentTargetObjectNode == null) {
			currentTargetObjectNode = objNode;
			AddedTargetObjectNode?.Invoke(currentTargetObjectNode);
		}
	}

	public void HandleBodyExited(Node objectNode) {
		ObjectNode? objNode = FindAncestorObjectNode(objectNode);
		if(objNode == null || objNode.Data == null) {
			return;
		}
		ObjectNodesInRange.Remove(objNode.Data.Id);
		if(currentTargetObjectNode != null && currentTargetObjectNode.Data.Id == objNode.Data.Id) {
			currentTargetObjectNode = null;
			RemovedTargetObjectNode?.Invoke(objNode);

			string nextId = GetClosestObjectNodeId();
			if(!string.IsNullOrEmpty(nextId)) {
				ObjectNode nextTarget = ObjectNodesInRange[nextId];
				currentTargetObjectNode = nextTarget;
				AddedTargetObjectNode?.Invoke(currentTargetObjectNode);
			}
		}
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

	public void AttemptPickup() {
		if(currentTargetObjectNode == null) {
			Log.Info("Attempted pickup but no target object node.");
			return;
		}
		string targetItemId = currentTargetObjectNode.Data.ItemId;
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
			string removedId = currentTargetObjectNode.Data.Id;
			HandleBodyExited(currentTargetObjectNode);
			WorldObjectManager.RemoveWorldObject(removedId);
		}
		else {
			Log.Info("Failed to pick up item, not enough inventory space.");
		}
	}

	public void HandleChestPickup() {
		if(currentTargetObjectNode == null) {
			return;
		}
		if(currentTargetObjectNode.Data.ComponentDictionary.Has<InventoryComponent>()) {
			Inventory chestInventory = currentTargetObjectNode.Data.ComponentDictionary.Get<InventoryComponent>().Inventory;
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
	}
}

