using System;
using Services;

namespace Components {
	public interface IDurability { Durability Durability { get; } }

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

		public DurabilityData Serialize() => new DurabilityData {
			Current = Current,
			Max = Max,
		};

		public void Deserialize(in DurabilityData data) {
			Current = data.Current;
			Max = data.Max;
		}
	}

	public static class DurabilityExtensions {
		public static void Repair(this IDurability target, int amount) => target.Durability.Current += amount;
		public static void Damage(this IDurability target, int amount) => target.Durability.Current -= amount;
		public static void Repair(this IDurability target) => target.Durability.Current = target.Durability.Max;
		public static void Damage(this IDurability target) => target.Durability.Current = 0;

		public static bool IsNew(this IDurability target) => target.Durability.Current == target.Durability.Max;
		public static bool IsBroken(this IDurability target) => target.Durability.Current == 0;
		public static bool IsDamaged(this IDurability target) => !IsNew(target);
		public static bool IsFunctional(this IDurability target) => !IsBroken(target);

		public static float Percent(this IDurability target) {
			return (float) target.Durability.Current / target.Durability.Max;
		}

		public static Action WhenNew(this IDurability target, Action callback) {
			return target.Durability.When((int from, int to) => {
				if(from < target.Durability.Max && to == target.Durability.Max) { callback(); }
			});
		}

		public static Action WhenBroken(this IDurability target, Action callback) {
			return target.Durability.When((int from, int to) => {
				if(from > 0 && to == 0) { callback(); }
			});
		}
	}

	public readonly struct DurabilityData : ISaveData {
		public int Current { get; init; }
		public int Max { get; init; }
	}
}