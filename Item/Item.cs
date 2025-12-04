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
        IconTexture = IconTexture,
        ComponentsData = SerializeComponents(),
    };

    public Dictionary<ItemComponentType, IItemComponent> SerializeComponents(){
        Dictionary<ItemComponentType, IItemComponent> serializedComponents = new();
        foreach(var component in components) {
            serializedComponents.Add(component.Key, component.Value);
        }
        return serializedComponents;
    }

    public void Deserialize(in ItemData data) {
        Id = data.Id;
        Name = data.Name;
        Description = data.Description;
        MaxStackSize = data.MaxStackSize;
        IsConsumable = data.IsConsumable;
        IconTexture = data.IconTexture;
        components = DeserializeComponents(data);
    }

    public Dictionary<ItemComponentType, IItemComponent> DeserializeComponents(in ItemData data){
        Dictionary<ItemComponentType, IItemComponent> deserializedComponents = new();
        foreach(var component in data.ComponentsData) {
            deserializedComponents.Add(component.Key, component.Value);
        }
        return deserializedComponents;
    }
}

namespace SaveSystem {
    public readonly record struct ItemData : ISaveData {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public int MaxStackSize { get; init; }
        public bool IsConsumable { get; init; }
        public Texture2D IconTexture { get; init; }
        public Dictionary<Item.ItemComponentType, IItemComponent> ComponentsData { get; init; }
    }
}