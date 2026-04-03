namespace ItemSystem.WorldObjects.House;

using System;
using Components;
using Godot;
using ItemSystem;
using ItemSystem.Icons;
using InventorySystem;
using Services;

public partial class GameWorld : Node, ISaveable<GameWorldData> {
    [Export] public PackedScene BaseScene = null!;
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    private Item3DIconManager Item3DIconManager = null!;
    private WorldObjectManager WorldObjectManager = null!;

	public GameWorldData Export() => new GameWorldData {
		Id = Id,
		Item3DIconManager = Item3DIconManager.Export(),
		WorldObjectManager = WorldObjectManager.Export(),
	};

	public void Import(GameWorldData data) {
		Id = data.Id;
        Item3DIconManager.Import(data.Item3DIconManager);
        WorldObjectManager.Import(data.WorldObjectManager);
	}
}

public readonly record struct GameWorldData : ISaveData {
	public string Id { get; init; }
	public Item3DIconManagerData Item3DIconManager { get; init; }
	public WorldObjectManagerData WorldObjectManager { get; init; }

}
