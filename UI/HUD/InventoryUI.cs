using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUI: Control, IInventoryUI {
	private Player Player = null!;
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
	}

	public void SetUpInventoryUI() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		Inventory = Player.Inventory;
		GridContainer = GetNode<Control>("Background/GridBackground/GridContainer");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		InventorySlots = Inventory.MaxSlotsRows * Inventory.MaxSlotsColumns;
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
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		Inventory = Player.Inventory;
		for(int i = 0; i < InventorySlots; i++) {
			InvSlotUIs[i].UpdateSlotUI(Inventory.ItemSlots[i]);
		}
	}
}
