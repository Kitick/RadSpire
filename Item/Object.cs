namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
    using Components;

    public partial class ObjectNode : Node3D {
        public Object Data { get; private set; } = null!;

        public void Bind(Object obj) {
            Data = obj;

            GlobalPosition = obj.WorldLocation.Position;
            GlobalRotation = obj.WorldLocation.Rotation;

            obj.WorldLocation.When((from, to) => {
                GlobalPosition = to.Position;
                GlobalRotation = to.Rotation;
            });
        }
    }

    public sealed class WorldObjectFactory {
        private readonly Node ParentNode;

        public WorldObjectFactory(Node parent) {
            ParentNode = parent;
        }

        public ObjectNode Spawn(Object obj) {
            ItemDefinition ItemDefinition = ItemDataBaseManager.Instance.GetItemDefinitionById(obj.ItemId);
            PackedScene Scene = ItemDefinition.ItemScene;
            ObjectNode ChildNode = Scene.Instantiate<ObjectNode>();

            ParentNode.AddChild(ChildNode);
            ChildNode.Bind(obj);

            return ChildNode;
        }
    }

    public sealed class Object : ISaveable<ObjectData> {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string ItemId { get; private set; } = null!;
        public WorldLocation WorldLocation { get; private set; } = null!;

        public Object(string itemId, Vector3 pos, Vector3 rot){
            ItemId = itemId;
            WorldLocation = new WorldLocation(pos, rot);
        }

        public Object() {}

        public ObjectData Export() => new ObjectData {
            Id = Id,
            ItemId = ItemId,
            WorldLocation = WorldLocation.Export()
        };

        public void Import(ObjectData data) {
            Id = data.Id;
            ItemId = data.ItemId;
            WorldLocation = new WorldLocation(
                data.WorldLocation.Position,
                data.WorldLocation.Rotation
            );
        }
    }
    
    public readonly record struct ObjectData: ISaveData {
        public string Id { get; init; }
        public string ItemId { get; init; }
        public WorldLocationData WorldLocation { get; init; }

    }
}