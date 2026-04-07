namespace ItemSystem.WorldObjects;

using Godot;
using Root;

public partial class WorldObjectSpawnPoint : Node3D {
	[Export] public string ItemId { get; set; } = ItemID.AppleRed;
	[Export] public Godot.Collections.Array<WorldObjectSpawnComponentDefinition> ComponentDefinitions { get; set; } = new();
}
