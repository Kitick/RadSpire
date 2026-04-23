namespace GameWorld;

using Godot;

using ItemSystem.WorldObjects;

public sealed partial class Outside : Node {
	[Export] public WorldEnvironment? WorldEnvironment;
	[Export] public Marker3D PlayerSpawnMarker = null!;
	[Export] public Marker3D NPCSpawnMarker = null!;
	[Export] public Godot.Collections.Array<Marker3D> EnemySpawnPoints = [];
	[Export] public WorldObjectManager WorldObjectManager = null!;
}
