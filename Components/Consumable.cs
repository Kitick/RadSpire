using Godot;
using Services;

namespace Components {
	public interface IConsumable { Consumable Consumable { get; } }

	public sealed class Consumable : ISaveable<ConsumeableData> {
		public int HealAmount { get; set; }

		public ConsumeableData Export() => new ConsumeableData {
			Amount = HealAmount,
		};

		public void Import(ConsumeableData data) {
			HealAmount = data.Amount;
		}
	}

	public readonly struct ConsumeableData : ISaveData {
		public int Amount { get; init; }
	}
}