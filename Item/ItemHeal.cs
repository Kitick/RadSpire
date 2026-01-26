namespace ItemSystem {
	using System.Collections.Generic;
	using Components;
	using Godot;
	using Services;

	public partial class ItemHeal : ISaveable<ItemHealData>, IHealItem {
        public HealItem Heal { get; set; } = null!;

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
