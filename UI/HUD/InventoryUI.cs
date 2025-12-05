using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUI: Control, IInventoryUI {

	private static readonly Logger Log = new(nameof(InventoryUI), enabled: false);
	private Player Player = null!;
	private bool MouseHasItemSlot = false;
	public Inventory Inventory { get; set; } = null!;
	private List<InvSlotUI> InvSlotUIs = new List<InvSlotUI>();
	private int InventorySlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;
	public event Action<string, int>? OnSlotClicked;

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

	public void SetUpInventoryUI() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			Log.Error("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		Inventory = Player.Inventory;
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
			slotInstance.OnSlotClicked += HandleOnSlotClicked;
			InvSlotUIs.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
		UpdateInventoryUI();
	}

	public void HandleOnSlotClicked(int slotIndex) {
		OnSlotClicked?.Invoke(Inventory.Name, slotIndex);
	}

	public void UpdateInventoryUI(){
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
		if(@event is InputEventMouseButton mouseButton && mouseButton.Pressed) {
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
