namespace InventorySystem;

using System;
using System.Collections.Generic;
using Character;
using GameWorld;
using Godot;
using InventorySystem.Interface;
using ItemSystem;
using ItemSystem.Icons;
using Services;
using UI.HUD;

public partial class InventoryManager : Node {
	private static readonly LogService Log = new(nameof(InventoryManager), enabled: true);

	public Dictionary<string, (Inventory, IInventoryUI)> Inventories = new Dictionary<string, (Inventory, IInventoryUI)>();
	public InventoryUIManager InventoryUIManager = null!;
	public PackedScene? InventoryUIManagerTemplate = null!;
	private bool MouseHasItemSlot = false;
	public ItemSlot? HeldItemSlot = null;
	private string? LastPickupInventoryName = null;
	private int LastPickupSlotIndex = -1;
	private bool IgnoreNextRightRelease = false;
	public event Action<ItemSlot>? StartMoveItemEvent;
	public event Action? EndMoveItemEvent;
	public bool InventoryUIOpen = false;
	public event Action<ItemSlot>? ItemSlotHovered;
	public event Action<Item, Vector3>? SpawnItem3DIconRequested;

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
		if(InventoryUIManager != null && IsInstanceValid(InventoryUIManager)) {
			return;
		}
		var player = GetParent<Player>();
		var gameManager = player?.GetParent<GameManager>();
		var hud = gameManager?.GetNodeOrNull<HUD>("HUD");
		var inventoryUi = hud?.GetNodeOrNull<InventoryUI>("Inventory");
		if(hud == null || inventoryUi == null) {
			CallDeferred(nameof(LoadInventoryUIManager));
			return;
		}
		InventoryUIManager = InventoryUIManagerTemplate.Instantiate<InventoryUIManager>();
		inventoryUi.AddChild(InventoryUIManager);
		hud.InventoryRequested += OnInventoryRequested;
		hud.InventoryItemInformationUI.InventoryManager = this;
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
		uiControl.OnSlotHovered += OnItemSlotHovered;
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

	public void OnInventoryRequested(bool open) {
		InventoryUIOpen = open;
		if(open) {
			if(GetInventoryUI("Hotbar") is Hotbar hotbar) {
				ItemSlot temp = hotbar.GetSelectedItemSlot();
				if(!temp.IsEmpty()) {
					ItemSlotHovered?.Invoke(temp);
				}
			}
		}
		Log.Info($"Inventory UI open: {InventoryUIOpen}");
	}

	public void HandleOnSlotPressed(string inventoryName, int slotIndex, MouseButton button) {
		if(InventoryUIOpen == false) {
			Log.Info("Inventory UI is not open, ignoring slot press.");
			return;
		}
		if(!MouseHasItemSlot) {
			Log.Info($"InventoryManager: Slot {slotIndex} pressed in inventory {inventoryName}.");
			if(button == MouseButton.Right) {
				HandlePickupHalfItemSlot(inventoryName, slotIndex);
			}
			else if(button == MouseButton.Left) {
				HandlePickupItemSlot(inventoryName, slotIndex);
			}
		}
	}

	public void HandleOnSlotReleased(string inventoryName, int slotIndex, MouseButton button) {
		if(InventoryUIOpen == false) {
			Log.Info("Inventory UI is not open, ignoring slot release.");
			return;
		}
		if(MouseHasItemSlot) {
			Log.Info($"InventoryManager: Slot {slotIndex} released in inventory {inventoryName}.");
			if(button == MouseButton.Right) {
				if(IgnoreNextRightRelease == true && inventoryName == LastPickupInventoryName && slotIndex == LastPickupSlotIndex) {
					IgnoreNextRightRelease = false;
					return;
				}
				IgnoreNextRightRelease = false;
				HandlePlaceSingleItemSlot(inventoryName, slotIndex);
			}
			else if(button == MouseButton.Left) {
				HandlePlaceItemSlot(inventoryName, slotIndex);
			}
		}
	}

