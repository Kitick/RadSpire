using System;
using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public partial class Inventory : ISaveable<InventoryData> {
    [Export] private int MaxSlots;
    //[Export] private ItemBaseData[] Items;



	public InventoryData Serialize() => new InventoryData {
        MaxSlots = MaxSlots,
	};

	public void Deserialize(in InventoryData data) {
		MaxSlots = data.MaxSlots;
	}
}

namespace SaveSystem {
	public readonly record struct InventoryData : ISaveData {
		public int MaxSlots { get; init; }
	}
}