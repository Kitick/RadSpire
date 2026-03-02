namespace Objects{
	using Core;
	using Godot;

	public partial class WorldObjectSpawnPoint : Node3D {
		[Export] public string ItemId { get; set; } = ItemID.AppleRed;
	}
}