	public void HandlePickupItemSlot(string inventoryName, int slotIndex) {
		if(!MouseHasItemSlot) {
			if(IsItemSlotEmpty(inventoryName, slotIndex)) {
				Log.Info("Clicked on empty slot, nothing to pick up.");
				return;
			}
			Log.Info("Picking up item from inventory:" + inventoryName + " slot index: " + slotIndex);

			MouseHasItemSlot = true;
			int row = GetInventory(inventoryName).GetRow(slotIndex);
			int column = GetInventory(inventoryName).GetColumn(slotIndex);
			ItemSlot slot = GetInventory(inventoryName).GetItemSlot(row, column);
			HeldItemSlot = new ItemSlot(slot.Item!, slot.Quantity);
			GetInventory(inventoryName).RemoveItem(row, column);
			StartMoveItemEvent?.Invoke(HeldItemSlot);
		}
	}

	public void HandlePickupHalfItemSlot(string inventoryName, int slotIndex) {
		if(!MouseHasItemSlot) {
			if(IsItemSlotEmpty(inventoryName, slotIndex)) {
				Log.Info("Clicked on empty slot, nothing to pick up.");
				return;
			}
			Log.Info("Picking up half of item stack from inventory:" + inventoryName + " slot index: " + slotIndex);

			MouseHasItemSlot = true;
			LastPickupInventoryName = inventoryName;
			LastPickupSlotIndex = slotIndex;
			IgnoreNextRightRelease = true;
			int row = GetInventory(inventoryName).GetRow(slotIndex);
			int column = GetInventory(inventoryName).GetColumn(slotIndex);
			ItemSlot slot = GetInventory(inventoryName).GetItemSlot(row, column);
			if(slot.Quantity == 1) {
				MouseHasItemSlot = false;
				HandlePickupItemSlot(inventoryName, slotIndex);
				return;
			}
			else {
				HeldItemSlot = new ItemSlot(slot.Item!, slot.Quantity / 2);
				GetInventory(inventoryName).RemoveItem(row, column, slot.Quantity / 2);
			}
			StartMoveItemEvent?.Invoke(HeldItemSlot);
		}
	}

