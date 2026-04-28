namespace GameWorld;

using Godot;

public partial class EnemySpawnPoint : Marker3D {
	[Export] public PackedScene? EnemyScene { get; set; }
}
