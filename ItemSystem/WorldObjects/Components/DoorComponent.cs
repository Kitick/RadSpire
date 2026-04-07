namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using Godot;
using InventorySystem;
using ItemSystem;
using ItemSystem.WorldObjects.House;
using Root;
using Services;
using GameWorld;
using Character;

public interface IDoorComponent { DoorComponent DoorComponent { get; set; } }

public sealed class DoorComponent : IObjectComponent, IInteract, ISaveable<DoorComponentData> {
	private static readonly LogService Log = new(nameof(DoorComponent), enabled: true);
	public Object ComponentOwner { get; init; } = null!;
	public GameWorldManager GameWorldManager = null!;
	public GameManager GameManager = null!;
	public PackedScene DefaultScene { get; set; } = null!;
	public bool ReturnToMainWorld { get; set; }
	public string WorldID { get; set; } = string.Empty;
	public bool HasWorldID => !string.IsNullOrEmpty(WorldID);
	public Vector3? SpawnPosition { get; set; }
	public bool IsInitalized => GameWorldManager != null && GameManager != null;

	public DoorComponent(PackedScene defaultScene, Vector3? spawnPosition, Object owner){
		DefaultScene = defaultScene;
		SpawnPosition = spawnPosition;
		ComponentOwner = owner;
	}

	public void Initialize(GameWorldManager gameWorldManager, GameManager gameManager) {
		GameWorldManager = gameWorldManager;
		GameManager = gameManager;
	}

	public bool Interact<TEntity>(TEntity interactor) {
		if(interactor is not Player) {
			return false;
		}
		if(!IsInitalized) {
			Log.Error("DoorComponent is not initialized properly.");
			return false;
		}

		if(HasWorldID) {
			Log.Info($"Player is entering door to WorldID: {WorldID}");
			return TrySwitchToWorld();
		}
		if(ReturnToMainWorld) {
			if(string.IsNullOrEmpty(GameWorldManager.MainGameWorldId)) {
				Log.Error("Door is configured to return to main world, but MainGameWorldId is not set.");
				return false;
			}
			WorldID = GameWorldManager.MainGameWorldId;
			Log.Info($"Door returning player to main world: {WorldID}");
			return TrySwitchToWorld();
		}

		Log.Info("Door has no target WorldID set.");
		string newWorldId = GameWorldManager.CreateNewGameWorld(DefaultScene);
		if(string.IsNullOrEmpty(newWorldId)) {
			Log.Error("Failed to create new world for door.");
			return false;
		}

		Log.Info($"Created new world with ID: {newWorldId} for door.");
		WorldID = newWorldId;
		return TrySwitchToWorld();
	}

	private bool TrySwitchToWorld() {
		if(!IsInitalized) {
			Log.Error("DoorComponent is not initialized properly.");
			return false;
		}
		if(SpawnPosition.HasValue) {
			Log.Info($"Door has spawn position: {SpawnPosition.Value}");
			return GameManager.SwitchToGameWorld(WorldID, SpawnPosition);
		}

		Log.Info("Door has no spawn position set.");
		return GameManager.SwitchToGameWorld(WorldID);
	}

	public DoorComponentData Export() => new DoorComponentData {
		WorldID = WorldID,
		SpawnPosition = SpawnPosition,
		DefaultScene = DefaultScene,
		ReturnToMainWorld = ReturnToMainWorld,
	};

	public void Import(DoorComponentData data) {
		WorldID = data.WorldID;
		SpawnPosition = data.SpawnPosition;
		DefaultScene = data.DefaultScene;
		ReturnToMainWorld = data.ReturnToMainWorld;
	}
}

public readonly record struct DoorComponentData : ISaveData {
	public string WorldID { get; init; }
	public PackedScene DefaultScene { get; init; }
	public bool ReturnToMainWorld { get; init; }
	public Vector3? SpawnPosition { get; init; }
}
