using System;
using Components;
using Core;
using SaveSystem;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Components {
    public partial class Item3DIconPickup : Node3D {
        public Player Player = null!;
        public Inventory PlayerInventory = null!;
        public InteractionArea PlayerInteractionArea = null!;
        [Export] public PackedScene? Item3DIconPromptTemplate = null!;
        [Export] public PackedScene? Item3DIconPickupScreenTemplate = null!;
        public Control? Item3DIconPickupScreenInstance = null;
        OrderedDictionary<Item3DIcon, Control> ItemsInRange = new OrderedDictionary<Item3DIcon, Control>();

        public override void _Ready() {
            base._Ready();
            Player = GetParent<Player>();
            PlayerInventory = Player.PlayerInventory;
            PlayerInteractionArea = Player.GetNode<InteractionArea>("InteractionArea");
            if(PlayerInteractionArea == null) {
                GD.PrintErr("[Item3DIconPickup] _Ready: Player InteractionArea not found.");
                return;
            }
            PlayerInteractionArea.OnBodyEnteredArea += HandleOnBodyEnteredArea;
            PlayerInteractionArea.OnBodyExitedArea += HandleOnBodyExitedArea;
            if(Item3DIconPromptTemplate == null) {
                Item3DIconPromptTemplate = GD.Load<PackedScene>("res://UI/HUD/Item3DIconPickupPrompt.tscn");
            }
            if(Item3DIconPickupScreenTemplate == null) {
                Item3DIconPickupScreenTemplate = GD.Load<PackedScene>("res://UI/HUD/Item3DIconPickupScreen.tscn");
            }
        }

        public override void _UnhandledInput(InputEvent @event) {
            if(@event.IsActionPressed("PickupItem")) {
                GD.Print("[Item3DIconPickup] PickupItem action pressed.");
                if(ItemsInRange.Count == 0) {
                    GD.Print("[Item3DIconPickup] No items in range to pick up.");
                    return;
                }
                else {
                    GD.Print("[Item3DIconPickup] Items in range detected, attempting to pick up.");
                    PickupItem();
                }
            }
        }

        public void PickupItem() {
            Player = GetParent<Player>();
            PlayerInventory = Player.PlayerInventory;
            if(ItemsInRange.Count == 0) {
                GD.Print("[Item3DIconPickup] No item icons in range to pick up.");
                return;
            }
            Item3DIcon itemIcon3D = ItemsInRange.Keys.First();
            if(itemIcon3D.Item == null) {
                GD.PrintErr("[Item3DIconPickup] PickupItem: Item is null.");
                return;
            }
            GD.Print($"[Item3DIconPickup] Picking up item: {itemIcon3D.Item.Name}");
            PlayerInventory.AddItem(itemIcon3D.Item);
            ItemsInRange.Remove(itemIcon3D);
            RemoveItemIconPrompt(itemIcon3D);
            itemIcon3D.QueueFree();
            GD.Print("[Item3DIconPickup] Item picked up and removed from the world.");
        }

        public void HandleOnBodyEnteredArea(Node3D node) {
            GD.Print("[Item3DIconPickup] Body entered interaction area.");
            if(node.GetNode<Item3DIcon>("../..") is Item3DIcon item3DIcon) {
                GD.Print("[Item3DIconPickup] Item3DIcon detected in interaction area.");
                CreateItemIconPrompt(item3DIcon);
            }
        }

        public void HandleOnBodyExitedArea(Node3D node) {
            GD.Print("[Item3DIconPickup] Body exited interaction area.");
            if(node.GetNode<Item3DIcon>("../..") is Item3DIcon item3DIcon) {
                GD.Print("[Item3DIconPickup] Item3DIcon exited interaction area.");
                RemoveItemIconPrompt(item3DIcon);
            }
        }

        public void CreatePickupScreen() {
            RemovePickupScreen();
            Item3DIconPickupScreenInstance = Item3DIconPickupScreenTemplate.Instantiate<Control>();
            if(Item3DIconPickupScreenInstance == null) {
                GD.PrintErr("[Item3DIconPickup] CreatePickupScreen: Failed to instantiate pickup screen.");
                return;
            }
            AddChild(Item3DIconPickupScreenInstance);
        }

        public void RemovePickupScreen() {
            if(Item3DIconPickupScreenInstance != null) {
                Item3DIconPickupScreenInstance.QueueFree();
                Item3DIconPickupScreenInstance = null;
            }
        }

        public void CreateItemIconPrompt(Item3DIcon item3DIcon) {
            if(item3DIcon.Item == null) {
                GD.PrintErr("[Item3DIconPickup] CreateItemIconPrompt called but Item is null.");
                return;
            }
            Control promptInstance = Item3DIconPromptTemplate.Instantiate<Control>();
            promptInstance.GetNode<Label>("GlassPanel/Label").Text = $"{item3DIcon.Item?.Name}";
            promptInstance.GetNode<TextureRect>("GlassPanel/TextureRect").Texture = item3DIcon.Item.IconTexture;
            if(Item3DIconPickupScreenInstance != null) {
                CreatePickupScreen();
            }
            Item3DIconPickupScreenInstance.GetNode<VBoxContainer>("ScrollContainer/PromptContainer").AddChild(promptInstance);
            ItemsInRange.Add(item3DIcon, promptInstance);
        }

        public void RemoveItemIconPrompt(Item3DIcon item3DIcon) {
            if(item3DIcon.Item == null) {
                GD.PrintErr("[Item3DIconPickup] RemoveItemIconPrompt called but Item is null.");
                return;
            }
            if(ItemsInRange.ContainsKey(item3DIcon)) {
                Item3DIconPickupScreenInstance.GetNode<VBoxContainer>("ScrollContainer/PromptContainer").RemoveChild(ItemsInRange[item3DIcon]);
                ItemsInRange[item3DIcon].QueueFree();
                ItemsInRange.Remove(item3DIcon);
            }
            if(ItemsInRange.Count == 0) {
                RemovePickupScreen();
            }
        }
    }
}
