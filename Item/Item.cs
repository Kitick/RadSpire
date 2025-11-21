using System;
using Components;
using Core;
using SaveSystem;

public class Item : ISaveable<ItemData> {
	public ItemBaseData Data {
        get;
        set;
    }

    public int Quantity {
        get;
        set {
            if(field < 0) {
                field = 0;
                return;
            }
            if(!Data.IsStackable && field > 1) {
                field = 1;
                return;
            }
            if(Data.IsStackable && field > Data.MaxStackSize) {
                field = Data.MaxStackSize;
                return;
            }
            field = value;
        }
    }

	public ItemData Serialize() => new ItemData {
		ItemBaseData = Data.Serialize(),
	};

	public void Deserialize(in ItemData data) {
		Data.Deserialize(data.ItemBaseData);
	}
}

namespace SaveSystem {
	public readonly record struct ItemData : ISaveData {
                public ItemBaseDataData ItemBaseData { get; init; }
	}
}