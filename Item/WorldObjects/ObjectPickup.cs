namespace Objects {
    using Godot;
    using System;
    using Services;
    using ItemSystem;
    using System.Collections.Generic;
	using Components;
	using Core;

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

        public ObjectPickup(InteractionArea interactionArea, InventoryManager inventoryManager) {
            InteractionArea = interactionArea;
            InventoryManager = inventoryManager;
            InteractionArea.BodyEntered += HandleBodyEntered;
            InteractionArea.BodyExited += HandleBodyExited;
        }

        public void HandleBodyEntered(Node objectNode) {
            if(objectNode is ObjectNode objNode) {
                ObjectNodesInRange.Add(objNode.Data.Id, objNode);
                if(currentTargetObjectNode == null) {
                    currentTargetObjectNode = objNode;
                }
            }
        }

        public void HandleBodyExited(Node objectNode) {
            if(objectNode is ObjectNode objNode) {
                if(currentTargetObjectNode != null && currentTargetObjectNode.Data.Id == objNode.Data.Id) {
                    currentTargetObjectNode = null;
                    if(ObjectNodesInRange.Count > 0) {
                        currentTargetObjectNode = ObjectNodesInRange[GetClosestObjectNodeId()];
                    }
                }
                ObjectNodesInRange.Remove(objNode.Data.Id);
            }
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
            Item targetItem = ItemDataBaseManager.Instance.CreateItemInstanceById(targetItemId);
            ItemSlot targetItemSlot = new ItemSlot(targetItem, 1);
            ItemSlot remainSlot = InventoryManager.AddItemSlotToPlayerInventory(targetItemSlot);
            if(remainSlot.IsEmpty()) {
                Log.Info("Successfully picked up item.");
                string removedId = currentTargetObjectNode.Data.Id;
                HandleBodyExited(currentTargetObjectNode);
                WorldObjectManager.RemoveWorldObject(removedId);
            }
            else {
                Log.Info("Failed to pick up item, not enough inventory space.");
            }
        }

        public override void _ExitTree() {
            base._ExitTree();
            InteractionArea.BodyEntered -= HandleBodyEntered;
            InteractionArea.BodyExited -= HandleBodyExited;
        }
    }

    public partial class ObjectPickupUIManager {
        private static readonly LogService Log = new(nameof(ObjectPickupUIManager), enabled: true);

    }
    
    public partial class ObjectPickupUI : Control {
        private static readonly LogService Log = new(nameof(ObjectPickupUI), enabled: true);
    }

}