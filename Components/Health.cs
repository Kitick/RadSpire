//This file was developed entirely by the RadSpire Development Team.

using System;
using Network;
using SaveSystem;

namespace Components {
	public sealed class Health : ISaveable<HealthData>, INetworkable<HealthData> {
		private static readonly Logger Log = new(nameof(Health), enabled: true);
	
		public event Action? OnStateChanged;

		public int CurrentHealth {
			get;
			set {
				value = Math.Clamp(value, 0, MaxHealth);
				Log.Info($"Health changed from {field} to {value}.");
				if(field == value) { return; }

				OnHealthChanged?.Invoke(field, value);
				OnStateChanged?.Invoke();

				field = value;
			}
		}

		public int MaxHealth {
			get;
			set {
				var oldValue = field;
				field = Math.Max(1, value);
				CurrentHealth = Math.Min(CurrentHealth, field);
				if(oldValue != field) { OnStateChanged?.Invoke(); }
			}
		}

		public event Action<int, int>? OnHealthChanged;

		public Health(int maxHealth) {
			MaxHealth = maxHealth;
			CurrentHealth = maxHealth;
		}

		public HealthData Serialize() => new HealthData {
			MaxHealth = MaxHealth,
			CurrentHealth = CurrentHealth
		};

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
			(float) health.CurrentHealth / health.MaxHealth;
	}
}

namespace SaveSystem {
	public readonly struct HealthData : ISaveData, INetworkData {
		public int CurrentHealth { get; init; }
		public int MaxHealth { get; init; }
	}
}