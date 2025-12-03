using System;
using System.Collections.Generic;
using Godot; 

public partial class InventoryUI: Control {
	private Player player = null!;
	private Inventory PlayerInventory = null!;
	private List<InvSlotUI> InvSlotUIs = new List<InvSlotUI>();
	private int InventorySlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;

	public override void _Ready() {
		base._Ready();
		SetUpInventoryUI();
		PlayerInventory.OnInventoryChanged += updateInventoryUI;
	}

	public void SetUpInventoryUI() {
		player = GetParent<HUD>().Player;
		if(player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerInventory = player.PlayerInventory;
		GridContainer = GetNode<Control>("Background/GridBackground/GridContainer");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		InventorySlots = (PlayerInventory.MaxSlotsRows - 1) * PlayerInventory.MaxSlotsColumns;
		for(int i = 0; i < InventorySlots; i++) {
			InvSlotUI slotInstance = InvSlotTemplate.Instantiate<InvSlotUI>();
			slotInstance.SlotIndex = i;
			InvSlotUIs.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
		updateInventoryUI();
	}
	
	public void HandleOnSlotClicked(int slotIndex) {
		
	}

	public void updateInventoryUI(){
		player = GetParent<HUD>().Player;
		if(player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerInventory = player.PlayerInventory;
		for(int i = 0; i < InventorySlots; i++) {
			InvSlotUIs[i].UpdateSlotUI(PlayerInventory.ItemSlots[i]);
		}
	}
}