	public bool IsItemSlotEmpty(string inventoryName, int slotIndex) {
		return GetInventory(inventoryName).IsEmptySlot(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
	}

	public void HandlePlaceItemSlot(string inventoryName, int slotIndex) {
		if(MouseHasItemSlot) {
			if(HeldItemSlot == null) return;
			HandlePlaceItemSlotOnEmptySlot(inventoryName, slotIndex);
			if(!MouseHasItemSlot || HeldItemSlot == null) {
				return;
			}
			HandlePlaceItemSlotOnNonEmptySlot(inventoryName, slotIndex);
		}
	}

	public void HandlePlaceSingleItemSlot(string inventoryName, int slotIndex) {
		if(!MouseHasItemSlot || HeldItemSlot == null) {
			return;
		}
		int row = GetInventory(inventoryName).GetRow(slotIndex);
		int column = GetInventory(inventoryName).GetColumn(slotIndex);
		ItemSlot targetSlot = GetInventory(inventoryName).GetItemSlot(row, column);

		if(targetSlot.IsEmpty()) {
			PlaceSingleItem(inventoryName, row, column);
			return;
		}
		if(targetSlot.Item == null) {
			return;
		}
		if(!HeldItemSlot.SameItem(targetSlot.Item)) {
			return;
		}
		if(!targetSlot.Item.IsStackable || targetSlot.Quantity >= targetSlot.Item.MaxStackSize) {
			return;
		}
		PlaceSingleItem(inventoryName, row, column);
	}

	private void PlaceSingleItem(string inventoryName, int row, int column) {
		if(HeldItemSlot == null) {
			return;
		}
		Log.Info("Placing held single item into slot inventory:" + inventoryName + " row: " + row + " column: " + column);
		ItemSlot temp = new ItemSlot(HeldItemSlot.Item!, 1);
		ItemSlot remainSlot = GetInventory(inventoryName).AddItem(temp, row, column);
		if(remainSlot.IsEmpty()) {
			HeldItemSlot.Quantity -= 1;
		}
		EndMoveItemEvent?.Invoke();
		if(HeldItemSlot.Quantity <= 0) {
			MouseHasItemSlot = false;
			HeldItemSlot = null;
		}
		else {
			StartMoveItemEvent?.Invoke(HeldItemSlot);
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

	public void HandlePlaceSingleItemSlotOnEmptySlot(string inventoryName, int slotIndex) {
		if(IsItemSlotEmpty(inventoryName, slotIndex)) {
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in HandlePlaceItemSlotOnEmptySlot");
				return;
			}
			Log.Info("Placing held single item into empty slot inventory:" + inventoryName + " index: " + slotIndex);
			ItemSlot temp = new ItemSlot(HeldItemSlot.Item!, 1);
			GetInventory(inventoryName).AddItem(temp, GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
			HeldItemSlot.Quantity -= 1;
			EndMoveItemEvent?.Invoke();
			if(HeldItemSlot.Quantity <= 0) {
				MouseHasItemSlot = false;
				HeldItemSlot = null;
			}
			else {
				StartMoveItemEvent?.Invoke(HeldItemSlot);
			}
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

	public void HandlePlaceSingleItemSlotOnNonEmptySlot(string inventoryName, int slotIndex) {
		if(!IsItemSlotEmpty(inventoryName, slotIndex)) {
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in HandlePlaceItemSlotOnNonEmptySlot");
				return;
			}
			Log.Info("Placing held single item onto non-empty slot inventory:" + inventoryName + " index: " + slotIndex);
			HandlePlaceSingleItemSlotOnSameItem(inventoryName, slotIndex);
		}
	}

	public void HandlePlaceItemSlotOnDifferentItem(string inventoryName, int slotIndex) {
		if(HeldItemSlot == null) return;
		int row = GetInventory(inventoryName).GetRow(slotIndex);
		int column = GetInventory(inventoryName).GetColumn(slotIndex);
		Item targetItem = GetInventory(inventoryName).GetItem(row, column);
		if(targetItem == null) return;
		if(!HeldItemSlot.SameItem(targetItem)) {
			Log.Info("Placing held item onto different item type slot inventory:" + inventoryName + " index: " + slotIndex);
			ItemSlot targetSlot = GetInventory(inventoryName).GetItemSlot(row, column);
			ItemSlot newHeldSlot = new ItemSlot(targetSlot.Item!, targetSlot.Quantity);

			GetInventory(inventoryName).RemoveItem(row, column);
			GetInventory(inventoryName).AddItem(HeldItemSlot, row, column);

			HeldItemSlot = newHeldSlot;
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

	public void HandlePlaceSingleItemSlotOnSameItem(string inventoryName, int slotIndex) {
		if(HeldItemSlot == null) return;
		int row = GetInventory(inventoryName).GetRow(slotIndex);
		int column = GetInventory(inventoryName).GetColumn(slotIndex);
		ItemSlot targetSlot = GetInventory(inventoryName).GetItemSlot(row, column);
		if(targetSlot.Item == null) return;
		if(HeldItemSlot.SameItem(targetSlot.Item)) {
			if(!targetSlot.Item.IsStackable || targetSlot.Quantity >= targetSlot.Item.MaxStackSize) {
				return;
			}
			Log.Info("Placing held single item onto same item type slot inventory:" + inventoryName + " index: " + slotIndex);
			ItemSlot temp = new ItemSlot(HeldItemSlot.Item!, 1);
			ItemSlot remainSlot = GetInventory(inventoryName).AddItem(temp, row, column);
			if(remainSlot.IsEmpty()) {
				HeldItemSlot.Quantity -= 1;
			}
			EndMoveItemEvent?.Invoke();
			if(HeldItemSlot.Quantity <= 0) {
				MouseHasItemSlot = false;
				HeldItemSlot = null;
			}
			else {
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
					if(mouseButton.ButtonIndex == MouseButton.Right) {
						Log.Info("Right click outside inventory detected, dropping one item.");
						if(IgnoreNextRightRelease == true) {
							IgnoreNextRightRelease = false;
							return;
						}
						DropSingleItemOutside();
					}
					else if(mouseButton.ButtonIndex == MouseButton.Left) {
						Log.Info("Dropping item outside due to left click.");
						DropItemOutside();
					}
				}
			}
		}
		if(@event.IsActionPressed("DropItem")) {
			HandleItemDropWithKeyboard();
		}
		if(@event.IsActionPressed("Consume")) {

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

	public void DropSingleItemOutside() {
		if(MouseHasItemSlot) {
			Log.Info("Dropping single held item into the world.");
			if(HeldItemSlot == null) {
				Log.Error("HeldItemSlot is null in DropSingleItemOutside");
				return;
			}
			DropItem(HeldItemSlot.Item!);
			HeldItemSlot.Quantity -= 1;
			EndMoveItemEvent?.Invoke();
			if(HeldItemSlot.Quantity <= 0) {
				MouseHasItemSlot = false;
				HeldItemSlot = null;
			}
			else {
				StartMoveItemEvent?.Invoke(HeldItemSlot);
			}
		}
	}

	public void DropItemSlot(ItemSlot itemSlot) {
		int quantity = itemSlot.Quantity;
		for(int i = 0; i < quantity; i++) {
			DropItem(itemSlot.Item!);
		}
		itemSlot.Quantity = 0;
	}

	public void DropItem(Item item) {
		Vector3 dropPosition = GetParent<Player>().GlobalPosition + GetParent<Player>().GlobalTransform.Basis.Z * 2 + Vector3.Up;
		if(SpawnItem3DIconRequested != null) {
			SpawnItem3DIconRequested.Invoke(item, dropPosition);
			return;
		}

		Item3DIcon droppedItemIcon = new Item3DIcon();
		droppedItemIcon.Item = item;
		GetParent<Player>().GetParent().AddChild(droppedItemIcon);
		droppedItemIcon.SpawnItem3D(dropPosition);
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

	public bool ConsumeSelectedHotbar(Hotbar hotbar, int amount) {
		if(hotbar == null) {
			Log.Error("ConsumeSelectedHotbar: Hotbar is null.");
			return false;
		}
		int selectedIndex = hotbar.SelectedSlot;
		int row = hotbar.Inventory.GetRow(selectedIndex);
		int column = hotbar.Inventory.GetColumn(selectedIndex);
		if(hotbar.Inventory.IsEmptySlot(row, column)) {
			Log.Info("ConsumeSelectedHotbar: Selected slot is empty.");
			return false;
		}
		return hotbar.Inventory.RemoveItem(row, column, amount);
	}

	public void OnItemSlotHovered(string inventoryName, int slotIndex) {
		Log.Info($"InventoryManager: Slot {slotIndex} hovered in inventory {inventoryName}.");
		if(GetInventory(inventoryName).IsEmptySlot(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex))) {
			return;
		}
		ItemSlot itemSlot = GetInventory(inventoryName).GetItemSlot(GetInventory(inventoryName).GetRow(slotIndex), GetInventory(inventoryName).GetColumn(slotIndex));
		ItemSlotHovered?.Invoke(itemSlot);
	}

	public ItemSlot AddItemSlotToInventory(string inventoryName, ItemSlot itemSlot) {
		if(itemSlot.IsEmpty()) {
			Log.Info("AddItemSlotToInventory called with empty item slot, nothing to add.");
			return itemSlot;
		}
		if(!Inventories.ContainsKey(inventoryName)) {
			Log.Error($"AddItemSlotToInventory: Inventory with name {inventoryName} not found.");
			return itemSlot;
		}
		return GetInventory(inventoryName).AddItem(itemSlot);
	}

	public ItemSlot AddItemSlotToPlayerInventory(ItemSlot itemSlot) {
		ItemSlot remainSlot = itemSlot;
		remainSlot = AddItemSlotToInventory("Hotbar", remainSlot);
		remainSlot = AddItemSlotToInventory("Inventory", remainSlot);
		return remainSlot;
	}
}
