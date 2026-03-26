namespace Objects;

using Godot;

[GlobalClass]
public partial class WorldObjectInventorySpawnEntry : Resource {
	[Export]
	public string ItemId { get; set; } = string.Empty;

	[Export]
	public int Quantity { get; set; } = 1;
}

