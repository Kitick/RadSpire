namespace Components {
	using System;
	using ItemSystem;

	public interface IDurable { Durability Durability { get; set; } }

	public sealed class Durability : Component<DurabilityData>, IItemComponent, IItemUseable, IItemUseableOnTarget {
		public int Current {
			get => Data.Current;
			set => SetData(Data with { Current = Math.Clamp(value, 0, Data.Max) });
		}

		public int Max {
			get => Data.Max;
			set {
				int max = Math.Max(1, value);
				SetData(Data with { Max = max, Current = Math.Min(Data.Current, max) });
			}
		}

		public bool Use<TEntity>(TEntity user) {
			// Durability usage logic here
			return false;
		}

		public bool UseOnTarget<TEntity, TTarget>(TEntity user, TTarget target) {
			// Durability usage on target logic here
			return false;
		}

		public Durability(int max) : base(new DurabilityData { Max = max, Current = max }) { }
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
			return entity.Durability.When((DurabilityData from, DurabilityData to) => {
				if(from.Current < to.Max && to.Current == to.Max) { callback(); }
			});
		}

		public static Action WhenBroken(this IDurable entity, Action callback) {
			return entity.Durability.When((DurabilityData from, DurabilityData to) => {
				if(from.Current > 0 && to.Current == 0) { callback(); }
			});
		}
	}

	public readonly record struct DurabilityData : Services.ISaveData {
		public int Current { get; init; }
		public int Max { get; init; }
	}
}