namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
    using Components;

    public partial class ObjectNode : Node3D, ISaveable<ObjectNodeData> {
        private static readonly LogService Log = new(nameof(ObjectNode), enabled: true);
        public Object ObjectData { get; private set; } = null!;

        public PackedScene GetItemScene() {
            if(ObjectData == null) {
                Log.Error("ObjectNode.GetItemScene called but ObjectData is null.");
                return null!;
            }

            ItemDefinition ItemDefinition = ItemDataBaseManager.Instance.GetItemDefinitionById(ObjectData.ItemId);
            if(ItemDefinition == null) {
                Log.Error($"No ItemDefinition found for ItemId '{ObjectData.ItemId}'.");
                return null!;
            }
            else if(ItemDefinition.ItemScene == null) {
                Log.Error($"No ItemScene found for '{ObjectData.ItemId}'.");
                return null!;
            }
            return ItemDefinition.ItemScene;
        }

        public Item GetItem() {
            if(ObjectData == null) {
                Log.Error("ObjectNode.GetItemScene called but ObjectData is null.");
                return null!;
            }
            
            Item Item = ItemDataBaseManager.Instance.CreateItemInstanceById(ObjectData.ItemId);
            if(Item == null) {
                Log.Error($"Failed to create item instance for ItemId '{ObjectData.ItemId}'.");
                return null!;
            }
            return Item;
        }

        public ObjectNodeData Export() => new ObjectNodeData {
            ObjectData = ObjectData.Export()
        };
        
        public void Import(ObjectNodeData data) {
            ObjectData = new Object();
            ObjectData.Import(data.ObjectData);
        }
    }
    
    public readonly record struct ObjectNodeData: ISaveData {
        public ObjectData ObjectData { get; init; }
    }

	public partial class Object : IWorldLocation, ISaveable<ObjectData> {
        private static readonly LogService Log = new(nameof(Object), enabled: true);
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ItemId { get; set; } = null!;
        public WorldLocation WorldLocation { get; set; } = null!;

        public Object() { }

        public ObjectData Export() => new ObjectData{
            Id = Id,
            ItemId = ItemId,
            WorldLocation = WorldLocation.Export()
        };

        public void Import(ObjectData data) {
            Id = data.Id;
            ItemId = data.ItemId;
            WorldLocation = new WorldLocation(data.WorldLocation.Position, data.WorldLocation.Rotation);
        }
    }
    
    public readonly record struct ObjectData: ISaveData {
        public string Id { get; init; }
        public string ItemId { get; init; }
        public WorldLocationData WorldLocation { get; init; }

    }
}