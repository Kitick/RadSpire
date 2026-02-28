namespace Objects{
    using Godot;
    using System;
    using Services;
    using ItemSystem;
    using Components;
    public partial class WorldObjectSpawnPoint : Node3D {
        private static readonly LogService Log = new(nameof(WorldObjectSpawnPoint), enabled: true);
        [Export] public ItemDefinition ItemDefinition = null!;
    }
}