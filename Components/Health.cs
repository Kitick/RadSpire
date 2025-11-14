using System;
using SaveSystem;

namespace Components {
	public class Health : ISaveable<HealthData> {
		public int CurrentHealth {
			get;
			set {
				value = Math.Clamp(value, 0, MaxHealth);
				if(field == value) { return; }

				HealthChanged?.Invoke(field, value);

				field = value;
			}
		}

		public int MaxHealth {
			get;
			set {
				field = Math.Max(1, value);
				CurrentHealth = Math.Min(CurrentHealth, field);
			}
		}

		public event Action<int, int>? HealthChanged;

		public Health(int maxHealth) {
			MaxHealth = maxHealth;
			CurrentHealth = maxHealth;
		}

		public HealthData Serialize() {
			return new HealthData {
				MaxHealth = MaxHealth,
				CurrentHealth = CurrentHealth
			};
		}

		public void Deserialize(in HealthData data) {
			MaxHealth = data.MaxHealth;
			CurrentHealth = data.CurrentHealth;
		}
	}

	public static class HealthExtensions {
		public static bool IsAlive(this Health health) => health.CurrentHealth > 0;
		public static bool IsFull(this Health health) => health.CurrentHealth == health.MaxHealth;
		public static bool IsHurt(this Health health) => health.CurrentHealth < health.MaxHealth;
		public static bool IsDead(this Health health) => health.CurrentHealth == 0;

		public static float Percent(this Health health) =>
			(float)health.CurrentHealth / health.MaxHealth;
	}
}

namespace SaveSystem {
	public readonly struct HealthData : ISaveData {
		public int CurrentHealth { get; init; }
		public int MaxHealth { get; init; }
	}
}