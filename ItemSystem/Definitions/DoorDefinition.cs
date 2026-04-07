namespace ItemSystem;

using Godot;

[GlobalClass]
public partial class DoorDefinition : ItemComponentDefinition
{
    [Export] public Vector3 SpawnPositionMarker = Vector3.Zero;
    [Export] public PackedScene BaseScene = null!;
}
