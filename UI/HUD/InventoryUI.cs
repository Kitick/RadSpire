using System;
using System.Collections.Generic;
using Godot;
using Services;
using Character;
using ItemSystem;

namespace UI {
	public interface IInventoryUI {
		Inventory Inventory { get; set; }
		Rect2 GetGlobalRect();
		void SetUpInventoryUI();
		public event Action<string, int, MouseButton>? OnSlotPressed;
		public event Action<string, int, MouseButton>? OnSlotReleased;
		void UpdateInventoryUI();
	}

	public partial class InventoryUI : Control, IInventoryUI {
		private static readonly LogService Log = new(nameof(InventoryUI), enabled: true);

		private Player Player = null!;
		private bool MouseHasItemSlot = false;
		public Inventory Inventory { get; set; } = null!;
		private List<InvSlotUI> InvSlotUIs = new List<InvSlotUI>();
		private int InventorySlots = 0;
		private PackedScene? InvSlotTemplate = null!;
		private Control? GridContainer = null!;

		public event Action<string, int, MouseButton>? OnSlotPressed;
		public event Action<string, int, MouseButton>? OnSlotReleased;

		public override void _Ready() {
			base._Ready();
			SetUpInventoryUI();
			Inventory.OnInventoryChanged += UpdateInventoryUI;
			// Track when an item is being moved so UI can forward outside clicks to drop
			if(Player != null && Player.InventoryManager != null) {
				Player.InventoryManager.StartMoveItemEvent += (_) => MouseHasItemSlot = true;
				Player.InventoryManager.EndMoveItemEvent += () => MouseHasItemSlot = false;
			}
		}

		public override void _ExitTree() {
			if(Player != null && Player.InventoryManager != null) {
				Player.InventoryManager.UnregisterInventory(Inventory.Name);
			}
		}

		public void SetUpInventoryUI() {
			Player = GetParent<HUD>().Player;
			if(Player == null) {
				Log.Error("InventoryUI SetUpInventoryUI: Player is null.");
				return;
			}
			Inventory = Player.Inventory;
			Player.InventoryManager.RegisterInventory(Inventory, this);
			GridContainer = GetNode<Control>("Background/GridBackground/GridContainer");
			// Allow clicks to pass through non-interactive background so Hotbar can receive them
			var background = GetNodeOrNull<Control>("Background");
			if(background != null) {
				background.MouseFilter = Control.MouseFilterEnum.Pass;
			}
			var gridBackground = GetNodeOrNull<Control>("Background/GridBackground");
			if(gridBackground != null) {
				gridBackground.MouseFilter = Control.MouseFilterEnum.Pass;
			}
			if(InvSlotTemplate == null) {
				InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
			}
			InventorySlots = Inventory.MaxRows * Inventory.MaxColumns;
			for(int i = 0; i < InventorySlots; i++) {
				InvSlotUI slotInstance = InvSlotTemplate.Instantiate<InvSlotUI>();
				slotInstance.SlotIndex = i;
				slotInstance.OnSlotPressed += HandleOnSlotPressed;
				slotInstance.OnSlotReleased += HandleOnSlotReleased;
				InvSlotUIs.Add(slotInstance);
				GridContainer.AddChild(slotInstance);
			}
			UpdateInventoryUI();
		}

		public void HandleOnSlotPressed(int slotIndex, MouseButton button) {
			Log.Info($"InventoryUI: Slot {slotIndex} pressed.");
			OnSlotPressed?.Invoke(Inventory.Name, slotIndex, button);
		}

		public void HandleOnSlotReleased(int slotIndex, MouseButton button) {
			Log.Info($"InventoryUI: Slot {slotIndex} released.");
			OnSlotReleased?.Invoke(Inventory.Name, slotIndex, button);
		}

		public void UpdateInventoryUI() {
			Player = GetParent<HUD>().Player;
			if(Player == null) {
				Log.Error("InventoryUI SetUpInventoryUI: Player is null.");
				return;
			}
			Inventory = Player.Inventory;
			for(int i = 0; i < InventorySlots; i++) {
				InvSlotUIs[i].UpdateSlotUI(Inventory.ItemSlots[i]);
			}
		}

		public override void _Input(InputEvent @event) {
			if(@event is InputEventMouseButton mouseButton && !mouseButton.Pressed) {
				Vector2 clickPos = mouseButton.GlobalPosition;
				if(MouseHasItemSlot) {
					// Use InventoryManager to determine whether click is outside all inventory UIs
					if(Player != null && Player.InventoryManager != null) {
						if(Player.InventoryManager.ClickedOutsideInventory(clickPos)) {
							Player.InventoryManager.DropItemOutside();
						}
					}
				}
			}
		}
	}
}
