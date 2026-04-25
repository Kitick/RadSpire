namespace ItemSystem.WorldObjects;

using Godot;
using ItemSystem;
using Root;
using Services;
using GameWorld;
using Character;

public interface IDoorComponent { DoorComponent DoorComponent { get; set; } }

public sealed class DoorComponent : IObjectComponent, IInteract, ISaveable<DoorComponentData> {
	private static readonly LogService Log = new(nameof(DoorComponent), enabled: true);
	public Object ComponentOwner { get; init; } = null!;
	public GameManager GameManager = null!;
	public PackedScene DefaultScene { get; set; } = null!;
	public bool ReturnToMainWorld { get; set; }
	public Vector3? SpawnPosition { get; set; }
	public bool IsInitialized => GameManager != null;

	public DoorComponent(PackedScene defaultScene, Vector3? spawnPosition, Object owner) {
		DefaultScene = defaultScene;
		SpawnPosition = spawnPosition;
		ComponentOwner = owner;
	}

	public void Initialize(GameManager gameManager) {
		GameManager = gameManager;
	}

	public bool Interact<TEntity>(TEntity interactor) {
		if(interactor is not Player) {
			return false;
		}
		if(!IsInitialized) {
			Log.Error("DoorComponent is not initialized.");
			return false;
		}

		Vector3? spawnPosition = HasConfiguredSpawnPosition(SpawnPosition) ? SpawnPosition : null;

		if(ReturnToMainWorld) {
			Log.Info("Door returning player to outside world.");
			GameManager.SwitchToOutside(spawnPosition);
			return true;
		}

		Log.Info("Door entering building world.");
		GameManager.SwitchToBuilding(spawnPosition);
		return true;
	}

	private static bool HasConfiguredSpawnPosition(Vector3? spawnPosition) {
		return spawnPosition.HasValue && spawnPosition.Value != Vector3.Zero;
	}

	public DoorComponentData Export() => new DoorComponentData {
		SpawnPosition = SpawnPosition,
		DefaultScenePath = DefaultScene?.ResourcePath ?? string.Empty,
		ReturnToMainWorld = ReturnToMainWorld,
	};

	public void Import(DoorComponentData data) {
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
	public string DefaultScenePath { get; init; }
	public bool ReturnToMainWorld { get; init; }
	public Vector3? SpawnPosition { get; init; }
}
