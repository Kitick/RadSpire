using Godot;
using Services;

namespace Components {
	public interface IHealItem { HealItem Item { get; } }

	public sealed class HealItem : ISaveable<HealItemData> {
		public int HealAmount { get; set; }

		public HealItemData Export() => new HealItemData {
			Amount = HealAmount,
		};

		public void Import(HealItemData data) {
			HealAmount = data.Amount;
		}
	}

	public static class ItemExtensions {
		public static void HealWith<TEntity, TItem>(this TEntity target, TItem item)
		where TEntity : IHealth
		where TItem : IHealItem {
			target.Heal(item.Item.HealAmount);
		}
	}

	public readonly struct HealItemData : ISaveData {
		public int Amount { get; init; }
	}
}