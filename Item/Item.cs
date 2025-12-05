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

    [Export] public Godot.Collections.Array<Resource> ComponentResources { get; set; } = new();
    public List<IItemComponent> Components = new();

    public Item() {
        BuildComponents();
    }

    public void BuildComponents() {
        Components.Clear();
        if(ComponentResources == null){
            return;
        }
        foreach(var resource in ComponentResources) {
            if(resource is IItemComponent comp){
                IItemComponent componentInstance = GD.Load<IItemComponent>(resource.ResourcePath);
                Components.Add(componentInstance);
            }
        }
    }

    public bool OnUse(CharacterBody3D user) {
        bool success = false;
        foreach(IItemComponent component in Components) {
            if(component is IUsable usable) {
                success |= usable.OnUse(user);
            }
        }
        return success;
    }

    public bool OnConsume(CharacterBody3D consumer) {
        bool success = false;
        foreach(IItemComponent component in Components) {
            if(component is IConsumable consumable) {
                GD.Print("Consuming item with component: " + consumable.GetType().Name);
                success |= consumable.OnConsume(consumer);
            }
        }
        return success;
    }

    public void OnEquip(CharacterBody3D equipper) {
        foreach(var component in Components) {
            if(component is IEquipable equipable) {
                equipable.OnEquip(equipper);
            }
        }
    }

    public void OnUnequip(CharacterBody3D unequipper) {
        foreach(var component in Components) {
            if(component is IEquipable equipable) {
                equipable.OnUnequip(unequipper);
            }
        }
    }

    public bool AddComponent(IItemComponent component) {
        if(component == null) {
            return false;
        }
        if(!(component is IItemComponent comp)){
            return false;
        }
        foreach(var resource in ComponentResources) {
            if(ReferenceEquals(resource, component)){
                return false;
            }
        }
        ComponentResources.Add((Resource)component);
        Components.Add(component);
        return true;
    }

    public bool RemoveComponent(IItemComponent component) {
        if(component == null){
            return false;
        }
        foreach(IItemComponent comp in Components) {
            if(ReferenceEquals(comp, component)){
                ComponentResources.Remove((Resource)comp);
                Components.Remove(component);
                return true;
            }
        }
        return false;
    }

    public bool HasComponent(IItemComponent component) {
        foreach(IItemComponent comp in Components) {
            if(ReferenceEquals(comp, component)){
                return true;
            }
        }
        return false;
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
        copy.ComponentResources = new Godot.Collections.Array<Resource>(ComponentResources);
        copy.BuildComponents();
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
        ComponentResources = new Godot.Collections.Array<Resource>(loaded.ComponentResources);
        BuildComponents();
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