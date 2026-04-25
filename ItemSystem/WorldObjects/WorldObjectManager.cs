namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using Components;
using GameWorld;
using Godot;
using InventorySystem;
using ItemSystem;
using ItemSystem.WorldObjects.Hierarchy;
using Services;

public partial class WorldObjectManager : Node, ISaveable<WorldObjectManagerData> {
	private static readonly LogService Log = new(nameof(WorldObjectManager), enabled: true);

	private readonly WorldObjects WorldObjects = new WorldObjects();
	private readonly WorldObjectNodes WorldObjectNodes = new WorldObjectNodes();
	private ObjectNodeFactory ObjectNodeFactory = null!;
	private Node GameWorldNode = null!;
	private Node WorldObjectParentNode = null!;
	private readonly Dictionary<string, Node> AnchorRegistry = new();
	private readonly HashSet<string> MissingAnchorWarnings = new();
	private bool MissingParentAnchorWarningLogged;
	private GameManager? GameManager;
	private bool SetUpComplete = false;

	public void SetUpWorldObjectManager(Node parentNode, Node gameWorldNode, GameManager? gameManager) {
		if(SetUpComplete) {
			Log.Warn("SetUpWorldObjectManager called more than once. Ignoring duplicate call.");
			return;
		}
		if(parentNode == null) {
			Log.Error("SetUpWorldObjectManager called with null parent node.");
			return;
		}
		if(gameWorldNode == null) {
			Log.Error("SetUpWorldObjectManager called with null game world node.");
			return;
		}
		GameWorldNode = gameWorldNode;
		WorldObjectParentNode = parentNode;
		BuildAnchorRegistry(GameWorldNode);
		GameManager = gameManager;
		List<WorldObjectSpawnPoint> spawnPoints = GetSpawnPointsRecursive(GameWorldNode);
		Dictionary<string, (string SpawnPointName, Godot.Collections.Array<WorldObjectSpawnComponentDefinition> ComponentDefinitions)> pendingSpawnComponents = new();
		foreach(WorldObjectSpawnPoint objNode in spawnPoints) {
			if(!GodotObject.IsInstanceValid(objNode)) {
				continue;
			}

			string itemId = objNode.ItemId;
			if(string.IsNullOrWhiteSpace(itemId)) {
				Log.Warn($"Skipping spawn point '{objNode.Name}' because ItemId is empty.");
				continue;
			}

			if(DatabaseManager.Instance.GetItemDefinitionById(itemId) == null) {
				Log.Warn($"Skipping spawn point '{objNode.Name}' because ItemId '{itemId}' is not registered.");
				continue;
			}
			Object obj = new Object(itemId, objNode.GlobalPosition, objNode.GlobalRotation);
			if(TryGetAnchorIdFromNodeChain(objNode, out string anchorId)) {
				obj.ParentAnchorId = anchorId;
			}
			pendingSpawnComponents[obj.Id] = (objNode.Name, objNode.ComponentDefinitions);
			if(!WorldObjects.RegisterWorldObject(obj)) {
				Log.Warn($"Skipping spawn point '{objNode.Name}' because world object registration failed.");
			}
		}
		foreach(WorldObjectSpawnPoint spawnPoint in spawnPoints) {
			Node? spawnPointParent = spawnPoint.GetParent();
			if(spawnPointParent is ObjectNode node) {
				node.QueueFree();
			}
			else {
				Log.Warn($"Spawn point '{spawnPoint.Name}' does not have an ObjectNode parent.");
				spawnPoint.QueueFree();
			}
		}
		ObjectNodeFactory = new ObjectNodeFactory(WorldObjectParentNode);
		foreach(Object obj in WorldObjects.Objects.Values) {
			ObjectNode? node = ObjectNodeFactory.Spawn(obj, ResolveSpawnParent(obj));
			if(node == null) {
				Log.Error($"Failed to spawn world object with ID {obj.Id} and ItemId {obj.ItemId}");
				continue;
			}
			InitializeObjectComponents(obj);
			bool hasInventorySpawnDefinition = false;
			string spawnPointName = "UnknownSpawnPoint";
			if(pendingSpawnComponents.TryGetValue(obj.Id, out var pendingData)) {
				spawnPointName = pendingData.SpawnPointName;
				hasInventorySpawnDefinition = ApplyPendingSpawnComponents(obj, pendingData);
			}
			if(!hasInventorySpawnDefinition) {
				obj.TryFillInventoryFromRarity(spawnPointName);
			}
			WorldObjectNodes.AddObjectNode(node);
		}
		WorldObjects.OnWorldObjectAdded += HandleOnWorldObjectAdded;
		WorldObjects.OnWorldObjectRemoved += HandleOnWorldObjectRemoved;
		SetUpComplete = true;
	}

