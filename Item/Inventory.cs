using System;
using Components;
using Core;
using SaveSystem;

public partial class Inventory : ISaveable<InventoryData> {
	public string Name {
		get;
		set;
	}

	public int MaxSlotsRows {
		get;
		set {
			if(value < 1) {
				value = 1;
			}
			MaxSlots = value * MaxSlotsColumns;
			field = value;
		}
	}
	
	public int MaxSlotsColumns {
		get;
		set {
			if(value < 1) {
				value = 1;
			}
			MaxSlots = value * MaxSlotsRows;
			field = value;
		}
	}

	public int MaxSlots {
		get;
		private set;
    }

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

	public int GetItemIndex(string itemId) {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(!ItemSlots[i].IsEmpty() && ItemSlots[i].Item != null && ItemSlots[i].Item.Data != null) {
				if(ItemSlots[i].ContainsItem(itemId)) {
					return i;
				}
			}
		}
		return -1;
	}

	public int GetEmptySlotIndex() {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(ItemSlots[i].IsEmpty()) {
				return i;
			}
		}
		return -1;
	}

	public int GetTotalQuantity(string itemId) {
		if(GetItemIndex(itemId) == -1) {
			return 0;
		}
		int totalQuantity = 0;
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(!ItemSlots[i].IsEmpty() && ItemSlots[i].Item != null && ItemSlots[i].Item.Data != null) {
				if(ItemSlots[i].ContainsItem(itemId)) {
					totalQuantity += ItemSlots[i].Item.Quantity;
				}
			}
		}
		return totalQuantity;
	}

	public Item AddItem(Item item, int row, int column) {
		int index = GetIndex(row, column);
		if(index == -1) {
			return item;
		}
		ItemSlot slot = ItemSlots[index];
		Item remainingItem = slot.AddItem(item);
		if(remainingItem.Quantity == 0) {
			return remainingItem;
		}
		int remainingItemSlot = GetEmptySlotIndex();
		if(remainingItemSlot == -1) {
			return remainingItem;
		}
		remainingItem = ItemSlots[remainingItemSlot].AddItem(remainingItem);
		return remainingItem;
	}

	public Item AddItem(Item item) {
		int index = GetItemIndex(item.Data.Id);
		if(index != -1) {
			return AddItem(item, GetRow(index), GetColumn(index));
		}
		int emptySlotIndex = GetEmptySlotIndex();
		if(emptySlotIndex == -1) {
			return item;
		}
		return AddItem(item, GetRow(emptySlotIndex), GetColumn(emptySlotIndex));
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
		MaxSlots = MaxSlots,
		ItemSlots = SerializeItemSlots(),
	};

	public void Deserialize(in InventoryData data) {
		MaxSlotsRows = data.MaxSlotsRows;
		MaxSlotsColumns = data.MaxSlotsColumns;
		MaxSlots = data.MaxSlots;
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
		public int MaxSlots { get; init; }
        public ItemSlotData[] ItemSlots { get; init; }
    }
}