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

    public Item() {
        Id = "DefaultItem";
        Name = "Default Item";
        Description = "Default Item Description";
        MaxStackSize = 1;
        IsConsumable = false;
        IconPath = "NONE";
    }

    public bool SameItem(Item other) {
        if(other == null) {
            return false;
        }
        return Id == other.Id;
    }

    public bool IsItem(string itemId) {
        if(Id == null) {
            return false;
        }
        return Id == itemId;
    }

	public ItemData Serialize() => new ItemData {
        Id = Id,
        Name = Name,
        Description = Description,
        MaxStackSize = MaxStackSize,
        IsConsumable = IsConsumable,
        IconPath = IconPath,
    };

	public void Deserialize(in ItemData data) {
            Id = data.Id;
			Name = data.Name;
            Description = data.Description;
            MaxStackSize = data.MaxStackSize;
            IsConsumable = data.IsConsumable;
            IconPath = data.IconPath;
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
    }
}