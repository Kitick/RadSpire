using System;
using Components;
using Core;
using SaveSystem;

public class ItemSlot : ISaveable<ItemSlotData> {
    public Item? Item { get; set; }

    public ItemSlot() {
        Item = null;
    }

    public bool IsEmpty() {
        if(Item == null) {
            return true;
        }
        if(Item.Quantity <= 0) {
            return true;
        }
        return false;
    }

    public Item AddItem(Item item) {
        Item returnItem = item;
        if (IsEmpty()) {
            Item = item;
            returnItem.Quantity = 0;
            return returnItem;
        }
        if (Item != null && Item.Data != null && item.Data != null && Item.Data.Id == item.Data.Id && Item.Data.IsStackable) {
            int space = Item.Data.MaxStackSize - Item.Quantity;
            if (space <= 0) {
                return returnItem; 
            }
            int transfer = Math.Min(space, item.Quantity);
            Item.Quantity += transfer;
            item.Quantity -= transfer;
            returnItem.Quantity = item.Quantity;
            return returnItem;
        }
        return returnItem;
    }

    public int RemoveItem(int quantity) {
        if (IsEmpty() || Item == null) {
            return 0;
        }
        int removeQuantity = Math.Min(quantity, Item.Quantity);
        Item.Quantity -= removeQuantity;
        return removeQuantity;
    }

    public void ClearSlot() {
        Item = null;
    }

    public ItemSlotData Serialize() {
        return new ItemSlotData {
            Item = Item?.Serialize()
        };
    }

    public void Deserialize(in ItemSlotData data) {
        if(data.Item is null) {
            Item = null;
            return;
        }
        else {
            Item = new Item();
            Item.Deserialize(data.Item.Value);
        }
    }
}

namespace SaveSystem {
    public readonly record struct ItemSlotData : ISaveData {
        public ItemData? Item { get; init; }
    }
}
