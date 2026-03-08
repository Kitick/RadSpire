namespace Objects {
	using System;
	using System.Collections.Generic;
	using Components;
	using Godot;
	using ItemSystem;
	using Services;

	public partial class WorldObjectManager : Node, ISaveable<WorldObjectManagerData> {
		private static readonly LogService Log = new(nameof(WorldObjectManager), enabled: true);

		private readonly WorldObjects WorldObjects = new WorldObjects();
		private readonly WorldObjectNodes WorldObjectNodes = new WorldObjectNodes();
		private ObjectNodeFactory ObjectNodeFactory = null!;
		private Node GameWorldNode = null!;
		private Node WorldObjectParentNode = null!;
		private bool SetUpComplete = false;

		public void SetUpWorldObjectManager(Node parentNode, Node gameWorldNode) {
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

				if(ItemDataBaseManager.Instance.GetItemDefinitionById(itemId) == null) {
					Log.Warn($"Skipping spawn point '{objNode.Name}' because ItemId '{itemId}' is not registered.");
					continue;
				}
				Object obj = new Object(itemId, objNode.GlobalPosition, objNode.GlobalRotation);
				if(objNode.ComponentDefinitions.Count > 0) {
					pendingSpawnComponents[obj.Id] = (objNode.Name, objNode.ComponentDefinitions);
				}
				if(!WorldObjects.RegisterWorldObject(obj)) {
					Log.Warn($"Skipping spawn point '{objNode.Name}' because world object registration failed.");
				}
			}
			foreach(WorldObjectSpawnPoint spawnPoint in spawnPoints) {
				ObjectNode? node = spawnPoint.GetParent<ObjectNode>();
				if(node != null) {
					node.QueueFree();
				}
				else {
					Log.Warn($"Spawn point '{spawnPoint.Name}' does not have an ObjectNode parent.");
					spawnPoint.QueueFree();
				}
			}
			ObjectNodeFactory = new ObjectNodeFactory(WorldObjectParentNode);
			foreach(Object obj in WorldObjects.Objects.Values) {
				ObjectNode? node = ObjectNodeFactory.Spawn(obj);
				if(node == null) {
					Log.Error($"Failed to spawn world object with ID {obj.Id} and ItemId {obj.ItemId}");
					continue;
				}
				if(pendingSpawnComponents.TryGetValue(obj.Id, out var pendingData)) {
					ApplyPendingSpawnComponents(obj, pendingData);
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

		private void ApplyPendingSpawnComponents(Object obj, (string SpawnPointName, Godot.Collections.Array<WorldObjectSpawnComponentDefinition> ComponentDefinitions) pendingData) {
			foreach(WorldObjectSpawnComponentDefinition definition in pendingData.ComponentDefinitions) {
				if(definition == null) {
					continue;
				}
				if(definition is WorldObjectInventorySpawnDefinition inventorySpawnDefinition) {
					ApplyInventorySpawnDefinition(obj, pendingData.SpawnPointName, inventorySpawnDefinition);
				}
			}
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
				if(ItemDataBaseManager.Instance.GetItemDefinitionById(entry.ItemId) == null) {
					Log.Warn($"Spawn point '{spawnPointName}' contains unknown item '{entry.ItemId}'. Skipping entry.");
					continue;
				}
				Item itemInstance = ItemDataBaseManager.Instance.CreateItemInstanceById(entry.ItemId);
				int quantity = Math.Max(1, entry.Quantity);
				ItemSlot itemSlot = new ItemSlot(itemInstance, quantity);
				ItemSlot remainder = inventory.AddItem(itemSlot);
				if(!remainder.IsEmpty()) {
					Log.Warn($"Spawn point '{spawnPointName}' inventory overflow for item '{entry.ItemId}'. Remaining quantity: {remainder.Quantity}.");
				}
			}
		}

		public bool CreateWorldObject(string itemId, Vector3 position, Vector3 rotation) {
			if(!SetUpComplete) {
				Log.Error("Attempted to create world object before WorldObjectManager was set up.");
				return false;
			}
			Object obj = new Object(itemId, position, rotation);
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
			ObjectNode? node = ObjectNodeFactory.Spawn(obj);
			if(node == null) {
				Log.Error($"Failed to spawn world object with ID {obj.Id} and ItemId {obj.ItemId}");
				return;
			}
			WorldObjectNodes.AddObjectNode(node);
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

			if(node == null){ return false; }

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
}