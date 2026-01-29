namespace Components {
	using ItemSystem;
	public interface IHealItem { HealItem Heal { get; set; } }

	public sealed class HealItem : Component<HealItemData>, IItemComponent, IItemUseable {
		public int HealAmount {
			get => Data.HealAmount;
			set => SetData(Data with { HealAmount = value });
		}

		public bool Use<TEntity>(TEntity user) {
			if(user is IHealth healthEntity) {
				if(healthEntity.IsHurt())
					healthEntity.Heal(HealAmount);
					return true;
			}
			return false;
		}

		public HealItem(HealItemData data) : base(data) { }
		public HealItem(int amount) : base(new HealItemData { HealAmount = amount }) { }
	}

	public readonly record struct HealItemData : Services.ISaveData {
		public int HealAmount { get; init; }
	}
}
