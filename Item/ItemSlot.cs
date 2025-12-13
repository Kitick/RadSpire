//This file was developed entirely by the RadSpire Development Team.

using System;
using Components;
using Core;
using SaveSystem;

public class ItemSlot : ISaveable<ItemSlotData> {
	public Item? Item { 
		get;
		set {
			field = value;
			Item?.BuildComponents();
		} 
	}

	public int Quantity {
		get;
		set {
			if(value < 0) {
				field = 0;
				return;
			}
			if(Item == null) {
				field = 0;
				return;
			}
			if(!Item.IsStackable && value > 1) {
				field = 1;
				return;
			}
			if(Item.IsStackable && value > Item.MaxStackSize) {
				field = Item.MaxStackSize;
				return;
			}
			field = value;
		}
	}

	public ItemSlot() {
		Item = null;
		Quantity = 0;
	}

	public ItemSlot(Item item, int quantity) {
		Item = item;
		Quantity = quantity;
	}

	public ItemSlot(Item item) {
		Item = item;
		Quantity = 1;
	}

	public bool IsEmpty() {
		if(Item == null) {
			return true;
		}
		if(Quantity <= 0) {
			return true;
		}
		return false;
	}

	public ItemSlot AddItem(Item item) {
		ItemSlot returnItemStack = new ItemSlot(item);
		returnItemStack = combineItemSlot(returnItemStack);
		return returnItemStack;
	}

	public ItemSlot combineItemSlot(ItemSlot other) {
		ItemSlot returnItemStack = other;
		if(IsEmpty()) {
			Item = other.Item;
			Quantity = other.Quantity;
			returnItemStack.Quantity = 0;
			return returnItemStack;
		}
		if(SameItem(other) && Item!.IsStackable) {
			int space = Item.MaxStackSize - Quantity;
			if(space <= 0) {
				return returnItemStack;
			}
			int transfer = Math.Min(space, other.Quantity);
			Quantity += transfer;
			other.Quantity -= transfer;
			returnItemStack.Quantity = other.Quantity;
			return returnItemStack;
		}
		return returnItemStack;
	}

	public ItemSlot RemoveItem(int quantity) {
		ItemSlot returnItemStack = new ItemSlot();
		if(IsEmpty()) {
			return returnItemStack;
		}
		returnItemStack.Item = Item;
		returnItemStack.Quantity = Quantity;
		int removeQuantity = Math.Min(quantity, Quantity);
		Quantity -= removeQuantity;
		returnItemStack.Quantity -= removeQuantity;
		return returnItemStack;
	}

	public void ClearSlot() {
		Item = null;
		Quantity = 0;
	}

	public bool SameItem(Item item) {
		if(IsEmpty()) {
			return false;
		}
		return Item!.SameItem(item);
	}

	public bool SameItem(ItemSlot other) {
		if(IsEmpty() || other.IsEmpty()) {
			return false;
		}
		return Item!.SameItem(other.Item!);
	}

	public ItemSlot Copy() {
		ItemSlot copy = new ItemSlot();
		if(Item == null) {
			return copy;
		}
		copy.Item = Item.Copy();
		copy.Quantity = Quantity;
		return copy;
	}

	public ItemSlotData Serialize() {
		return new ItemSlotData {
			Item = Item?.Serialize(),
			Quantity = Quantity,
		};
	}

	public void Deserialize(in ItemSlotData data) {
		if(data.Item is null || string.IsNullOrEmpty(data.Item.Value.ResourcePath)) {
			Item = null;
			Quantity = 0;
			return;
		}

		// Load the item directly from resource path
		Item = Item.LoadFromData(data.Item.Value);
		Quantity = data.Quantity;
	}
}

namespace SaveSystem {
	public readonly record struct ItemSlotData : ISaveData {
		public ItemData? Item { get; init; }
		public int Quantity { get; init; }
	}
}
