namespace ItemSystem.WorldObjects;

using System;
using Godot;
using InventorySystem;
using ItemSystem;
using ItemSystem.WorldObjects.House;
using Root;
using Services;
using GameWorld;
using Character;

public interface IStructureComponent { StructureComponent StructureComponent { get; set; } }

public sealed class StructureComponent : IObjectComponent, ISaveable<StructureComponentData> {
	private static readonly LogService Log = new(nameof(StructureComponent), enabled: true);
    public Object ComponentOwner { get; init; } = null!;
    public string WorldID { get; set; } = string.Empty;
    public GameWorldManager GameWorldManager = null!;
    public string StructureName { get; set; } = string.Empty;
    public int Value { get; set; } = 0;
    public int TotalValue { get; set; }
    public bool HasWorld => WorldID != string.Empty && GameWorldManager != null && GameWorldManager.HasGameWorld(WorldID);
    public NPC? AttachedNPC { get; set; }
    public bool HasAttachedNPC => AttachedNPC != null;

    public StructureComponent(Object owner) {
        ComponentOwner = owner;
        ItemDefinition Structure = DatabaseManager.Instance.GetItemDefinitionById(ComponentOwner.ItemId);
        if(Structure == null) {
            Log.Error($"Failed to initialize StructureComponent. ItemDefinition with ID {ComponentOwner.ItemId} not found.");
            return;
        }
        StructureName = Structure.Name;
    }

    public void AddWorld(string worldID, GameWorldManager gameWorldManager) {
        WorldID = worldID;
        GameWorldManager = gameWorldManager;
        UpdateTotalValue();
    }

    public void UpdateTotalValue() {
        if(!HasWorld) {
            Log.Error("Cannot update total value: StructureComponent is not associated with a world.");
            return;
        }
        GameWorldState? gameWorld = GameWorldManager.GetGameWorld(WorldID);
        if(gameWorld == null) {
            Log.Error($"Cannot update total value: Game world with ID {WorldID} not found.");
            return;
        }
        TotalValue = Value;
        if(gameWorld.WorldObjectManager == null) {
            Log.Error($"Cannot update total value: WorldObjectManager for world ID {WorldID} not found.");
            return;
        }

        foreach(Object worldObject in gameWorld.WorldObjectManager.GetWorldObjects()) {
            if(!worldObject.ComponentDictionary.Has<FurnitureValueComponent>()) {
                continue;
            }

            FurnitureValueComponent furnitureValue = worldObject.ComponentDictionary.Get<FurnitureValueComponent>();
            TotalValue += furnitureValue.GetValue();
        }
    }

    public void AddAttachedNPC(NPC npc) {
        AttachedNPC = npc;
    }
    
    public void RemoveAttachedNPC() {
        AttachedNPC = null;
    }

	public StructureComponentData Export() => new StructureComponentData {
        WorldID = WorldID,
        StructureName = StructureName,
        TotalValue = TotalValue
	};

	public void Import(StructureComponentData data) {
		WorldID = data.WorldID;
        StructureName = data.StructureName;
        TotalValue = data.TotalValue;
	}
}

public readonly record struct StructureComponentData : ISaveData {
    public string WorldID { get; init; }
    public string StructureName { get; init; }
    public int TotalValue { get; init; }
}
