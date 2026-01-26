namespace ItemSystem {
	using System.Collections.Generic;
	using Components;
	using Godot;
	using Services;

	public partial class ItemHeal : Item, IHealItem, ISaveable<ItemHealData> {
        public HealItem Heal { get; set; } = null!;

        public ItemHeal(Item BaseItem, HealItem HealComponent) : base(BaseItem) {
            Heal = HealComponent;
        }

		public ItemHealData Export() => new ItemHealData {
			HealData = Heal.Export()
		};

		public void Import(ItemHealData data) {
			Heal.Import(data.HealData);
		}
	}

	public readonly record struct ItemHealData : ISaveData {
		public HealItemData HealData { get; init; }
	}
}
