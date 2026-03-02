namespace Objects {
	using System;
	using Godot;
	using Services;
	using ItemSystem;
	using Components;

	public sealed class ObjectNodeFactory {
		private static readonly LogService Log = new(nameof(ObjectNodeFactory), enabled: true);
		private readonly Node ParentNode;

		public ObjectNodeFactory(Node parent) {
			ParentNode = parent;
		}

		public ObjectNode? Spawn(Object obj) {
			ItemDefinition? ItemDefinition = ItemDataBaseManager.Instance.GetItemDefinitionById(obj.ItemId);
			if(ItemDefinition == null) {
				Log.Error($"Failed to spawn object. ItemDefinition with ID {obj.ItemId} not found.");
				return null;
			}
			ItemDataBaseManager.Instance.BuildObjectComponents(obj, ItemDefinition);
			PackedScene? Scene = ItemDefinition.ItemScene;
			if(Scene == null) {
				Log.Error($"Failed to spawn object. ItemDefinition with ID {obj.ItemId} has no ItemScene assigned.");
				return null;
			}
			ObjectNode ChildNode = Scene.Instantiate<ObjectNode>();

			ParentNode.AddChild(ChildNode);
			ChildNode.Bind(obj);

			return ChildNode;
		}
	}

	public partial class Object : IWorldLocation, ISaveable<ObjectData> {
		public string Id { get; private set; } = Guid.NewGuid().ToString();
		public string ItemId { get; private set; } = null!;
		public WorldLocation WorldLocation { get; private set; } = null!;
		public ComponentDictionary<IObjectComponent> ComponentDictionary { get; } = new();

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

			if (WorldLocation == null) {
				WorldLocation = new WorldLocation(data.WorldLocation.Position, data.WorldLocation.Rotation);
			}
			else{
				WorldLocation.Import(data.WorldLocation);
			}
		}
	}
	
	public readonly record struct ObjectData: ISaveData {
		public string Id { get; init; }
		public string ItemId { get; init; }
		public WorldLocationData WorldLocation { get; init; }

	}
}
