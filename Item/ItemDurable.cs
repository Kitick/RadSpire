namespace ItemSystem {
	using System.Collections.Generic;
	using Components;
	using Godot;
	using Services;

	public partial class ItemDurability : Item, IDurable, ISaveable<ItemDurabilityData> {
        public Durability Durability { get; set; } = null!;

        public ItemDurability(Item BaseItem, Durability DurabilityComponent) : base(BaseItem) {
            Durability = DurabilityComponent;
        }

		public ItemDurabilityData Export() => new ItemDurabilityData {
            BaseItemData = base.Export(),
			DurabilityData = Durability.Export()
		};

		public void Import(ItemDurabilityData data) {
            base.Import(data.BaseItemData);
			Durability.Import(data.DurabilityData);
		}
	}

	public readonly record struct ItemDurabilityData : ISaveData {
        public ItemData BaseItemData { get; init; }
		public DurabilityData DurabilityData { get; init; }
	}
}
