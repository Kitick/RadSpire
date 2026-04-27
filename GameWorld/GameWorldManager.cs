namespace ItemSystem.WorldObjects.House;

using System.Collections.Generic;
using Character;
using GameWorld;
using Godot;
using ItemSystem;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;
using Root;
using Services;

public partial class GameWorldManager : Node, ISaveable<GameWorldManagerData> {
	private static readonly LogService Log = new(nameof(GameWorldManager), enabled: true);
	[Export] private PackedScene MainWorldScene = null!;

	private Node? WorldRoot;
	private bool IsInitialized;
	private GameManager? GameManager;

	public string CurrentGameWorldId = null!;
	public string MainGameWorldId = null!;
	public Dictionary<string, GameWorldState> GameWorlds = new();

	public GameWorldState? CurrentGameWorld => GetCurrentGameWorld();
	public Item3DIconManager? Item3DIconManager => CurrentGameWorld?.Item3DIconManager;
	public WorldObjectManager? WorldObjectManager => CurrentGameWorld?.WorldObjectManager;

	public void Initialize(Node worldRoot, GameManager? gameManager) {
		WorldRoot = worldRoot;
		GameManager = gameManager;

		if(GameWorlds.Count == 0) {
			if(MainWorldScene == null) {
				Log.Error("Cannot initialize GameWorldManager: MainWorldScene is not assigned.");
				return;
			}

			GameWorldState mainWorld = new(MainWorldScene, WorldRoot, this, GameManager);
			RegisterGameWorld(mainWorld);
			CurrentGameWorldId = mainWorld.Id;
			MainGameWorldId = mainWorld.Id;
		}
		else if(CurrentGameWorld == null) {
			CurrentGameWorldId = FirstGameWorldId();
			CurrentGameWorld?.Initialize(WorldRoot ?? this, this, GameManager);
		}
		if(string.IsNullOrEmpty(MainGameWorldId)) {
			MainGameWorldId = CurrentGameWorldId;
		}

		IsInitialized = true;
	}

	public bool SwitchToGameWorld(string gameWorldId) {
		if(!GameWorlds.TryGetValue(gameWorldId, out GameWorldState? gameWorld)) {
			Log.Error($"Cannot switch game worlds: world '{gameWorldId}' does not exist.");
			return false;
		}
		if(CurrentGameWorldId == gameWorldId) {
			return true;
		}

		if(CurrentGameWorld != null) {
			GameWorldStateData currentData = CurrentGameWorld.Export();
			CurrentGameWorld.Cleanup();
			CurrentGameWorld.Import(currentData);
		}

		CurrentGameWorldId = gameWorldId;

		if(IsInitialized && WorldRoot != null) {
			gameWorld.Initialize(WorldRoot, this, GameManager);
		}

		return true;
	}

	public string CreateNewGameWorld(PackedScene scene) {
		if(scene == null) {
			Log.Error("Cannot create new game world: scene is null.");
			return null!;
		}
		if(!IsInitialized) {
			Log.Error("Cannot create new game world: GameWorldManager is not initialized.");
			return null!;
		}
		GameWorldState newWorld = new(scene, WorldRoot!, this, GameManager);
		RegisterGameWorld(newWorld);
		return newWorld.Id;
	}

	public bool HasGameWorld(string gameWorldId) {
		return GameWorlds.ContainsKey(gameWorldId);
	}

	public GameWorldState? GetCurrentGameWorld() {
		if(CurrentGameWorldId == null) {
			return null;
		}

		if(GameWorlds.TryGetValue(CurrentGameWorldId, out GameWorldState? gameWorld)) {
			return gameWorld;
		}

		return null;
	}

	public bool RegisterGameWorld(GameWorldState gameWorld) {
		if(GameWorlds.ContainsKey(gameWorld.Id)) {
			return false;
		}

		GameWorlds.Add(gameWorld.Id, gameWorld);
		if(gameWorld.GetParent() != this) {
			AddChild(gameWorld);
		}
		return true;
	}

