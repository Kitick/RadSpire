using System;
using Components;
using Core;
using SaveSystem;
using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Item : Resource, ISaveable<ItemData> {
    //Basic properties of all items
    [Export]
    public string Id {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    } = "ItemDefault";

    [Export]
    public string Name {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    } = "Default Item";

    [Export]
    public string Description {
        get;
        set {
            if(string.IsNullOrWhiteSpace(value)) {
                return;
            }
            field = value;
        }
    } = "Default Description";

    [Export]
    public int MaxStackSize {
        get; set {
            if(value < 1) {
                field = 1;
                return;
            }
            field = value;
        }
    } = 1;

    public bool IsStackable => MaxStackSize > 1;
    [Export] public bool IsConsumable { get; set; } = false;
    [Export] public Texture2D IconTexture { get; set; } = null!;

    //Components
    public enum ItemComponentType {
        WeaponBase,
        Defense,
        Durability,
        Crafting,
    }
    public Dictionary<ItemComponentType, IItemComponent> components = new();

    public void addComponent(ItemComponentType type, IItemComponent component) {
        if(components.ContainsKey(type)) {
            components[type] = component;
            return;
        }
        components.Add(type, component);
    }

    public void removeComponent(ItemComponentType type) {
        if(components.ContainsKey(type)) {
            components.Remove(type);
        }
    }

    public bool HasComponent(ItemComponentType type) {
        return components.ContainsKey(type);
    }

    public bool OnUse(CharacterBody3D user) {
        bool success = true;
        foreach(var component in components.Values) {
            if(component is IUsable usable) {
                success &= usable.OnUse(user);
            }
        }
        return success;
    }

    public bool OnConsume(CharacterBody3D consumer) {
        bool success = true;
        foreach(var component in components.Values) {
            if(component is IConsumable consumable) {
                success &= consumable.OnConsume(consumer);
            }
        }
        return success;
    }

    public void OnEquip(CharacterBody3D equipper) {
        foreach(var component in components.Values) {
            if(component is IEquipable equipable) {
                equipable.OnEquip(equipper);
            }
        }
    }

    public void OnUnequip(CharacterBody3D unequipper) {
        foreach(var component in components.Values) {
            if(component is IEquipable equipable) {
                equipable.OnUnequip(unequipper);
            }
        }
    }

    public bool SameItem(Item other) {
        if(other == null) {
            return false;
        }
        return Id == other.Id;
    }

    public Item Copy() {
        Item copy = new Item();
        copy.Id = Id;
        copy.Name = Name;
        copy.Description = Description;
        copy.MaxStackSize = MaxStackSize;
        copy.IsConsumable = IsConsumable;
        copy.IconTexture = IconTexture;
        copy.components = new Dictionary<ItemComponentType, IItemComponent>(components);
        return copy;
    }

    public ItemData Serialize() => new ItemData {
        ResourcePath = ResourcePath,
    };

    public void Deserialize(in ItemData data) {
        // Load the item from its resource path and copy properties
        if(string.IsNullOrEmpty(data.ResourcePath)) return;

        var loaded = GD.Load<Item>(data.ResourcePath);
        if(loaded == null) return;

        Id = loaded.Id;
        Name = loaded.Name;
        Description = loaded.Description;
        MaxStackSize = loaded.MaxStackSize;
        IsConsumable = loaded.IsConsumable;
        IconTexture = loaded.IconTexture;
        components = new Dictionary<ItemComponentType, IItemComponent>(loaded.components);
    }

    public static Item? LoadFromData(in ItemData data) {
        if(string.IsNullOrEmpty(data.ResourcePath)) return null;
        return GD.Load<Item>(data.ResourcePath);
    }
}

namespace SaveSystem {
    public readonly record struct ItemData : ISaveData {
        public string ResourcePath { get; init; }
    }
}