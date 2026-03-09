namespace Root {
	using Godot;

	[GlobalClass]
	public partial class ItemSpawnEntry : Marker3D {
		[Export] public StringName ItemId = "";
	}
}
