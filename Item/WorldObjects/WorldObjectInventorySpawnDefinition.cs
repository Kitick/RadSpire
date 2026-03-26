namespace Objects;

using Godot;

[GlobalClass]
public partial class WorldObjectInventorySpawnDefinition : WorldObjectSpawnComponentDefinition {
	[Export]
	public Godot.Collections.Array<WorldObjectInventorySpawnEntry> Entries { get; set; } = new();
}

