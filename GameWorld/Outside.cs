namespace GameWorld;

using Godot;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;

public interface IEnemySpawnWorld {
	Godot.Collections.Array<Marker3D> EnemySpawnPoints { get; }
}

public interface INPCSpawnWorld {
	Marker3D NPCSpawnMarker { get; }
}

public sealed partial class Outside : Node, IEnemySpawnWorld, INPCSpawnWorld {
	[Export] public WorldEnvironment? WorldEnvironment;
	[Export] public Marker3D PlayerSpawnMarker = null!;
	[Export] public Marker3D NPCSpawnMarker { get; set; } = null!;
	[Export] public Godot.Collections.Array<Marker3D> EnemySpawnPoints { get; set; } = [];
	[Export] public WorldObjectManager WorldObjectManager = null!;
	[Export] public Item3DIconManager Item3DIconManager = null!;
}
