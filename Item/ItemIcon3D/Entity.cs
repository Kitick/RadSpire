namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;

    public partial class Entity : Node3D, ISaveable<EntityData> {
        private static readonly LogService Log = new(nameof(Entity), enabled: true);

		public EntityData Export() => new EntityData{
			Position = GlobalPosition,
			Rotation = GlobalRotation
		};

		public void Import(EntityData data) {
			GlobalPosition = data.Position;
			GlobalRotation = data.Rotation;
		}
    }
    
    public readonly record struct EntityData: ISaveData {
        public Vector3 Position { get; init; }
        public Vector3 Rotation { get; init; }
    }
}