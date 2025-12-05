using System;
using SaveSystem;
using Godot;

namespace Components {
    [GlobalClass]
    public partial class Healing : Resource, IItemComponent, IConsumable, ISaveable<HealingData> {
        private static readonly Logger Log = new(nameof(Healing), enabled: true);

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
                player.Health.CurrentHealth += Mathf.Clamp(HealAmount, 0, player.Health.MaxHealth - player.Health.CurrentHealth);
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
}

namespace SaveSystem {
    public readonly struct HealingData : ISaveData {
        public int healAmount { get; init; }
    }
}