	public override void _ExitTree() {
		if(!SetUpComplete) {
			return;
		}
		WorldObjects.OnWorldObjectAdded -= HandleOnWorldObjectAdded;
		WorldObjects.OnWorldObjectRemoved -= HandleOnWorldObjectRemoved;
	}

	private static List<WorldObjectSpawnPoint> GetSpawnPointsRecursive(Node root) {
		List<WorldObjectSpawnPoint> results = new List<WorldObjectSpawnPoint>();
		CollectSpawnPoints(root, results);
		return results;
	}

	private static void CollectSpawnPoints(Node node, List<WorldObjectSpawnPoint> results) {
		foreach(Node child in node.GetChildren()) {
			if(child is WorldObjectSpawnPoint spawnPoint) {
				results.Add(spawnPoint);
			}
			CollectSpawnPoints(child, results);
		}
	}

	private bool ApplyPendingSpawnComponents(Object obj, (string SpawnPointName, Godot.Collections.Array<WorldObjectSpawnComponentDefinition> ComponentDefinitions) pendingData) {
		bool hasInventorySpawnDefinition = false;
		foreach(WorldObjectSpawnComponentDefinition definition in pendingData.ComponentDefinitions) {
			if(definition == null) {
				continue;
			}
			if(definition is WorldObjectInventorySpawnDefinition inventorySpawnDefinition) {
				hasInventorySpawnDefinition = true;
				ApplyInventorySpawnDefinition(obj, pendingData.SpawnPointName, inventorySpawnDefinition);
			}
			if(definition is WorldObjectDoorSpawnDefinition doorSpawnDefinition) {
				ApplyDoorSpawnDefinition(obj, pendingData.SpawnPointName, doorSpawnDefinition);
			}
		}
		return hasInventorySpawnDefinition;
	}

	private void ApplyInventorySpawnDefinition(Object obj, string spawnPointName, WorldObjectInventorySpawnDefinition inventorySpawnDefinition) {
		if(!obj.ComponentDictionary.Has<InventoryComponent>()) {
			Log.Warn($"Spawn point '{spawnPointName}' has inventory spawn data but object ItemId '{obj.ItemId}' has no InventoryComponent.");
			return;
		}
		Inventory inventory = obj.ComponentDictionary.Get<InventoryComponent>().Inventory;
		foreach(WorldObjectInventorySpawnEntry entry in inventorySpawnDefinition.Entries) {
			if(entry == null) {
				continue;
			}
			if(DatabaseManager.Instance.GetItemDefinitionById(entry.ItemId) == null) {
				Log.Warn($"Spawn point '{spawnPointName}' contains unknown item '{entry.ItemId}'. Skipping entry.");
				continue;
			}
			Item itemInstance = DatabaseManager.Instance.CreateItemInstanceById(entry.ItemId);
			int quantity = Math.Max(1, entry.Quantity);
			while(quantity > 0) {
				int quantityToAdd = Math.Min(quantity, itemInstance.MaxStackSize);
				ItemSlot itemSlot = new ItemSlot(itemInstance, quantityToAdd);
				ItemSlot remaining = inventory.AddItem(itemSlot);
				int added = quantityToAdd - remaining.Quantity;
				if(added <= 0) {
					Log.Warn($"Failed to add item '{entry.ItemId}' to inventory for spawn point '{spawnPointName}'. Inventory may be full.");
					break;
				}
				quantity -= added;
			}
		}
	}

