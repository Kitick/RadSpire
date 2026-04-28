namespace ItemSystem.WorldObjects;

using System;
using Components;
using Godot;
using ItemSystem;
using Services;

public sealed class ObjectNodeFactory {
	private static readonly LogService Log = new(nameof(ObjectNodeFactory), enabled: true);
	private readonly Node ParentNode;

	public ObjectNodeFactory(Node parent) {
		ParentNode = parent;
	}

	public ObjectNode? Spawn(Object obj, Node? parentNode = null) {
		ItemDefinition? ItemDefinition = DatabaseManager.Instance.GetItemDefinitionById(obj.ItemId);
		if(ItemDefinition == null) {
			Log.Error($"Failed to spawn object. ItemDefinition with ID {obj.ItemId} not found.");
			return null;
		}
		DatabaseManager.Instance.BuildObjectComponents(obj, ItemDefinition);
		obj.ApplyComponentData();
		PackedScene? Scene = ItemDefinition.ItemScene;
		if(Scene == null) {
			Log.Error($"Failed to spawn object. ItemDefinition with ID {obj.ItemId} has no ItemScene assigned.");
			return null;
		}
		ObjectNode ChildNode = Scene.Instantiate<ObjectNode>();

		Node targetParent = parentNode ?? ParentNode;
		targetParent.AddChild(ChildNode);
		ChildNode.Bind(obj);

		return ChildNode;
	}
}

public partial class Object : IWorldLocation, ISaveable<ObjectData> {
	public string Id { get; private set; } = Guid.NewGuid().ToString();
	public string ItemId { get; private set; } = null!;
	public string ParentAnchorId { get; set; } = string.Empty;
	public WorldLocation WorldLocation { get; private set; } = null!;
	public ComponentDictionary<IObjectComponent> ComponentDictionary { get; } = new();
	private InventoryComponentData? InventoryComponentData;
	private DoorComponentData? SavedDoorComponentData;
	private StructureComponentData? SavedStructureComponentData;

	public Object(string itemId, Vector3 pos, Vector3 rot) {
		ItemId = itemId;
		WorldLocation = new WorldLocation(pos, rot);
	}

	public Object() { }

	public ObjectData Export() => new ObjectData {
		Id = Id,
		ItemId = ItemId,
		ParentAnchorId = ParentAnchorId,
		WorldLocation = WorldLocation.Export(),
		InventoryComponentData = ExportInventoryComponent(),
		DoorComponentData = ExportDoorComponent(),
		StructureComponentData = ExportStructureComponent(),
	};

	public InventoryComponentData? ExportInventoryComponent() {
		if(ComponentDictionary.Has<InventoryComponent>()) {
			return ComponentDictionary.Get<InventoryComponent>().Export();
		}
		return null;
	}

	public DoorComponentData? ExportDoorComponent() {
		if(ComponentDictionary.Has<DoorComponent>()) {
			return ComponentDictionary.Get<DoorComponent>().Export();
		}
		return null;
	}

	public StructureComponentData? ExportStructureComponent() {
		if(ComponentDictionary.Has<StructureComponent>()) {
			return ComponentDictionary.Get<StructureComponent>().Export();
		}
		return null;
	}

	public void Import(ObjectData data) {
		Id = data.Id;
		ItemId = data.ItemId;
		ParentAnchorId = data.ParentAnchorId ?? string.Empty;

		if(WorldLocation == null) {
			WorldLocation = new WorldLocation(data.WorldLocation.Position, data.WorldLocation.Rotation);
		}
		else {
			WorldLocation.Import(data.WorldLocation);
		}

		InventoryComponentData = data.InventoryComponentData;
		SavedDoorComponentData = data.DoorComponentData;
		SavedStructureComponentData = data.StructureComponentData;
		ApplyComponentData();
	}

	public void ApplyComponentData() {
		if(InventoryComponentData.HasValue && ComponentDictionary.Has<InventoryComponent>()) {
			ComponentDictionary.Get<InventoryComponent>().Import(InventoryComponentData.Value);
			InventoryComponentData = null;
		}
		if(SavedDoorComponentData.HasValue && ComponentDictionary.Has<DoorComponent>()) {
			ComponentDictionary.Get<DoorComponent>().Import(SavedDoorComponentData.Value);
			SavedDoorComponentData = null;
		}
		if(SavedStructureComponentData.HasValue && ComponentDictionary.Has<StructureComponent>()) {
			ComponentDictionary.Get<StructureComponent>().Import(SavedStructureComponentData.Value);
			SavedStructureComponentData = null;
		}
	}
}

public readonly record struct ObjectData : ISaveData {
	public string Id { get; init; }
	public string ItemId { get; init; }
	public string ParentAnchorId { get; init; }
	public WorldLocationData WorldLocation { get; init; }
	public InventoryComponentData? InventoryComponentData { get; init; }
	public DoorComponentData? DoorComponentData { get; init; }
	public StructureComponentData? StructureComponentData { get; init; }

}

