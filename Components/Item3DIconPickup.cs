using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Components {
	public partial class Item3DIconPickup : Node3D {
		private static readonly Logger Log = new(nameof(Item3DIconPickup), enabled: true);

		public Player Player = null!;
		public Inventory PlayerInventory = null!;
		public Inventory PlayerHotbar = null!;
		public InteractionArea PlayerInteractionArea = null!;
		[Export] public PackedScene? Item3DIconPromptTemplate = null!;
		[Export] public PackedScene? Item3DIconPickupScreenTemplate = null!;
		public Control? Item3DIconPickupScreenInstance = null;
		OrderedDictionary<Item3DIcon, Control> ItemsInRange = new OrderedDictionary<Item3DIcon, Control>();

		public override void _Ready() {
			base._Ready();
			Player = GetParent<Player>();
			PlayerInventory = Player.Inventory;
			PlayerHotbar = Player.Hotbar;
			PlayerInteractionArea = Player.GetNode<InteractionArea>("InteractionArea");
			if(PlayerInteractionArea == null) {
				Log.Error("Player InteractionArea not found.");
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
				Log.Info("Interact action pressed.");
				if(ItemsInRange.Count == 0) {
					Log.Info("No items in range to pick up.");
					return;
				}
				else {
					Log.Info("Items in range detected, attempting to pick up.");
					PickupItem();
				}
			}
		}

		public void PickupItem() {
			Player = GetParent<Player>();
			PlayerInventory = Player.Inventory;
			PlayerHotbar = Player.Hotbar;
			if(ItemsInRange.Count == 0) {
				Log.Info("No item icons in range to pick up.");
				return;
			}
			Item3DIcon itemIcon3D = ItemsInRange.Keys.First();
			if(itemIcon3D.Item == null) {
				Log.Error("PickupItem: Item is null.");
				return;
			}
			Log.Info($"Picking up item: {itemIcon3D.Item.Name}");
			if(PlayerHotbar.AddItem(itemIcon3D.Item)) {
				Log.Info("Item added to Hotbar.");
				RemoveItemIconPrompt(itemIcon3D);
				itemIcon3D.QueueFree();
				Log.Info("Item picked up and removed from the world.");
				return;
			}
			else if(PlayerInventory.AddItem(itemIcon3D.Item)) {
				Log.Info("Item added to Inventory.");
				RemoveItemIconPrompt(itemIcon3D);
				itemIcon3D.QueueFree();
				Log.Info("Item picked up and removed from the world.");
				return;
			}
			else {
				Log.Info("Inventory and Hotbar full, cannot pick up item.");
				return;
			}

		}

		public void HandleOnBodyEnteredArea(Node3D node) {
			Log.Info("Body entered interaction area.");
			var item3DIcon = FindAncestorItem3DIcon(node);
			if(item3DIcon != null) {
				Log.Info("Item3DIcon detected in interaction area.");
				CreateItemIconPrompt(item3DIcon);
			}
		}

		public void HandleOnBodyExitedArea(Node3D node) {
			Log.Info("Body exited interaction area.");
			var item3DIcon = FindAncestorItem3DIcon(node);
			if(item3DIcon != null) {
				Log.Info("Item3DIcon exited interaction area.");
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
			Log.Info("Creating pickup screen.");
			RemovePickupScreen();
			Item3DIconPickupScreenInstance = Item3DIconPickupScreenTemplate!.Instantiate<Control>();
			if(Item3DIconPickupScreenInstance == null) {
				Log.Error("CreatePickupScreen: Failed to instantiate pickup screen.");
				return;
			}
			Log.Info("Pickup screen created successfully.");
			AddChild(Item3DIconPickupScreenInstance);
		}

		public void RemovePickupScreen() {
			Log.Info("Removing pickup screen.");
			if(Item3DIconPickupScreenInstance != null) {
				Item3DIconPickupScreenInstance.QueueFree();
				Item3DIconPickupScreenInstance = null;
				Log.Info("Pickup screen removed successfully.");
			}
		}

		public void CreateItemIconPrompt(Item3DIcon item3DIcon) {
			if(item3DIcon.Item == null) {
				Log.Error("CreateItemIconPrompt called but Item is null.");
				return;
			}
			Log.Info("Creating item icon prompt.");
			Control promptInstance = Item3DIconPromptTemplate!.Instantiate<Control>();
			if(promptInstance == null) {
				Log.Error("CreateItemIconPrompt: Failed to instantiate prompt.");
				return;
			}
			Log.Info("Item icon prompt created successfully.");
			promptInstance.GetNode<Label>("GlassPanel/Label").Text = $"{item3DIcon.Item?.Name}";
			promptInstance.GetNode<TextureRect>("GlassPanel/TextureRect").Texture = item3DIcon.Item!.IconTexture;
			if(Item3DIconPickupScreenInstance == null) {
				CreatePickupScreen();
			}
			Item3DIconPickupScreenInstance!.GetNode<VBoxContainer>("ScrollContainer/PromptContainer").AddChild(promptInstance);
			ItemsInRange.Add(item3DIcon, promptInstance);
		}

		public void RemoveItemIconPrompt(Item3DIcon item3DIcon) {
			if(item3DIcon.Item == null) {
				Log.Error("RemoveItemIconPrompt called but Item is null.");
				return;
			}
			Log.Info("Removing item icon prompt.");
			if(ItemsInRange.ContainsKey(item3DIcon)) {
				Item3DIconPickupScreenInstance!.GetNode<VBoxContainer>("ScrollContainer/PromptContainer").RemoveChild(ItemsInRange[item3DIcon]);
				ItemsInRange[item3DIcon].QueueFree();
				ItemsInRange.Remove(item3DIcon);
			}
			if(ItemsInRange.Count == 0) {
				RemovePickupScreen();
			}
		}
	}
}