	private void ApplyDoorSpawnDefinition(Object obj, string spawnPointName, WorldObjectDoorSpawnDefinition doorSpawnDefinition) {
		if(!obj.ComponentDictionary.Has<DoorComponent>()) {
			Log.Warn($"Spawn point '{spawnPointName}' has door spawn data but object ItemId '{obj.ItemId}' has no DoorComponent.");
			return;
		}
		DoorComponent doorComponent = obj.ComponentDictionary.Get<DoorComponent>();
		doorComponent.SpawnPosition = doorSpawnDefinition.SpawnPositionMarker;
		doorComponent.DefaultScene = doorSpawnDefinition.BaseScene;
		doorComponent.ReturnToMainWorld = doorSpawnDefinition.ReturnToMainWorld;
	}

	public bool CreateWorldObject(string itemId, Vector3 position, Vector3 rotation, string parentAnchorId = "") {
		if(!SetUpComplete) {
			Log.Error("Attempted to create world object before WorldObjectManager was set up.");
			return false;
		}
		Object obj = new Object(itemId, position, rotation);
		obj.ParentAnchorId = parentAnchorId ?? string.Empty;
		return WorldObjects.RegisterWorldObject(obj);
	}

	public bool RemoveWorldObject(string objectId) {
		if(!SetUpComplete) {
			Log.Error("Attempted to remove world object before WorldObjectManager was set up.");
			return false;
		}
		return WorldObjects.UnregisterWorldObject(objectId);
	}

	public Object? GetWorldObject(string objectId) {
		if(!SetUpComplete) {
			Log.Error("Attempted to get world object before WorldObjectManager was set up.");
			return null;
		}
		if(WorldObjects.Objects.ContainsKey(objectId)) {
			return WorldObjects.Objects[objectId];
		}
		return null;
	}

	public ObjectNode? GetWorldObjectNode(string objectId) {
		if(!SetUpComplete) {
			Log.Error("Attempted to get world object node before WorldObjectManager was set up.");
			return null;
		}
		ObjectNode? temp = WorldObjectNodes.GetObjectNode(objectId);
		if(temp != null) {
			return temp;
		}
		return null;
	}

	private void HandleOnWorldObjectAdded(Object obj) {
		ObjectNode? node = ObjectNodeFactory.Spawn(obj, ResolveSpawnParent(obj));
		if(node == null) {
			Log.Error($"Failed to spawn world object with ID {obj.Id} and ItemId {obj.ItemId}");
			return;
		}
		InitializeObjectComponents(obj);
		WorldObjectNodes.AddObjectNode(node);
	}

	private void BuildAnchorRegistry(Node root) {
		AnchorRegistry.Clear();
		CollectAnchors(root);
	}

	private void CollectAnchors(Node node) {
		if(node is WorldObjectHierarchyAnchor anchor) {
			if(string.IsNullOrWhiteSpace(anchor.AnchorId)) {
				Log.Warn($"WorldObjectHierarchyAnchor '{anchor.GetPath()}' has an empty AnchorId and will be ignored.");
			}
			else if(AnchorRegistry.ContainsKey(anchor.AnchorId)) {
				Log.Warn($"Duplicate WorldObjectHierarchyAnchor id '{anchor.AnchorId}' found at '{anchor.GetPath()}'. Keeping first definition.");
			}
			else {
				AnchorRegistry.Add(anchor.AnchorId, anchor);
			}
		}

		foreach(Node child in node.GetChildren()) {
			CollectAnchors(child);
		}
	}

	private bool TryGetAnchorIdFromNodeChain(Node startNode, out string anchorId) {
		anchorId = string.Empty;
		Node? current = startNode;
		while(current != null) {
			if(current is WorldObjectHierarchyAnchor anchor && !string.IsNullOrWhiteSpace(anchor.AnchorId)) {
				anchorId = anchor.AnchorId;
				return true;
			}
			current = current.GetParent();
		}
		return false;
	}

	private Node ResolveSpawnParent(Object obj) {
		if(string.IsNullOrWhiteSpace(obj.ParentAnchorId)) {
			if(!MissingParentAnchorWarningLogged) {
				Log.Info("World object missing ParentAnchorId. Falling back to WorldObjectParentNode.");
				MissingParentAnchorWarningLogged = true;
			}
			return WorldObjectParentNode;
		}

		if(AnchorRegistry.TryGetValue(obj.ParentAnchorId, out Node? anchorNode) && GodotObject.IsInstanceValid(anchorNode)) {
			return anchorNode;
		}

		if(MissingAnchorWarnings.Add(obj.ParentAnchorId)) {
			Log.Warn($"World object anchor '{obj.ParentAnchorId}' was not found. Falling back to WorldObjectParentNode.");
		}
		return WorldObjectParentNode;
	}

