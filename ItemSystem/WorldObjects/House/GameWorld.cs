namespace ItemSystem.WorldObjects.House;

using System;
using GameWorld;
using Godot;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;
using Services;

public sealed partial class GameWorldState : Node, ISaveable<GameWorldStateData> {
	[Export] public PackedScene BaseScene = null!;
	public string Id { get; private set; } = Guid.NewGuid().ToString();

	private Node? WorldRoot;
	private Node? ActiveWorldNode;
	private GameWorldManager? GameWorldManager;
	private GameManager? GameManager;
	public Item3DIconManager? Item3DIconManager;
	public WorldObjectManager? WorldObjectManager;
	private GameWorldStateData? SavedData;
	private bool OwnsActiveWorldNode;

	public void Initialize(Node worldRoot, GameWorldManager gameWorldManager, GameManager? gameManager) {
		WorldRoot = worldRoot;
		GameWorldManager = gameWorldManager;
		GameManager = gameManager;

		SetupActiveWorldNode();
		SetupManagers();

		Node worldNode = ActiveWorldNode ?? WorldRoot ?? this;
		WorldObjectManager!.SetUpWorldObjectManager(worldNode, worldNode, GameWorldManager!, GameManager!);

		if(SavedData.HasValue) {
			ApplySavedData(SavedData.Value);
		}
		else {
			Item3DIconManager?.SetUpItem3DIconManager(worldNode);
		}
	}

	public GameWorldState(PackedScene baseScene, Node worldRoot, GameWorldManager gameWorldManager, GameManager? gameManager) {
		BaseScene = baseScene;
		Initialize(worldRoot, gameWorldManager, gameManager);
	}

	public GameWorldState() { }

	public GameWorldStateData Export() {
		GameWorldStateData data = new() {
			Id = Id,
			Item3DIconManager = Item3DIconManager?.Export() ?? SavedData?.Item3DIconManager ?? new Item3DIconManagerData(),
			WorldObjectManager = WorldObjectManager?.Export() ?? SavedData?.WorldObjectManager ?? new WorldObjectManagerData(),
		};

		SavedData = data;
		return data;
	}

	public void Import(GameWorldStateData data) {
		Id = data.Id;
		SavedData = data;

		if(Item3DIconManager != null) {
			Item3DIconManager.Import(data.Item3DIconManager);
		}
		if(WorldObjectManager != null) {
			WorldObjectManager.Import(data.WorldObjectManager);
		}
	}

	public void Cleanup() {
		if(IsInstanceValid(Item3DIconManager)) {
			Item3DIconManager.QueueFree();
		}
		if(IsInstanceValid(WorldObjectManager)) {
			WorldObjectManager.QueueFree();
		}
		if(OwnsActiveWorldNode && IsInstanceValid(ActiveWorldNode)) {
			ActiveWorldNode.QueueFree();
		}

		Item3DIconManager = null;
		WorldObjectManager = null;
		ActiveWorldNode = null;
		WorldRoot = null;
		OwnsActiveWorldNode = false;
	}

	private void ApplySavedData(GameWorldStateData data) {
		if(Item3DIconManager != null) {
			Item3DIconManager.Import(data.Item3DIconManager);
		}
		if(WorldObjectManager != null) {
			WorldObjectManager.Import(data.WorldObjectManager);
		}
	}

	private void SetupActiveWorldNode() {
		if(ActiveWorldNode != null) {
			return;
		}

		if(BaseScene != null) {
			ActiveWorldNode = BaseScene.Instantiate<Node>();
			if(WorldRoot != null) {
				WorldRoot.AddChild(ActiveWorldNode);
			}
			else {
				AddChild(ActiveWorldNode);
			}
			OwnsActiveWorldNode = true;
			return;
		}

		ActiveWorldNode = WorldRoot ?? this;
		OwnsActiveWorldNode = false;
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
	}

}

public readonly record struct GameWorldStateData : ISaveData {
	public string Id { get; init; }
	public Item3DIconManagerData Item3DIconManager { get; init; }
	public WorldObjectManagerData WorldObjectManager { get; init; }
}