	public bool UnregisterGameWorld(string gameWorldId) {
		if(!GameWorlds.Remove(gameWorldId, out GameWorldState? gameWorld)) {
			return false;
		}

		CleanupGameWorld(gameWorld);
		if(CurrentGameWorldId == gameWorldId) {
			CurrentGameWorldId = GameWorlds.Count > 0 ? FirstGameWorldId() : null!;
		}

		return true;
	}

	public void BindPlayer(Player player) {
		if(!IsInitialized || Item3DIconManager == null || !IsInstanceValid(player)) {
			return;
		}

		player.InventoryManager.SpawnItem3DIconRequested += HandleSpawnItem3DIconRequested;
		player.PickupComponent.DespawnItem3DIconRequested += Item3DIconManager.RequestDespawnItem;
	}

	public void UnbindPlayer(Player player) {
		if(Item3DIconManager == null || !IsInstanceValid(player)) {
			return;
		}

		player.InventoryManager.SpawnItem3DIconRequested -= HandleSpawnItem3DIconRequested;
		player.PickupComponent.DespawnItem3DIconRequested -= Item3DIconManager.RequestDespawnItem;
	}

	public GameWorldManagerData Export() => new() {
		CurrentGameWorldId = CurrentGameWorldId,
		MainGameWorldId = MainGameWorldId,
		GameWorlds = ExportGameWorlds(),
	};

	public void Import(GameWorldManagerData data) {
		foreach(GameWorldState existingWorld in GameWorlds.Values) {
			CleanupGameWorld(existingWorld);
		}

		CurrentGameWorldId = data.CurrentGameWorldId;
		MainGameWorldId = data.MainGameWorldId;
		GameWorlds = new Dictionary<string, GameWorldState>();

		foreach(KeyValuePair<string, GameWorldStateData> pair in data.GameWorlds) {
			GameWorldState gameWorld = new();
			gameWorld.Import(pair.Value);
			GameWorlds.Add(pair.Key, gameWorld);
			AddChild(gameWorld);
		}
		if(string.IsNullOrEmpty(MainGameWorldId) && GameWorlds.Count > 0) {
			MainGameWorldId = FirstGameWorldId();
		}

		if(IsInitialized && WorldRoot != null && CurrentGameWorld != null && CurrentGameWorld.Item3DIconManager == null) {
			CurrentGameWorld.Initialize(WorldRoot, this, GameManager);
		}
	}

	public void Cleanup() {
		foreach(GameWorldState gameWorld in GameWorlds.Values) {
			CleanupGameWorld(gameWorld);
		}

		GameWorlds.Clear();
		WorldRoot = null;
		IsInitialized = false;
		CurrentGameWorldId = null!;
		MainGameWorldId = null!;
	}

	private Dictionary<string, GameWorldStateData> ExportGameWorlds() {
		Dictionary<string, GameWorldStateData> data = new();
		foreach(KeyValuePair<string, GameWorldState> pair in GameWorlds) {
			data.Add(pair.Key, pair.Value.Export());
		}
		return data;
	}

	private string FirstGameWorldId() {
		foreach(string key in GameWorlds.Keys) {
			return key;
		}

		return null!;
	}

	private static void CleanupGameWorld(GameWorldState? gameWorld) {
		if(gameWorld == null) {
			return;
		}

		gameWorld.Cleanup();
		if(IsInstanceValid(gameWorld)) {
			gameWorld.QueueFree();
		}
	}

	private void HandleSpawnItem3DIconRequested(Item item, Vector3 position) {
		Item3DIconManager?.RequestSpawnItem(item, position);
	}
}

public readonly record struct GameWorldManagerData : ISaveData {
	public string CurrentGameWorldId { get; init; }
	public string MainGameWorldId { get; init; }
	public Dictionary<string, GameWorldStateData> GameWorlds { get; init; }
}
