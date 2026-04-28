namespace GameWorld;

using Godot;
using Root;

public sealed partial class NPCSpawnPoint : Marker3D {
	[Export] public NPCID NpcId { get; set; } = NPCID.None;
	[Export] public string DisplayNameOverride { get; set; } = string.Empty;
	[Export] public PackedScene? SceneOverride { get; set; }
}
