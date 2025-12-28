using System;
using Services;
using Services.Network;

namespace Components {
	public interface IHealth { Health Health { get; } }

	public sealed class Health : IOnChanged<int>, ISaveable<HealthData> {
		public int Current {
			get;
			set {
				value = Math.Clamp(value, 0, Max);
				if(field == value) { return; }

				OnChanged?.Invoke(field, value);
				field = value;
			}
		}

		public int Max {
			get;
			set {
				field = Math.Max(1, value);
				Current = Math.Min(Current, field);
			}
		}

		public event Action<int, int>? OnChanged;

		public Health(int max) {
			Max = max;
			Current = max;
		}

		public HealthData Export() => new HealthData {
			Max = Max,
			Current = Current
		};

		public void Import(HealthData data) {
			Max = data.Max;
			Current = data.Current;
		}
	}

	public static class HealthExtensions {
		public static void Heal(this IHealth entity, int amount) => entity.Health.Current += amount;
		public static void Hurt(this IHealth entity, int amount) => entity.Health.Current -= amount;
		public static void Heal(this IHealth entity) => entity.Health.Current = entity.Health.Max;
		public static void Hurt(this IHealth entity) => entity.Health.Current = 0;

		public static bool IsHealed(this IHealth entity) => entity.Health.Current == entity.Health.Max;
		public static bool IsDead(this IHealth entity) => entity.Health.Current == 0;
		public static bool IsHurt(this IHealth entity) => !IsHealed(entity);
		public static bool IsAlive(this IHealth entity) => !IsDead(entity);

		public static float Percent(this IHealth entity) {
			return (float) entity.Health.Current / entity.Health.Max;
		}

		public static Action WhenHealed(this IHealth entity, Action callback) {
			return entity.Health.When((int from, int to) => {
				if(from < entity.Health.Max && to == entity.Health.Max) { callback(); }
			});
		}

		public static Action WhenDead(this IHealth entity, Action callback) {
			return entity.Health.When((int from, int to) => {
				if(from > 0 && to == 0) { callback(); }
			});
		}
	}

	public readonly record struct HealthData : ISaveData, INetworkData {
		public int Current { get; init; }
		public int Max { get; init; }
	}
}