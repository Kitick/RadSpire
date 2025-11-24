using System;
using System.Runtime.CompilerServices;
using Components;
using Core;
using SaveSystem;

public partial class Inventory : ISaveable<InventoryData> {
	public int MaxSlots = 100;
	public ItemSlot[] ItemSlots;

	public Inventory() {
		ItemSlots = new ItemSlot[MaxSlots];
		for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i] = new ItemSlot();
		}
	}

	public Inventory(int maxSlots) {
		MaxSlots = maxSlots;
		ItemSlots = new ItemSlot[MaxSlots];
		for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i] = new ItemSlot();
		}
	}

	public bool IsFull() {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(ItemSlots[i].IsEmpty()) {
				return false;
			}
		}
		return true;
	}

	public bool ItemExists(string itemId) {
		for(int i = 0; i < ItemSlots.Length; i++) {
			if(!ItemSlots[i].IsEmpty() && ItemSlots[i].Item != null && ItemSlots[i].Item.Data != null) {
				if(ItemSlots[i].ContainsItem(itemId)) {
					return true;
				}
			}
		}
		return false;
	}

	private ItemSlotData[] SerializeItemSlots() {
		ItemSlotData[] itemSlots = new ItemSlotData[ItemSlots.Length];
		for(int i = 0; i < ItemSlots.Length; i++) {
			itemSlots[i] = ItemSlots[i].Serialize();
		}
		return itemSlots;
	}

	public InventoryData Serialize() => new InventoryData {
		MaxSlots = MaxSlots,
		ItemSlots = SerializeItemSlots(),
	};

	public void Deserialize(in InventoryData data) {
		MaxSlots = data.MaxSlots;
		ItemSlots = new ItemSlot[MaxSlots];
        for(int i = 0; i < ItemSlots.Length; i++) {
			ItemSlots[i].Deserialize(data.ItemSlots[i]);
		}
    }
}

namespace SaveSystem {
	public readonly record struct InventoryData : ISaveData {
		public int MaxSlots { get; init; }
        public ItemSlotData[] ItemSlots { get; init; }
    }
}