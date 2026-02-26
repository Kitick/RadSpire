namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
	using System.Collections.Generic;

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