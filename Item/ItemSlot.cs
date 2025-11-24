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

    public bool AddItem(Item item) {
        if(!IsEmpty()) {
            return false;
        }
        Item = item;
        return true;
    }

    public void RemoveItem() {
        Item = null;
    }

    public ItemSlotData Serialize() => new ItemSlotData {
        if(Item == null) {
            Item = null;
            return;
        }
        Item = Item.Serialize();
    }

    public void Deserialize(in ItemSlotData data) {
        if(data.Item is null) {
            Item = null;
            return;
        }
        Item = new Item();
        Item.Deserialize(data.Item);
    }
}

namespace SaveSystem {
    public readonly record struct ItemSlotData : ISaveData {
        public ItemData? Item { get; init; }
    }
}
