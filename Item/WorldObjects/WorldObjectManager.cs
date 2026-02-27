namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
	using System.Collections.Generic;

	public partial class WorldObjectManager : Node, ISaveable<WorldObjectManagerData> {
        private static readonly LogService Log = new(nameof(WorldObjectManager), enabled: true);
        private WorldObjects WorldObjects { get; set; } = new WorldObjects();
        private WorldObjectNodes WorldObjectNodes { get; set; } = new WorldObjectNodes();
        private ObjectNodeFactory ObjectNodeFactory = null!;
        private Node WorldObjectParentNode = null!;
        private bool SetUpComplete = false;

        public void SetUpWorldObjectManager(Node parentNode) {
            WorldObjectParentNode = parentNode;
            foreach(ObjectNode node in WorldObjectParentNode.GetChildren()) {
                if(node is ObjectNode objNode) {
                    WorldObjects.RegisterWorldObject(objNode.Data);
                    WorldObjectNodes.AddObjectNode(objNode);
                }
            }
            ObjectNodeFactory = new ObjectNodeFactory(this);
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
            WorldObjects.Import(data.WorldObjects);
            SetUpComplete = data.SetUpComplete;
            if(SetUpComplete) {
                WorldObjectParentNode = GetNode(data.ParentNodePath);
            }
        }
    }

    public readonly record struct WorldObjectManagerData: ISaveData {
        public WorldObjectsData WorldObjects { get; init; }
        public bool SetUpComplete { get; init; }
        public NodePath ParentNodePath { get; init; }
    }

    public partial class WorldObjectNodes {
        private static readonly LogService Log = new(nameof(WorldObjectNodes), enabled: true);
        private readonly Dictionary<string, ObjectNode> ObjectNodes = new Dictionary<string, ObjectNode>();

        public bool AddObjectNode(ObjectNode node) {
            if(node == null || ObjectNodes.ContainsKey(node.Data.Id)) {
                return false;
            }
            ObjectNodes.Add(node.Data.Id, node);
            return true;
        }

        public bool RemoveObjectNode(string objectId) {
            if(!ObjectNodes.ContainsKey(objectId)) {
                return false;
            }
            ObjectNode node = ObjectNodes[objectId];
            ObjectNodes.Remove(objectId);
            node.QueueFree();
            return true;
        }

        public ObjectNode? GetObjectNode(string objectId) {
            if(ObjectNodes.ContainsKey(objectId)) {
                return ObjectNodes[objectId];
            }
            return null;
        }
    }

    public partial class WorldObjects : ISaveable<WorldObjectsData> {
        private static readonly LogService Log = new(nameof(WorldObjects), enabled: true);

        public Dictionary<string, Object> Objects { get; private set; } = new Dictionary<string, Object>();
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
            foreach(ObjectData objData in data.Objects.Values) {
                Object obj = new Object();
                obj.Import(objData);
                RegisterWorldObject(obj);
            }
        }
    }
    
    public readonly record struct WorldObjectsData: ISaveData {
        public Dictionary<string, ObjectData> Objects { get; init; }
    }
}