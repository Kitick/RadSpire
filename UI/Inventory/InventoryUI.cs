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
		void Initialize(Inventory inventory, Player player);
		void SetUpInventoryUI();
		public event Action<string, int, MouseButton>? OnSlotPressed;
		public event Action<string, int, MouseButton>? OnSlotReleased;
		public event Action<string, int>? OnSlotHovered;
		void UpdateInventoryUI();
	}

	public partial class InventoryUI : Control, IInventoryUI {
		private static readonly LogService Log = new(nameof(InventoryUI), enabled: true);

		private Player Player = null!;
		private bool IsReady = false;
		private bool IsInitialized = false;
		private bool IsRegisteredToInventory = false;
		private bool IsSubscribedToMoveEvents = false;
		private bool MouseHasItemSlot = false;
		public Inventory Inventory { get; set; } = null!;
		private List<InvSlotUI> InvSlotUIs = new List<InvSlotUI>();
		private int InventorySlots = 0;
		private PackedScene? InvSlotTemplate = null!;
		private Control? GridContainer = null!;
		private RichTextLabel? Label = null!;

		public event Action<string, int, MouseButton>? OnSlotPressed;
		public event Action<string, int, MouseButton>? OnSlotReleased;
		public event Action<string, int>? OnSlotHovered;

		public InventoryUI() {
		}

		public InventoryUI(Inventory inventory, Player player) {
			Initialize(inventory, player);
		}

		public void Initialize(Inventory inventory, Player player) {
			if(inventory == null) {
				Log.Error("Inventory is null.");
				return;
			}
			if(player == null) {
				Log.Error("Player is null.");
				return;
			}

			if(IsInitialized && Inventory != null && Player != null && Player.InventoryManager != null) {
				if(IsRegisteredToInventory) {
					Inventory.OnInventoryChanged -= UpdateInventoryUI;
					Player.InventoryManager.UnregisterInventory(Inventory.Name);
					IsRegisteredToInventory = false;
				}
			}

			Inventory = inventory;
			Player = player;
			IsInitialized = true;

			if(IsReady) {
				SetUpInventoryUI();
			}
		}

		public override void _Ready() {
			base._Ready();
			IsReady = true;
			if(IsInitialized) {
				SetUpInventoryUI();
			}
		}

		public override void _ExitTree() {
			if(IsRegisteredToInventory && Inventory != null) {
				Inventory.OnInventoryChanged -= UpdateInventoryUI;
				IsRegisteredToInventory = false;
			}
			if(IsSubscribedToMoveEvents && Player != null && Player.InventoryManager != null) {
				Player.InventoryManager.StartMoveItemEvent -= HandleStartMoveItemEvent;
				Player.InventoryManager.EndMoveItemEvent -= HandleEndMoveItemEvent;
				IsSubscribedToMoveEvents = false;
			}
			if(IsInitialized && Player != null && Player.InventoryManager != null && Inventory != null) {
				Player.InventoryManager.UnregisterInventory(Inventory.Name);
			}
		}

		public void SetUpInventoryUI() {
			if(!IsInitialized) {
				return;
			}
			if(!IsReady) {
				return;
			}
			if(Player == null || Inventory == null) {
				Log.Error("Missing required Player or Inventory.");
				return;
			}
			if(!IsRegisteredToInventory) {
				Player.InventoryManager.RegisterInventory(Inventory, this);
				Inventory.OnInventoryChanged += UpdateInventoryUI;
				IsRegisteredToInventory = true;
			}
			if(!IsSubscribedToMoveEvents && Player.InventoryManager != null) {
				Player.InventoryManager.StartMoveItemEvent += HandleStartMoveItemEvent;
				Player.InventoryManager.EndMoveItemEvent += HandleEndMoveItemEvent;
				IsSubscribedToMoveEvents = true;
			}
			if(InvSlotUIs.Count > 0) {
				Label = GetNodeOrNull<RichTextLabel>("TabBackground/Label");
				if(Label != null) {
					Label.Text = Inventory.Name;
				}
				UpdateInventoryUI();
				return;
			}
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
				slotInstance.OnSlotHovered += HandleOnSlotHovered;
				InvSlotUIs.Add(slotInstance);
				GridContainer.AddChild(slotInstance);
			}
			UpdateInventoryUI();
			Label = GetNodeOrNull<RichTextLabel>("TabBackground/Label");
			if(Label != null) {
				Label.Text = Inventory.Name;
			}
		}

		private void HandleStartMoveItemEvent(ItemSlot _) {
			MouseHasItemSlot = true;
		}

		private void HandleEndMoveItemEvent() {
			MouseHasItemSlot = false;
		}

		public void HandleOnSlotPressed(int slotIndex, MouseButton button) {
			Log.Info($"InventoryUI: Slot {slotIndex} pressed.");
			OnSlotPressed?.Invoke(Inventory.Name, slotIndex, button);
		}

		public void HandleOnSlotReleased(int slotIndex, MouseButton button) {
			Log.Info($"InventoryUI: Slot {slotIndex} released.");
			OnSlotReleased?.Invoke(Inventory.Name, slotIndex, button);
		}

		public void HandleOnSlotHovered(int slotIndex) {
			Log.Info($"InventoryUI: Slot {slotIndex} hovered.");
			OnSlotHovered?.Invoke(Inventory.Name, slotIndex);
		}

		public void UpdateInventoryUI() {
			if(!IsInitialized || Inventory == null) {
				return;
			}
			for(int i = 0; i < InventorySlots; i++) {
				InvSlotUIs[i].UpdateSlotUI(Inventory.ItemSlots[i]);
			}
		}

		public void SetLabelText(string text) {
			Label = GetNodeOrNull<RichTextLabel>("TabBackground/Label");
			if(Label != null) {
				Label.Text = text;
			}
		}

		public override void _Input(InputEvent @event) {
			if(@event is InputEventMouseButton mouseButton && !mouseButton.Pressed) {
				Vector2 clickPos = mouseButton.GlobalPosition;
				if(MouseHasItemSlot) {
					// Use InventoryManager to determine whether click is outside all inventory UIs
					if(Player != null && Player.InventoryManager != null) {
						if(Player.InventoryManager.ClickedOutsideInventory(clickPos)) {
							// Only handle left-click drops here; right-click is handled by InventoryManager
							if(mouseButton.ButtonIndex == MouseButton.Left) {
								Player.InventoryManager.DropItemOutside();
							}
						}
					}
				}
			}
		}
	}
}
