namespace ItemSystem.WorldObjects;

using Godot;

[GlobalClass]
public partial class WorldObjectDoorSpawnDefinition : WorldObjectSpawnComponentDefinition {
    [Export] public Vector3 SpawnPositionMarker = Vector3.Zero;
    [Export] public PackedScene BaseScene = null!;
}

