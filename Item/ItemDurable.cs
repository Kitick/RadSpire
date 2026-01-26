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
			DurabilityData = Durability.Export()
		};

		public void Import(ItemDurabilityData data) {
			Durability.Import(data.DurabilityData);
		}
	}

	public readonly record struct ItemDurabilityData : ISaveData {
		public DurabilityData DurabilityData { get; init; }
	}
}
