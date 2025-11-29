using System;
using Components;
using Core;
using SaveSystem;

public class Item : ISaveable<ItemData> {
    //Basic properties of all items
    public string Id {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    }

    public string Name {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    }

    public string Description {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    }
    
    public int MaxStackSize { 
        get; set {
            if(value < 1) {
                field = 1;
                return;
            }
            field = value;
        }
    }
    
    public bool IsStackable => MaxStackSize > 1;

    public bool IsConsumable { get; set; }

    public string IconPath { get; set; }

    //Components
    public Durability? Durability { get; set; }
    public Crafting? Crafting { get; set; }

    public Item() {
        Id = "DefaultItem";
        Name = "Default Item";
        Description = "Default Item Description";
        MaxStackSize = 1;
        IsConsumable = false;
        IconPath = "NONE";
        Durability = null;
    }

    public bool SameItem(Item other) {
        if(other == null) {
            return false;
        }
        return Id == other.Id;
    }

	public ItemData Serialize() => new ItemData {
        Id = Id,
        Name = Name,
        Description = Description,
        MaxStackSize = MaxStackSize,
        IsConsumable = IsConsumable,
        IconPath = IconPath,
        Durability = Durability?.Serialize(),
        Crafting = Crafting?.Serialize(),
    };

	public void Deserialize(in ItemData data) {
        Id = data.Id;
        Name = data.Name;
        Description = data.Description;
        MaxStackSize = data.MaxStackSize;
        IsConsumable = data.IsConsumable;
        IconPath = data.IconPath;
        if(data.Durability != null) {
            Durability = new Durability(data.Durability.Value.MaxDurability);
            Durability.Deserialize(data.Durability.Value);
        }
        if(data.Crafting != null) {
            Crafting = new Crafting();
            Crafting.Deserialize(data.Crafting.Value);
        }
	}
}

namespace SaveSystem {
    public readonly record struct ItemData : ISaveData {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public int MaxStackSize { get; init; }
        public bool IsConsumable { get; init; }
        public string IconPath { get; init; }
        public DurabilityData? Durability { get; init; }
        public CraftingData? Crafting { get; init; }
    }
}