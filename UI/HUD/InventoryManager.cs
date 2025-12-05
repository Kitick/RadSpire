using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Godot;

public partial class InventoryManager : Node {
	private static readonly Logger Log = new(nameof(InventoryManager), enabled: true);

	public Dictionary<string, (Inventory, IInventoryUI)> Inventories = new Dictionary<string, (Inventory, IInventoryUI)>();
	public InventoryUIManager InventoryUIManager = null!;
	public PackedScene? InventoryUIManagerTemplate = null!;
	private bool MouseHasItemSlot = false;
	public ItemSlot? HeldItemSlot = null;
	public event Action<ItemSlot>? StartMoveItemEvent;
	public event Action? EndMoveItemEvent;

	public override void _Ready() {
		base._Ready();
		// Ensure this node receives input events even when GUI controls are present
		SetProcessInput(true);
		SetProcessUnhandledInput(true);
		LoadInventoryUIManager();
	}
	
	public void LoadInventoryUIManager() {
		if(InventoryUIManagerTemplate == null) {
			InventoryUIManagerTemplate = GD.Load<PackedScene>("res://UI/Inventory/InventoryUIManager.tscn");
		}
		InventoryUIManager = InventoryUIManagerTemplate.Instantiate<InventoryUIManager>();
		GetParent<Player>().GetNode<HUD>("HUD").GetNode<InventoryUI>("Inventory").AddChild(InventoryUIManager);
	}

	public void RegisterInventory(Inventory inventory, IInventoryUI uiControl) {
		if(inventory == null) {
			Log.Error("RegisterInventory: Inventory is null.");
			return;
		}
		if(Inventories.ContainsKey(inventory.Name)) {
			Log.Error($"RegisterInventory: Inventory with name {inventory.Name} already registered.");
			return;
		}
		Inventories.Add(inventory.Name, (inventory, uiControl));
		uiControl.OnSlotPressed += HandleOnSlotPressed;
		uiControl.OnSlotReleased += HandleOnSlotReleased;
		Log.Info($"Registered inventory: {inventory.Name}");
	}

	public void UnregisterInventory(string name) {
		if(!Inventories.ContainsKey(name)) {
			Log.Error($"UnregisterInventory: Inventory with name {name} not found.");
			return;
		}
		Inventories.Remove(name);
		Log.Info($"Unregistered inventory: {name}");
	}

	public Inventory GetInventory(string name) {
		if(Inventories.ContainsKey(name)) {
			return Inventories[name].Item1;
		}
		return null!;
	}

	public IInventoryUI GetInventoryUI(string name) {
		if(Inventories.ContainsKey(name)) {
			return Inventories[name].Item2;
		}
		return null!;
	}

	public void HandleOnSlotPressed(string inventoryName, int slotIndex) {
		if(!MouseHasItemSlot) {
			Log.Info($"InventoryManager: Slot {slotIndex} pressed in inventory {inventoryName}.");
			HandlePickupItemSlot(inventoryName, slotIndex);
		}
	}

	public void HandleOnSlotReleased(string inventoryName, int slotIndex) {
		if(MouseHasItemSlot) {
			Log.Info($"InventoryManager: Slot {slotIndex} released in inventory {inventoryName}.");
			HandlePlaceItemSlot(inventoryName, slotIndex);
		}
	}

