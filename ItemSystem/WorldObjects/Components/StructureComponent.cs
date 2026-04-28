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
    public string AttachedNpcId { get; private set; } = string.Empty;
    public string AttachedNpcName { get; private set; } = string.Empty;
    public bool HasWorld => WorldID != string.Empty && GameWorldManager != null && GameWorldManager.HasGameWorld(WorldID);
    public NPC? AttachedNPC { get; set; }
    public bool HasAttachedNPC => AttachedNPC != null || !string.IsNullOrWhiteSpace(AttachedNpcId);

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
        AttachedNpcId = npc?.Id ?? string.Empty;
        AttachedNpcName = npc?.DisplayName ?? string.Empty;
    }
    
    public void RemoveAttachedNPC() {
        AttachedNPC = null;
        AttachedNpcId = string.Empty;
        AttachedNpcName = string.Empty;
    }

    public void RestoreAttachedNpc(string npcId, string npcName) {
        AttachedNpcId = npcId ?? string.Empty;
        AttachedNpcName = npcName ?? string.Empty;
    }

    public NPC? ResolveAttachedNPC() {
        if(AttachedNPC != null && GodotObject.IsInstanceValid(AttachedNPC)) {
            return AttachedNPC;
        }
        if(string.IsNullOrWhiteSpace(AttachedNpcId)) {
            return null;
        }
        if(GameWorldManager?.NPCManager == null) {
            return null;
        }

        foreach(NPC npc in GameWorldManager.NPCManager.NPCs.Values) {
            if(npc.Id != AttachedNpcId) {
                continue;
            }
            AttachedNPC = npc;
            AttachedNpcName = npc.DisplayName;
            return npc;
        }

        return null;
    }

	public StructureComponentData Export() => new StructureComponentData {
        WorldID = WorldID,
        StructureName = StructureName,
        TotalValue = TotalValue,
        AttachedNpcId = AttachedNpcId,
        AttachedNpcName = AttachedNpcName,
	};

	public void Import(StructureComponentData data) {
		WorldID = data.WorldID;
        StructureName = data.StructureName;
        TotalValue = data.TotalValue;
        AttachedNpcId = data.AttachedNpcId ?? string.Empty;
        AttachedNpcName = data.AttachedNpcName ?? string.Empty;
        AttachedNPC = null;
	}
}

public readonly record struct StructureComponentData : ISaveData {
    public string WorldID { get; init; }
    public string StructureName { get; init; }
    public int TotalValue { get; init; }
    public string AttachedNpcId { get; init; }
    public string AttachedNpcName { get; init; }
}
