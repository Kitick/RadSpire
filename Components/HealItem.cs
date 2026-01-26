namespace Components {
	public interface IHealItem { HealItem Heal { get; } }

	public sealed class HealItem : Component<HealItemData> {
		public int HealAmount {
			get => Data.HealAmount;
			set => SetData(Data with { HealAmount = value });
		}

		public HealItem(HealItemData data) : base(data) { }
		public HealItem(int amount) : base(new HealItemData { HealAmount = amount }) { }
	}

	public readonly record struct HealItemData : Services.ISaveData {
		public int HealAmount { get; init; }
	}
}