	public void HandlePickupItemSlot(string inventoryName, int slotIndex) {
		if(!MouseHasItemSlot) {
			if(IsItemSlotEmpty(inventoryName, slotIndex)) {
				Log.Info("Clicked on empty slot, nothing to pick up.");
				return;
			}
			Log.Info("Picking up item from inventory:" + inventoryName + " slot index: " + slotIndex);
			HeldItemSlot = GetItemSlotCopy(inventoryName, slotIndex);
			StartMoveItemEvent?.Invoke(HeldItemSlot);
			MouseHasItemSlot = true;
			GetInventory(inventoryName).RemoveItem(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
		}
	}
	
	public bool IsItemSlotEmpty(string inventoryName, int slotIndex) {
		return GetInventory(inventoryName).IsEmptySlot(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
	}

	public ItemSlot GetItemSlotCopy(string inventoryName, int slotIndex) {
		ItemSlot original = GetInventory(inventoryName).GetItemSlot(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
		if(original == null || original.IsEmpty()) {
			Log.Error("Error: Original ItemSlot is null/empty");
			return new ItemSlot();
		}
		Log.Info("Getting copy of ItemSlot: " + original.Item!.Name + " x" + original.Quantity);
		ItemSlot copy = new ItemSlot();
		copy.Item = original.Item.Copy();
		copy.Quantity = original.Quantity;
		return copy;
	}

	public void HandlePlaceItemSlot(string inventoryName, int slotIndex) {
		if (MouseHasItemSlot) {
			if (HeldItemSlot == null) return;
			HandlePlaceItemSlotOnEmptySlot(inventoryName, slotIndex);
			if (!MouseHasItemSlot || HeldItemSlot == null) {
				return;
			}
			HandlePlaceItemSlotOnNonEmptySlot(inventoryName, slotIndex);
		}
	}

	public void HandlePlaceItemSlotOnEmptySlot(string inventoryName, int slotIndex) {
		if(IsItemSlotEmpty(inventoryName, slotIndex)) {
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in HandlePlaceItemSlotOnEmptySlot");
				return;
			}
			Log.Info("Placing held item into empty slot inventory:" + inventoryName + " index: " + slotIndex);
			GetInventory(inventoryName).AddItem(HeldItemSlot, GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
			EndMoveItemEvent?.Invoke();
			MouseHasItemSlot = false;
			HeldItemSlot = null;
		}
	}
	
	public void HandlePlaceItemSlotOnNonEmptySlot(string inventoryName, int slotIndex) {
		if(!IsItemSlotEmpty(inventoryName, slotIndex)) {
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in HandlePlaceItemSlotOnNonEmptySlot");
				return;
			}
			Log.Info("Placing held item onto non-empty slot inventory:" + inventoryName + " index: " + slotIndex);
			HandlePlaceItemSlotOnDifferentItem(inventoryName, slotIndex);
			HandlePlaceItemSlotOnSameItem(inventoryName, slotIndex);
		}
	}

	public void HandlePlaceItemSlotOnDifferentItem(string inventoryName, int slotIndex) {
		if (HeldItemSlot == null) return;
		Item targetItem = GetInventory(inventoryName).GetItem(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
		if (targetItem == null) return;
		if(!HeldItemSlot.SameItem(targetItem)) {
			Log.Info("Placing held item onto different item type slot inventory:" + inventoryName + " index: " + slotIndex);
			ItemSlot tempSlot = GetItemSlotCopy(inventoryName, slotIndex);
			GetInventory(inventoryName).RemoveItem(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
			GetInventory(inventoryName).AddItem(HeldItemSlot, GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
			HeldItemSlot = tempSlot;
			EndMoveItemEvent?.Invoke();
			StartMoveItemEvent?.Invoke(HeldItemSlot);
		}
	}

	public void HandlePlaceItemSlotOnSameItem(string inventoryName, int slotIndex) {
		if(HeldItemSlot == null) return;
		Item targetItem = GetInventory(inventoryName).GetItem(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
		if(targetItem == null) return;
		if(HeldItemSlot.SameItem(targetItem)) {
			Log.Info("Placing held item onto same item type slot inventory:" + inventoryName + " index: " + slotIndex);
			ItemSlot remainSlot = GetInventory(inventoryName).AddItem(HeldItemSlot, GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
			if(remainSlot.IsEmpty()) {
				Log.Info("All held items placed into slot inventory:" + inventoryName + " index: " + slotIndex);
				EndMoveItemEvent?.Invoke();
				MouseHasItemSlot = false;
				HeldItemSlot = null;
			}
			else {
				Log.Info("Some held items remain after placing into slot inventory:" + inventoryName + " index: " + slotIndex);
				HeldItemSlot = remainSlot;
				EndMoveItemEvent?.Invoke();
				StartMoveItemEvent?.Invoke(HeldItemSlot);
			}
		}
	}
	
	public override void _Input(InputEvent @event) {
		if(@event is InputEventMouseButton mouseButton && !mouseButton.Pressed) {
			if(MouseHasItemSlot) {
				Vector2 clickPos = mouseButton.GlobalPosition;
				if(ClickedOutsideInventory(clickPos)) {
					Log.Info("Clicked outside inventory, dropping held item.");
					DropItemOutside();
				}
			}
		}
		if(@event.IsActionPressed("DropItem")) {
			HandleItemDropWithKeyboard();
		}
		if(@event.IsActionPressed("Consume")) {
			HandleConsumeItem();
		}
	}

	public bool ClickedOutsideInventory(Vector2 clickPosition) {
		foreach(var inventory in Inventories.Keys) {
			Log.Info("Inventory UI global rect: " + GetInventoryUI(inventory).GetGlobalRect());
			if(GetInventoryUI(inventory).GetGlobalRect().HasPoint(clickPosition)) {
				return false;
			}
		}
		return true;
	}

	public void DropItemOutside() {
		if(MouseHasItemSlot) {
			Log.Info("Dropping held item into the world.");
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in DropItem");
				return;
			}
			DropItemSlot(HeldItemSlot);
			EndMoveItemEvent?.Invoke();
			MouseHasItemSlot = false;
			HeldItemSlot = null;
		}
	}

	public void DropItemSlot(ItemSlot itemSlot) {
		for(int i = 0; i < itemSlot.Quantity; i++) {
			DropItem(itemSlot.Item!);
			itemSlot.Quantity--;
		}
	}

	public void DropItem(Item item) {
		Vector3 dropPosition = GetParent<Player>().GlobalPosition + GetParent<Player>().GlobalTransform.Basis.Z * 2 + Vector3.Up;
		Item3DIcon droppedItemIcon = new Item3DIcon();
		droppedItemIcon.Item = item;
		droppedItemIcon.SpawnItem3D(dropPosition);
		GetParent<Player>().GetParent().AddChild(droppedItemIcon);
	}

	public void HandleItemDropWithKeyboard() {
		Hotbar hotbar = null!;
		Inventory hotbarInventory = null!;
		foreach(string inventoryName in Inventories.Keys) {
			if(GetInventoryUI(inventoryName) is Hotbar currentHotbar) {
				hotbar = currentHotbar;
				hotbarInventory = GetInventory(inventoryName);
				break;
			}
		}
		if(hotbar == null) {
			Log.Error("No Hotbar inventory found for dropping item.");
			return;
		}
		ItemSlot selectedSlot = hotbar.GetSelectedItemSlot();
		if(selectedSlot.IsEmpty()) {
			Log.Info("No item in selected hotbar slot to drop.");
			return;
		}
		int selectedIndex = hotbar.SelectedSlot;
		Log.Info("Dropping item from hotbar slot index: " + selectedIndex);
		hotbarInventory.RemoveItem(hotbar.Inventory.GetRow(selectedIndex), hotbar.Inventory.GetColumn(selectedIndex), 1);
		DropItem(selectedSlot.Item!);
	}
	
	public void HandleConsumeItem() {
		Log.Info("Handling consume item action.");
		Hotbar hotbar = null!;
		Inventory hotbarInventory = null!;
		foreach(string inventoryName in Inventories.Keys) {
			if(GetInventoryUI(inventoryName) is Hotbar currentHotbar) {
				hotbar = currentHotbar;
				hotbarInventory = GetInventory(inventoryName);
				break;
			}
		}
		if(hotbar == null) {
			Log.Error("No Hotbar inventory found for dropping item.");
			return;
		}
		ItemSlot selectedSlot = hotbar.GetSelectedItemSlot();
		if(selectedSlot.IsEmpty()) {
			Log.Info("No item in selected hotbar slot to drop.");
			return;
		}
		int selectedIndex = hotbar.SelectedSlot;
		if(selectedSlot.Item!.OnConsume(GetParent<Player>())) {
			hotbarInventory.RemoveItem(hotbar.Inventory.GetRow(selectedIndex), hotbar.Inventory.GetColumn(selectedIndex), 1);
		}
	} 
}
