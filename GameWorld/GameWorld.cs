namespace ItemSystem.WorldObjects.House;

using System;
using System.Collections.Generic;
using Character;
using GameWorld;
using Godot;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;
using Root;
using Services;

public sealed partial class GameWorldState : Node, ISaveable<GameWorldStateData> {
	private static readonly LogService Log = new(nameof(GameWorldState), enabled: true);
	[Export] public PackedScene BaseScene = null!;
	public string Id { get; private set; } = Guid.NewGuid().ToString();

	private Node? WorldRoot;
	private Node? ActiveWorldNode;
	private GameWorldManager? GameWorldManager;
	private GameManager? GameManager;
	private bool IsInitialized;
	public Item3DIconManager? Item3DIconManager;
	public WorldObjectManager? WorldObjectManager;
	public EnemyManager? EnemyManager;
	public NPCManager? NPCManager;
	private GameWorldStateData? SavedData;
	public Node? CurrentWorldNode => ActiveWorldNode;

	public void Initialize(Node worldRoot, GameWorldManager gameWorldManager, GameManager? gameManager) {
		if(IsInitialized) {
			Log.Info($"Initialize skipped for world '{Id}' (already initialized).");
			return;
		}

		WorldRoot = worldRoot;
		GameWorldManager = gameWorldManager;
		GameManager = gameManager;

		SetupActiveWorldNode();
		SetupManagers();

		Node worldNode = ActiveWorldNode ?? WorldRoot ?? this;
		WorldObjectManager!.SetUpWorldObjectManager(worldNode, worldNode, GameWorldManager!, GameManager!);
		EnemyManager!.Initialize(worldNode, GameManager!.EnemySceneRef, spawnFromWorld: !SavedData.HasValue);
		NPCManager!.Initialize(worldNode, GameManager.NPCSceneRef, GameManager.QuestManagerRef, spawnFromWorld: !SavedData.HasValue);
		Log.Info($"Initialize world '{Id}' managers: objects={WorldObjectManager != null}, enemies={EnemyManager?.Enemies.Count ?? 0}, npcs={NPCManager?.NPCs.Count ?? 0}");

		if(SavedData.HasValue) {
			ApplySavedData(SavedData.Value);
		}
		else {
			Item3DIconManager?.SetUpItem3DIconManager(worldNode);
		}

		IsInitialized = true;
		Log.Info($"Initialize complete for world '{Id}'.");
	}

	public GameWorldState(PackedScene baseScene, Node worldRoot, GameWorldManager gameWorldManager, GameManager? gameManager) {
		BaseScene = baseScene;
		Initialize(worldRoot, gameWorldManager, gameManager);
	}

	public GameWorldState() { }

	public GameWorldStateData Export() {
		GameWorldStateData data = new() {
			Id = Id,
			BaseScenePath = BaseScene?.ResourcePath ?? string.Empty,
			Item3DIconManager = Item3DIconManager?.Export() ?? SavedData?.Item3DIconManager ?? new Item3DIconManagerData(),
			WorldObjectManager = WorldObjectManager?.Export() ?? SavedData?.WorldObjectManager ?? new WorldObjectManagerData(),
			EnemyManager = EnemyManager?.Export() ?? SavedData?.EnemyManager ?? new EnemyManagerData { Enemies = new Dictionary<string, EnemyData>() },
			NPCManager = NPCManager?.Export() ?? SavedData?.NPCManager ?? new NPCManagerData { NPCs = new Dictionary<string, NPCData>() },
		};

		SavedData = data;
		return data;
	}

	public void Import(GameWorldStateData data) {
		Id = data.Id;
		if(!string.IsNullOrEmpty(data.BaseScenePath)) {
			PackedScene? scene = ResourceLoader.Load<PackedScene>(data.BaseScenePath);
			if(scene != null) {
				BaseScene = scene;
			}
			else {
				GD.PushWarning($"GameWorldState '{Id}' failed to load BaseScene from '{data.BaseScenePath}'.");
			}
		}
		SavedData = data;

		if(Item3DIconManager != null) {
			Item3DIconManager.Import(data.Item3DIconManager);
		}
		if(WorldObjectManager != null) {
			WorldObjectManager.Import(data.WorldObjectManager);
		}
		if(EnemyManager != null) {
			EnemyManager.Import(data.EnemyManager);
		}
		if(NPCManager != null) {
			NPCManager.Import(data.NPCManager);
		}
		Log.Info($"Import world '{Id}' data: enemyRecords={data.EnemyManager.Enemies?.Count ?? 0}, npcRecords={data.NPCManager.NPCs?.Count ?? 0}");
	}

	public void Cleanup() {
		Log.Info($"Cleanup start for world '{Id}'. enemies={EnemyManager?.Enemies.Count ?? 0}, npcs={NPCManager?.NPCs.Count ?? 0}");
		if(IsInstanceValid(Item3DIconManager)) {
			Item3DIconManager.QueueFree();
		}
		if(IsInstanceValid(WorldObjectManager)) {
			WorldObjectManager.QueueFree();
		}
		if(IsInstanceValid(EnemyManager)) {
			EnemyManager.QueueFree();
		}
		if(IsInstanceValid(NPCManager)) {
			NPCManager.QueueFree();
		}
		if(IsInstanceValid(ActiveWorldNode)) {
			ActiveWorldNode.QueueFree();
		}

		Item3DIconManager = null;
		WorldObjectManager = null;
		EnemyManager = null;
		NPCManager = null;
		ActiveWorldNode = null;
		WorldRoot = null;
		IsInitialized = false;
		Log.Info($"Cleanup complete for world '{Id}'.");
	}

	private void ApplySavedData(GameWorldStateData data) {
		if(Item3DIconManager != null) {
			Item3DIconManager.Import(data.Item3DIconManager);
		}
		if(WorldObjectManager != null) {
			WorldObjectManager.Import(data.WorldObjectManager);
		}
		if(EnemyManager != null) {
			EnemyManager.Import(data.EnemyManager);
		}
		if(NPCManager != null) {
			NPCManager.Import(data.NPCManager);
		}
	}

	private void SetupActiveWorldNode() {
		if(ActiveWorldNode != null) {
			return;
		}

		if(BaseScene == null) {
			GD.PushError($"GameWorldState '{Id}' cannot initialize because BaseScene is null.");
			return;
		}

		ActiveWorldNode = BaseScene.Instantiate<Node>();
		if(WorldRoot != null) {
			WorldRoot.AddChild(ActiveWorldNode);
		}
		else {
			AddChild(ActiveWorldNode);
		}
	}

	private void SetupManagers() {
		if(Item3DIconManager == null) {
			Item3DIconManager = new Item3DIconManager();
			AddChild(Item3DIconManager);
		}

		if(WorldObjectManager == null) {
			WorldObjectManager = new WorldObjectManager();
			AddChild(WorldObjectManager);
		}
		if(EnemyManager == null) {
			EnemyManager = new EnemyManager();
			AddChild(EnemyManager);
		}
		if(NPCManager == null) {
			NPCManager = new NPCManager();
			AddChild(NPCManager);
		}
	}
}

public readonly record struct GameWorldStateData : ISaveData {
	public string Id { get; init; }
	public string BaseScenePath { get; init; }
	public Item3DIconManagerData Item3DIconManager { get; init; }
	public WorldObjectManagerData WorldObjectManager { get; init; }
	public EnemyManagerData EnemyManager { get; init; }
	public NPCManagerData NPCManager { get; init; }
}
