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
	public string WorldID { get; set; } = string.Empty;
	public bool HasWorldID => !string.IsNullOrEmpty(WorldID);
	public Vector3? SpawnPosition { get; set; }

	public DoorComponent(Object owner, GameWorldManager gameWorldManager, GameManager gameManager) {
		ComponentOwner = owner;
		GameWorldManager = gameWorldManager;
		GameManager = gameManager;
	}

	public bool Interact<TEntity>(TEntity interactor) {
		if(interactor is not Player) {
			return false;
		}

		if(HasWorldID) {
			Log.Info($"Player is entering door to WorldID: {WorldID}");
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
	};

	public void Import(DoorComponentData data) {
		WorldID = data.WorldID;
		SpawnPosition = data.SpawnPosition;
	}
}

public readonly record struct DoorComponentData : ISaveData {
	public string WorldID { get; init; }
	public Vector3? SpawnPosition { get; init; }
}

