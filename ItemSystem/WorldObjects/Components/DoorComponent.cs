namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using Godot;
using InventorySystem;
using ItemSystem;
using Root;
using Services;

public interface IDoorComponent { DoorComponent DoorComponent { get; set; } }

public sealed class DoorComponent : IObjectComponent, IInteract, ISaveable<DoorComponentData> {
	private static readonly LogService Log = new(nameof(DoorComponent), enabled: true);
	public Object ComponentOwner { get; init; }
	public PackedScene DefaultScene { get; set; } = null!;
	public string WorldID { get; set; } = string.Empty;
	public bool HasWorldID => !string.IsNullOrEmpty(WorldID);
	public Vector3? SpawnPosition { get; set; }

	public DoorComponent(Object owner) {
		ComponentOwner = owner;
	}

	public bool Interact<TEntity>(TEntity interactor) {
		
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

public static class DoorComponentExtensions {

}

public readonly record struct DoorComponentData : ISaveData {
	public string WorldID { get; init; }
	public Vector3? SpawnPosition { get; init; }
}

