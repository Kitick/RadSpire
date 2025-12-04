using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUI: Control, IInventoryUI {
	private Player Player = null!;
	private Inventory PlayerInventory = null!;
	private List<InvSlotUI> InvSlotUIs = new List<InvSlotUI>();
	private int InventorySlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;
	public event Action<int>? OnSlotClicked;

	public override void _Ready() {
		base._Ready();
		SetUpInventoryUI();
		PlayerInventory.OnInventoryChanged += UpdateInventoryUI;
	}

	public void SetUpInventoryUI() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerInventory = Player.Inventory;
		GridContainer = GetNode<Control>("Background/GridBackground/GridContainer");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		InventorySlots = PlayerInventory.MaxSlotsRows * PlayerInventory.MaxSlotsColumns;
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

	}

	public void UpdateInventoryUI(){
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerInventory = Player.Inventory;
		for(int i = 0; i < InventorySlots; i++) {
			InvSlotUIs[i].UpdateSlotUI(PlayerInventory.ItemSlots[i]);
		}
	}
}
