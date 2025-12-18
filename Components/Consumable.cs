using Godot;
using Services;

namespace Components {
	public interface IConsumable { Consumable Consumable { get; } }

	public sealed class Consumable : ISaveable<ConsumeableData> {
		public int HealAmount { get; set; }

		public ConsumeableData Serialize() => new ConsumeableData {
			Amount = HealAmount,
		};

		public void Deserialize(in ConsumeableData data) {
			HealAmount = data.Amount;
		}
	}

	public readonly struct ConsumeableData : ISaveData {
		public int Amount { get; init; }
	}
}