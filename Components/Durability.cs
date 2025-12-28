using System;
using Services;

namespace Components {
	public interface IDurable { Durability Durability { get; } }

	public sealed class Durability : IOnChanged<int>, ISaveable<DurabilityData> {
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

		public Durability(int max) {
			Max = max;
			Current = max;
		}

		public DurabilityData Export() => new DurabilityData {
			Current = Current,
			Max = Max,
		};

		public void Import(DurabilityData data) {
			Current = data.Current;
			Max = data.Max;
		}
	}

	public static class DurabilityExtensions {
		public static void Repair(this IDurable entity, int amount) => entity.Durability.Current += amount;
		public static void Damage(this IDurable entity, int amount) => entity.Durability.Current -= amount;
		public static void Repair(this IDurable entity) => entity.Durability.Current = entity.Durability.Max;
		public static void Damage(this IDurable entity) => entity.Durability.Current = 0;

		public static bool IsNew(this IDurable entity) => entity.Durability.Current == entity.Durability.Max;
		public static bool IsBroken(this IDurable entity) => entity.Durability.Current == 0;
		public static bool IsDamaged(this IDurable entity) => !IsNew(entity);
		public static bool IsFunctional(this IDurable entity) => !IsBroken(entity);

		public static float Percent(this IDurable entity) {
			return (float) entity.Durability.Current / entity.Durability.Max;
		}

		public static Action WhenNew(this IDurable entity, Action callback) {
			return entity.Durability.When((int from, int to) => {
				if(from < entity.Durability.Max && to == entity.Durability.Max) { callback(); }
			});
		}

		public static Action WhenBroken(this IDurable entity, Action callback) {
			return entity.Durability.When((int from, int to) => {
				if(from > 0 && to == 0) { callback(); }
			});
		}
	}

	public readonly record struct DurabilityData : ISaveData {
		public int Current { get; init; }
		public int Max { get; init; }
	}
}