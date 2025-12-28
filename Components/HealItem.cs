using Godot;
using Services;

namespace Components {
	public interface IHealItem { HealItem Item { get; } }

	public sealed class HealItem : ISaveable<HealItemData> {
		public int HealAmount;

		public HealItem(int amount) {
			HealAmount = amount;
		}

		public HealItemData Export() => new HealItemData {
			HealAmount = HealAmount,
		};

		public void Import(HealItemData data) {
			HealAmount = data.HealAmount;
		}
	}

	public readonly record struct HealItemData : ISaveData {
		public int HealAmount { get; init; }
	}
}