	private void InitializeObjectComponents(Object obj) {
		if(GameManager == null) {
			return;
		}

		if(obj.ComponentDictionary.Has<DoorComponent>()) {
			DoorComponent doorComponent = obj.ComponentDictionary.Get<DoorComponent>();
			doorComponent.Initialize(GameManager);
		}
	}

	private void HandleOnWorldObjectRemoved(string objectId) {
		WorldObjectNodes.RemoveObjectNode(objectId);
	}

	public WorldObjectManagerData Export() => new WorldObjectManagerData {
		WorldObjects = WorldObjects.Export(),
		SetUpComplete = SetUpComplete,
		ParentNodePath = WorldObjectParentNode.GetPath()
	};

	public void Import(WorldObjectManagerData data) {
		if(SetUpComplete) {
			WorldObjectNodes.ClearAll();
			WorldObjects.Clear();
			foreach(ObjectData objData in data.WorldObjects.Objects.Values) {
				Object obj = new Object();
				obj.Import(objData);
				WorldObjects.RegisterWorldObject(obj);
			}
			return;
		}
		WorldObjects.Import(data.WorldObjects);
		SetUpComplete = data.SetUpComplete;
		if(SetUpComplete) {
			WorldObjectParentNode = GetNode(data.ParentNodePath);
		}
	}
}

public readonly record struct WorldObjectManagerData : ISaveData {
	public WorldObjectsData WorldObjects { get; init; }
	public bool SetUpComplete { get; init; }
	public NodePath ParentNodePath { get; init; }
}

public sealed class WorldObjectNodes {
	private readonly Dictionary<StringName, ObjectNode> ObjectNodes = [];

	public bool AddObjectNode(ObjectNode node) {
		if(node == null || ObjectNodes.ContainsKey(node.Data.Id)) {
			return false;
		}
		ObjectNodes.Add(node.Data.Id, node);
		return true;
	}

	public bool RemoveObjectNode(StringName objectId) {
		ObjectNodes.TryGetValue(objectId, out ObjectNode? node);

		if(node == null) { return false; }

		ObjectNodes.Remove(objectId);
		node.QueueFree();

		return true;
	}

	public ObjectNode? GetObjectNode(StringName objectId) {
		ObjectNodes.TryGetValue(objectId, out ObjectNode? node);
		return node;
	}

	public void ClearAll() {
		foreach(ObjectNode node in ObjectNodes.Values) {
			if(GodotObject.IsInstanceValid(node)) {
				node.QueueFree();
			}
		}
		ObjectNodes.Clear();
	}
}

public partial class WorldObjects : ISaveable<WorldObjectsData> {
	public Dictionary<string, Object> Objects { get; private set; } = [];

	public event Action<Object>? OnWorldObjectAdded;
	public event Action<string>? OnWorldObjectRemoved;

	public bool RegisterWorldObject(Object obj) {
		if(obj == null || Objects.ContainsKey(obj.Id)) {
			return false;
		}
		Objects.Add(obj.Id, obj);
		OnWorldObjectAdded?.Invoke(obj);
		return true;
	}

	public bool UnregisterWorldObject(string objectId) {
		if(!Objects.ContainsKey(objectId)) {
			return false;
		}
		Objects.Remove(objectId);
		OnWorldObjectRemoved?.Invoke(objectId);
		return true;
	}

	public void Clear() {
		Objects.Clear();
	}

	public WorldObjectsData Export() => new WorldObjectsData {
		Objects = ExportObjects()
	};

	public Dictionary<string, ObjectData> ExportObjects() {
		Dictionary<string, ObjectData> data = new Dictionary<string, ObjectData>();
		foreach((string id, Object obj) in Objects) {
			data.Add(id, obj.Export());
		}
		return data;
	}

	public void Import(WorldObjectsData data) {
		Objects.Clear();
		if(data.Objects == null) {
			return;
		}
		foreach(ObjectData objData in data.Objects.Values) {
			Object obj = new Object();
			obj.Import(objData);
			RegisterWorldObject(obj);
		}
	}
}

public readonly record struct WorldObjectsData : ISaveData {
	public Dictionary<string, ObjectData> Objects { get; init; }
}
