namespace ItemSystem {
	using System.Collections.Generic;
	using Components;
	using Godot;
	using Services;

	public partial class ItemDurability : Item, IDurable, ISaveable<ItemDurabilityData> {
        public Durability Durability { get; set; } = null!;

        public ItemDurability(Item BaseItem, Durability DurabilityComponent) {
            Id = BaseItem.Id;
            Name = BaseItem.Name;
            Description = BaseItem.Description;
            MaxStackSize = BaseItem.MaxStackSize;
            IconTexture = BaseItem.IconTexture;

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
