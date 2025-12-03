using System;
using Components;
using Core;
using Godot;
using SaveSystem;

public partial class Inventory : ISaveable<InventoryData> {
	public event Action OnInventoryChanged;
	public string Name {
		get;
		set {
			if(string.IsNullOrWhiteSpace(value)) {
				return;
			}
			field = value;
		} 
	} = "Inventory";

	public int MaxSlotsRows {
		get;
		set {
			if(value < 1) {
				value = 1;
			}
			MaxSlots = value * MaxSlotsColumns;
			field = value;
		}
	} = 4;

	public int MaxSlotsColumns {
		get;
		set {
			if(value < 1) {
				value = 1;
			}
			MaxSlots = value * MaxSlotsRows;
			field = value;
		}
	} = 8;

	public int MaxSlots { get; private set; } = 32;

	public ItemSlot[] ItemSlots;

	public Inventory() {
		MaxSlotsRows = 4;
		MaxSlotsColumns = 8;
		ItemSlots = new ItemSlot[MaxSlots];
		for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i] = new ItemSlot();
		}
	}

	public Inventory(int rows, int columns) {
		MaxSlotsRows = rows;
		MaxSlotsColumns = columns;
		ItemSlots = new ItemSlot[MaxSlots];
		for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i] = new ItemSlot();
		}
	}

	public int GetIndex(int row, int column) {
		if(row < 0 || column < 0 || row >= MaxSlotsRows || column >= MaxSlotsColumns) {
			return -1;
		}
		return row * MaxSlotsColumns + column;
	}

	public int GetRow(int index) {
		if(index < 0 || index >= MaxSlots) {
			return -1;
		}
		return index / MaxSlotsColumns;
	}

	public int GetColumn(int index) {
		if(index < 0 || index >= MaxSlots) {
			return -1;
		}
		return index % MaxSlotsColumns;
	}

	public bool IsFull() {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(ItemSlots[i].IsEmpty()) {
				return false;
			}
		}
		return true;
	}

	public int GetItemIndex(Item item) {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(!ItemSlots[i].IsEmpty()) {
				if(ItemSlots[i].SameItem(item)) {
					return i;
				}
			}
		}
		return -1;
	}

	public ItemSlot GetItemSlot(int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return new ItemSlot();
		}
		return ItemSlots[index];
	}

	public Item GetItem(int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return null;
		}
		if(ItemSlots[index].IsEmpty()) {
			return new Item();
		}
		return ItemSlots[index].Item;
	}

	public int GetEmptySlotIndex() {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(ItemSlots[i].IsEmpty()) {
				return i;
			}
		}
		return -1;
	}

	public bool IsEmptySlot(int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return false;
		}
		if(ItemSlots[index].IsEmpty()) {
			return true;
		}
		return false;
	}

	public int GetTotalQuantity(Item item) {
		if(GetItemIndex(item) == -1) {
			return 0;
		}
		int totalQuantity = 0;
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(!ItemSlots[i].IsEmpty()) {
				if(ItemSlots[i].SameItem(item)) {
					totalQuantity += ItemSlots[i].Quantity;
				}
			}
		}
		return totalQuantity;
	}

	public ItemSlot AddItem(ItemSlot item, int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return item;
		}
		ItemSlot remainingItem = ItemSlots[index].combineItemSlot(item);
		if(remainingItem.Quantity == 0) {
			OnInventoryChanged?.Invoke();
			return remainingItem;
		}
		int remainingItemSlot = GetEmptySlotIndex();
		if(remainingItemSlot == -1) {
			OnInventoryChanged?.Invoke();
			return remainingItem;
		}
		remainingItem = ItemSlots[remainingItemSlot].combineItemSlot(remainingItem);
		OnInventoryChanged?.Invoke();
		return remainingItem;
	}

	public ItemSlot AddItem(ItemSlot item) {
		if(item.Item == null) return item;
		int index = GetItemIndex(item.Item);
		if(index != -1) {
			return AddItem(item, GetRow(index), GetColumn(index));
		}
		int emptySlotIndex = GetEmptySlotIndex();
		if(emptySlotIndex == -1) {
			return item;
		}
		return AddItem(item, GetRow(emptySlotIndex), GetColumn(emptySlotIndex));
	}

	public bool AddItem(Item item) {
		if(item == null) {
			return false;
		}
		ItemSlot itemSlot = new ItemSlot(item, 1);
		ItemSlot remainingItem = AddItem(itemSlot);
		return !remainingItem.IsEmpty();
	}

	public bool RemoveItem(int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return false;
		}
		ItemSlots[index].ClearSlot();
		OnInventoryChanged?.Invoke();
		return true;
	}


	public bool RemoveItem(int rows, int columns, int quantity) {
		int index = GetIndex(rows, columns);
		if(index == -1) {
			return false;
		}
		if(ItemSlots[index].IsEmpty()) {
			return false;
		}
		if(ItemSlots[index].Quantity < quantity) {
			return false;
		}
		ItemSlots[index].RemoveItem(quantity);
		OnInventoryChanged?.Invoke();
		return true;
	}

	public bool RemoveItem(ItemSlot item) {
		if(item.IsEmpty()) {
			return false;
		}
		int quantityInInventory = GetTotalQuantity(item.Item);
		if(quantityInInventory < item.Quantity) {
			return false;
		}
		int quantityToRemove = item.Quantity;
		while(quantityToRemove > 0) {
			int index = GetItemIndex(item.Item);
			if(index == -1) {
				break;
			}
			int removedQuantity = ItemSlots[index].RemoveItem(quantityToRemove).Quantity;
			quantityToRemove -= removedQuantity;
		}
		OnInventoryChanged?.Invoke();
		return true;
	}

	private ItemSlotData[] SerializeItemSlots() {
		ItemSlotData[] itemSlots = new ItemSlotData[ItemSlots.Length];
		for(int i = 0; i < ItemSlots.Length; i++) {
			itemSlots[i] = ItemSlots[i].Serialize();
		}
		return itemSlots;
	}

	public InventoryData Serialize() => new InventoryData {
		Name = Name,
		MaxSlotsRows = MaxSlotsRows,
		MaxSlotsColumns = MaxSlotsColumns,
		ItemSlots = SerializeItemSlots(),
	};

	public void Deserialize(in InventoryData data) {
		MaxSlotsRows = data.MaxSlotsRows;
		MaxSlotsColumns = data.MaxSlotsColumns;
		ItemSlots = new ItemSlot[MaxSlots];
        for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i].Deserialize(data.ItemSlots[i]);
		}
    }
}

namespace SaveSystem {
	public readonly record struct InventoryData : ISaveData {
		public string Name { get; init; }
		public int MaxSlotsRows { get; init; }
		public int MaxSlotsColumns { get; init; }
        public ItemSlotData[] ItemSlots { get; init; }
    }
}