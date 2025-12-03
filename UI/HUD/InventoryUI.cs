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
	private bool MouseHasItemSlot = false;
	private InvSlotUI? HeldItemSlotUI = null;
	private ItemSlot? HeldItemSlot = null;

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
			slotInstance.OnSlotClicked += HandleOnSlotClicked;
			InvSlotUIs.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
		updateInventoryUI();
	}

	public void HandleOnSlotClicked(int slotIndex) {
		if(!MouseHasItemSlot) {
			if(PlayerInventory.IsEmptySlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex))) {
				return;
			}
			HeldItemSlotUI = new InvSlotUI();
			HeldItemSlotUI.UpdateSlotUI(PlayerInventory.GetItemSlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex)));
			AddChild(HeldItemSlotUI);
			MouseHasItemSlot = true;
			HeldItemSlot = new ItemSlot();
			HeldItemSlot = PlayerInventory.GetItemSlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
			PlayerInventory.RemoveItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
		}
		else {
			if(PlayerInventory.IsEmptySlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex))) {
				PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
				MouseHasItemSlot = false;
				HeldItemSlotUI.QueueFree();
				HeldItemSlotUI = null;
				HeldItemSlot = null;
			}
			else if(!HeldItemSlot.SameItem(PlayerInventory.GetItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex)))) {
				ItemSlot tempSlot = new ItemSlot();
				tempSlot = PlayerInventory.GetItemSlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
				PlayerInventory.RemoveItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
				PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
				HeldItemSlot = tempSlot;
				HeldItemSlotUI.UpdateSlotUI(HeldItemSlot);
			}
			else {
				ItemSlot remainSlot = PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
				if(remainSlot.IsEmpty()) {
					MouseHasItemSlot = false;
					HeldItemSlotUI.QueueFree();
					HeldItemSlotUI = null;
					HeldItemSlot = null;
				}
                else {
					HeldItemSlot = remainSlot;
					HeldItemSlotUI.UpdateSlotUI(HeldItemSlot);
                }
			}
		}
	}

	public override void _Process(double delta){
		if(MouseHasItemSlot && HeldItemSlotUI != null) {
			Vector2 mousePos = GetViewport().GetMousePosition();
			HeldItemSlotUI.GlobalPosition = mousePos;
		}
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			Vector2 clickPos = mouseButton.GlobalPosition;
			if (!GetGlobalRect().HasPoint(clickPos))
			{
				GD.Print("Clicked outside InventoryUI");
			}
		}
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
