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
            PlayerInventory = Player.Inventory;
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
            if(@event.IsActionPressed("Interact")) {
                GD.Print("[Item3DIconPickup] Interact action pressed.");
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
            PlayerInventory = Player.Inventory;
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
            RemoveItemIconPrompt(itemIcon3D);
            itemIcon3D.QueueFree();
            GD.Print("[Item3DIconPickup] Item picked up and removed from the world.");
        }

        public void HandleOnBodyEnteredArea(Node3D node) {
            GD.Print("[Item3DIconPickup] Body entered interaction area.");
            var item3DIcon = FindAncestorItem3DIcon(node);
            if(item3DIcon != null) {
                GD.Print("[Item3DIconPickup] Item3DIcon detected in interaction area.");
                CreateItemIconPrompt(item3DIcon);
            }
        }

        public void HandleOnBodyExitedArea(Node3D node) {
            GD.Print("[Item3DIconPickup] Body exited interaction area.");
            var item3DIcon = FindAncestorItem3DIcon(node);
            if(item3DIcon != null) {
                GD.Print("[Item3DIconPickup] Item3DIcon exited interaction area.");
                RemoveItemIconPrompt(item3DIcon);
            }
        }

        private Item3DIcon? FindAncestorItem3DIcon(Node3D node) {
            Node? current = node;
            while(current != null) {
                if(current is Item3DIcon ico) return ico;
                current = current.GetParent();
            }
            return null;
        }

        public void CreatePickupScreen() {
            GD.Print("[Item3DIconPickup] Creating pickup screen.");
            RemovePickupScreen();
            Item3DIconPickupScreenInstance = Item3DIconPickupScreenTemplate.Instantiate<Control>();
            if(Item3DIconPickupScreenInstance == null) {
                GD.PrintErr("[Item3DIconPickup] CreatePickupScreen: Failed to instantiate pickup screen.");
                return;
            }
            GD.Print("[Item3DIconPickup] Pickup screen created successfully.");
            AddChild(Item3DIconPickupScreenInstance);
        }

        public void RemovePickupScreen() {
            GD.Print("[Item3DIconPickup] Removing pickup screen.");
            if(Item3DIconPickupScreenInstance != null) {
                Item3DIconPickupScreenInstance.QueueFree();
                Item3DIconPickupScreenInstance = null;
                GD.Print("[Item3DIconPickup] Pickup screen removed successfully.");
            }
        }

        public void CreateItemIconPrompt(Item3DIcon item3DIcon) {
            if(item3DIcon.Item == null) {
                GD.PrintErr("[Item3DIconPickup] CreateItemIconPrompt called but Item is null.");
                return;
            }
            GD.Print("[Item3DIconPickup] Creating item icon prompt.");
            Control promptInstance = Item3DIconPromptTemplate.Instantiate<Control>();
            if(promptInstance == null) {
                GD.PrintErr("[Item3DIconPickup] CreateItemIconPrompt: Failed to instantiate prompt.");
                return;
            }
            GD.Print("[Item3DIconPickup] Item icon prompt created successfully.");
            promptInstance.GetNode<Label>("GlassPanel/Label").Text = $"{item3DIcon.Item?.Name}";
            promptInstance.GetNode<TextureRect>("GlassPanel/TextureRect").Texture = item3DIcon.Item.IconTexture;
            if(Item3DIconPickupScreenInstance == null) {
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
            GD.Print("[Item3DIconPickup] Removing item icon prompt.");
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
