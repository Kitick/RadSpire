namespace GameWorld;

using Godot;
using Root;

public sealed partial class Room : Node {
	[Export] public Marker3D PlayerSpawnMarker = null!;

	public void _Ready() {
		this.ValidateExports();
	}
}
