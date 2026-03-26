namespace GameWorld;

using Godot;

[GlobalClass]
public sealed partial class ItemSpawnEntry : Marker3D {
	[Export] public StringName ItemId = "";
}
