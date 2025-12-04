using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
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
		PlayerInventory = player.Inventory;
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
		if (MouseHasItemSlot) {
			HandlePlaceItemSlot(slotIndex);
		} else {
			HandlePickupItemSlot(slotIndex);
		}
	}

	public void HandlePickupItemSlot(int slotIndex) {
		if(!MouseHasItemSlot) {
			if(IsItemSlotEmpty(slotIndex)) {
				GD.Print("Clicked on empty slot, nothing to pick up.");
				return;
			}
			GD.Print("Picking up item from slot index: " + slotIndex);
			HeldItemSlot = GetItemSlotCopy(slotIndex);
			HeldItemSlotUI = CreateHeldItemSlotUI();
			HeldItemSlotUI.UpdateSlotUI(HeldItemSlot);
			MouseHasItemSlot = true;
			PlayerInventory.RemoveItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
		}
	}

	public ItemSlot GetItemSlotCopy(int slotIndex) {
		ItemSlot original = PlayerInventory.GetItemSlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
		if(original == null || original.IsEmpty()) {
			GD.PrintErr("Error: Original ItemSlot is null/empty in GetItemSlotCopy");
			return new ItemSlot();
		}
		if (original.Item != null) {
			GD.Print("Getting copy of ItemSlot: " + original.Item.Name + " x" + original.Quantity);
		} else {
			GD.Print("Getting copy of ItemSlot: <no item> x" + original.Quantity);
		}
		ItemSlot copy = new ItemSlot();
		copy.Item = original.Item;
		copy.Quantity = original.Quantity;
		return copy;
	}

	public InvSlotUI CreateHeldItemSlotUI() {
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		InvSlotUI invSlotUI = InvSlotTemplate.Instantiate<InvSlotUI>();
		AddChild(invSlotUI);
		invSlotUI.MouseFilter = Control.MouseFilterEnum.Ignore;
		foreach (var child in invSlotUI.GetChildren()){
			if (child is Control ctrl)
				ctrl.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		invSlotUI.ZIndex = 100;
		invSlotUI.Visible = true;
		var style = new StyleBoxFlat();
		style.BgColor = new Color(1,1,1, 0);
		invSlotUI.AddThemeStyleboxOverride("panel", style);
		return invSlotUI;
	}

	public void HandlePlaceItemSlot(int slotIndex) {
		if (MouseHasItemSlot) {
			if (HeldItemSlot == null) return;
			HandlePlaceItemSlotOnEmptySlot(slotIndex);
			if (!MouseHasItemSlot || HeldItemSlot == null) {
				return;
			}
			HandlePlaceItemSlotOnNonEmptySlot(slotIndex);
		}
	}

	public void HandlePlaceItemSlotOnEmptySlot(int slotIndex) {
		if(IsItemSlotEmpty(slotIndex)) {
			if(HeldItemSlot == null) {
				GD.Print("Error: HeldItemSlot is null in HandlePlaceItemSlotOnEmptySlot");
				return;
			}
			GD.Print("Placing held item into empty slot index: " + slotIndex);
			PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
			MouseHasItemSlot = false;
			HeldItemSlotUI?.QueueFree();
			HeldItemSlotUI = null;
			HeldItemSlot = null;
		}
	}

	public void HandlePlaceItemSlotOnNonEmptySlot(int slotIndex) {
		if(!IsItemSlotEmpty(slotIndex)) {
			if(HeldItemSlot == null) {
				GD.Print("Error: HeldItemSlot is null in HandlePlaceItemSlotOnNonEmptySlot");
				return;
			}
			GD.Print("Placing held item onto non-empty slot index: " + slotIndex);
			HandlePlaceItemSlotOnDifferentItem(slotIndex);
			HandlePlaceItemSlotOnSameItem(slotIndex);
		}
	}

	public void HandlePlaceItemSlotOnDifferentItem(int slotIndex) {
		if (HeldItemSlot == null) return;
		Item targetItem = PlayerInventory.GetItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
		if (targetItem == null) return;
		if(!HeldItemSlot.SameItem(targetItem)) {
			GD.Print("Placing held item onto different item slot index: " + slotIndex);
			ItemSlot tempSlot = GetItemSlotCopy(slotIndex);
			PlayerInventory.RemoveItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
			PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
			HeldItemSlot = tempSlot;
			HeldItemSlotUI?.QueueFree();
			HeldItemSlotUI = null;
			HeldItemSlotUI = CreateHeldItemSlotUI();
			HeldItemSlotUI.UpdateSlotUI(HeldItemSlot);
		}
	}

	public void HandlePlaceItemSlotOnSameItem(int slotIndex) {
		if (HeldItemSlot == null) return;
		Item targetItem = PlayerInventory.GetItem(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
		if (targetItem == null) return;
		if (HeldItemSlot.SameItem(targetItem)) {
			GD.Print("Placing held item onto same item slot index: " + slotIndex);
			ItemSlot remainSlot = PlayerInventory.AddItem(HeldItemSlot, PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
			if(remainSlot.IsEmpty()) {
				GD.Print("All held items placed into slot index: " + slotIndex);
				MouseHasItemSlot = false;
				HeldItemSlotUI?.QueueFree();
				HeldItemSlotUI = null;
				HeldItemSlot = null;
			}
			else {
				GD.Print("Some held items remain after placing into slot index: " + slotIndex);
				HeldItemSlot = remainSlot;
				if(HeldItemSlotUI != null && HeldItemSlot != null) {
					HeldItemSlotUI.UpdateSlotUI(HeldItemSlot);
				}
			}
		}
	}

	public bool IsItemSlotEmpty(int slotIndex) {
		return PlayerInventory.IsEmptySlot(PlayerInventory.GetRow(slotIndex), PlayerInventory.GetColumn(slotIndex));
	}

	public override void _Process(double delta){
		if(MouseHasItemSlot && HeldItemSlotUI != null) {
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector2 half = new Vector2(16, 16);
			HeldItemSlotUI.GlobalPosition = mousePos - half;
		}
	}

	public override void _UnhandledInput(InputEvent @event){
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed){
			Vector2 clickPos = mouseButton.GlobalPosition;
			if (MouseHasItemSlot && !GetGlobalRect().HasPoint(clickPos)){
				//Drop item
			}
		}
	}

	public void updateInventoryUI(){
		player = GetParent<HUD>().Player;
		if(player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerInventory = player.Inventory;
		for(int i = 0; i < InventorySlots; i++) {
			InvSlotUIs[i].UpdateSlotUI(PlayerInventory.ItemSlots[i]);
		}
	}
}
