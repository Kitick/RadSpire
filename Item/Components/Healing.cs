using Godot;
using Services;
using Character;

namespace Components {
	public partial class Healing : Resource, IItemComponent, IConsumable, ISaveable<HealingData> {
		private static readonly LogService Log = new(nameof(Healing), enabled: true);

		[Export] public int HealAmount { get; set; } = 10;

		public bool CanConsume(CharacterBody3D consumer) {
			if(consumer is Player) {
				return true;
			}
			return false;
		}
		public bool OnConsume(CharacterBody3D consumer) {
			if(CanConsume(consumer)) {
				Player player = (Player) consumer;
				player.Health.CurrentHealth += HealAmount;
				Log.Info($"Consumed healing item. Healed for {HealAmount} points.");
				return true;
			}
			return false;
		}
		public HealingData Serialize() => new HealingData {
			healAmount = HealAmount,
		};

		public void Deserialize(in HealingData data) {
			HealAmount = data.healAmount;
		}
	}

	public readonly struct HealingData : ISaveData {
		public int healAmount { get; init; }
	}
}