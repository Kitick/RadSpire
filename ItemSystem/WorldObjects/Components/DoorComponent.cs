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
		if(interactor is not Player player) {
			return false;
		}
		if(!IsInitalized) {
			Log.Error("DoorComponent is not initialized properly.");
			return false;
		}

		if(ReturnToMainWorld) {
			if(string.IsNullOrEmpty(GameWorldManager.MainGameWorldId)) {
				Log.Error("Door is configured to return to main world, but MainGameWorldId is not set.");
				return false;
			}

			string currentWorldId = GameWorldManager.CurrentGameWorldId;
			string mainWorldId = GameWorldManager.MainGameWorldId;
			WorldID = mainWorldId;

			Vector3? resolvedSpawnPosition = HasConfiguredSpawnPosition(SpawnPosition) ? SpawnPosition : null;
			if(!resolvedSpawnPosition.HasValue && !string.IsNullOrEmpty(currentWorldId) && currentWorldId != mainWorldId) {
				resolvedSpawnPosition = GameManager.GetMainWorldReturnPosition(currentWorldId);
				if(!resolvedSpawnPosition.HasValue) {
					resolvedSpawnPosition = GameManager.GetLastKnownMainWorldPlayerPosition();
				}
			}

			Log.Info($"Door returning player to main world: {WorldID}");
			return TrySwitchToWorld(WorldID, resolvedSpawnPosition);
		}

		if(HasWorldID) {
			Log.Info($"Player is entering door to WorldID: {WorldID}");
			TryCaptureMainWorldReturnPosition(player, WorldID);
			return TrySwitchToWorld(WorldID, HasConfiguredSpawnPosition(SpawnPosition) ? SpawnPosition : null);
		}

		Log.Info("Door has no target WorldID set.");
		if(DefaultScene == null) {
			Log.Error("Door has no target WorldID and no DefaultScene configured.");
			return false;
		}
		string newWorldId = GameWorldManager.CreateNewGameWorld(DefaultScene);
		if(string.IsNullOrEmpty(newWorldId)) {
			Log.Error("Failed to create new world for door.");
			return false;
		}

		Log.Info($"Created new world with ID: {newWorldId} for door.");
		WorldID = newWorldId;
		TryCaptureMainWorldReturnPosition(player, WorldID);
		return TrySwitchToWorld(WorldID, HasConfiguredSpawnPosition(SpawnPosition) ? SpawnPosition : null);
	}

	private static bool HasConfiguredSpawnPosition(Vector3? spawnPosition) {
		return spawnPosition.HasValue && spawnPosition.Value != Vector3.Zero;
	}

	private void TryCaptureMainWorldReturnPosition(Player player, string destinationWorldId) {
		if(GameWorldManager.CurrentGameWorldId != GameWorldManager.MainGameWorldId) {
			return;
		}
		if(string.IsNullOrEmpty(destinationWorldId) || destinationWorldId == GameWorldManager.MainGameWorldId) {
			return;
		}

		if(GameManager.TryRecordMainWorldReturnPosition(destinationWorldId, player.GlobalPosition)) {
		}
	}

	private bool TrySwitchToWorld(string targetWorldId, Vector3? targetSpawnPosition) {
		if(!IsInitalized) {
			Log.Error("DoorComponent is not initialized properly.");
			return false;
		}
		if(string.IsNullOrEmpty(targetWorldId)) {
			Log.Error("Door has no target world id.");
			return false;
		}
		if(targetSpawnPosition.HasValue) {
			Log.Info($"Door has spawn position: {targetSpawnPosition.Value}");
			return GameManager.SwitchToGameWorld(targetWorldId, targetSpawnPosition);
		}

		Log.Info("Door has no spawn position set.");
		return GameManager.SwitchToGameWorld(targetWorldId);
	}

	public DoorComponentData Export() => new DoorComponentData {
		WorldID = WorldID,
		SpawnPosition = SpawnPosition,
		DefaultScenePath = DefaultScene?.ResourcePath ?? string.Empty,
		ReturnToMainWorld = ReturnToMainWorld,
	};

	public void Import(DoorComponentData data) {
		WorldID = data.WorldID;
		SpawnPosition = data.SpawnPosition;
		if(!string.IsNullOrEmpty(data.DefaultScenePath)) {
			PackedScene? scene = ResourceLoader.Load<PackedScene>(data.DefaultScenePath);
			if(scene != null) {
				DefaultScene = scene;
			}
		}
		ReturnToMainWorld = data.ReturnToMainWorld;
	}
}

public readonly record struct DoorComponentData : ISaveData {
	public string WorldID { get; init; }
	public string DefaultScenePath { get; init; }
	public bool ReturnToMainWorld { get; init; }
	public Vector3? SpawnPosition { get; init; }
}
