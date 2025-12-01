using System;
using Components;
using Core;
using SaveSystem;
using Godot;

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
    [Export] public PackedScene? Item3DScene { get; set; } = null;
    //Components
    public Durability? Durability { get; set; } = null;
    public Crafting? Crafting { get; set; } = null;

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
        Item3DScene = Item3DScene,
        Durability = Durability?.Serialize(),
        Crafting = Crafting?.Serialize(),
    };

    public void Deserialize(in ItemData data) {
        Id = data.Id;
        Name = data.Name;
        Description = data.Description;
        MaxStackSize = data.MaxStackSize;
        IsConsumable = data.IsConsumable;
        IconTexture = data.IconTexture;
        if(data.Item3DScene != null) {
            Item3DScene = data.Item3DScene;
        }
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
        public Texture2D IconTexture { get; init; }
        public PackedScene? Item3DScene { get; init; }
        public DurabilityData? Durability { get; init; }
        public CraftingData? Crafting { get; init; }
    }
}