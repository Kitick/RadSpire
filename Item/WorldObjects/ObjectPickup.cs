namespace Objects {
    using Godot;
    using System;
    using Services;
    using ItemSystem;
    using System.Collections.Generic;
	using Components;

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
                }
                ObjectNodesInRange.Remove(objNode.Data.Id);
            }
        }








    }

    public partial class ObjectPickupUIManager {
        private static readonly LogService Log = new(nameof(ObjectPickupUIManager), enabled: true);

    }
    
    public partial class ObjectPickupUI : Control {
        private static readonly LogService Log = new(nameof(ObjectPickupUI), enabled: true);
    }

}