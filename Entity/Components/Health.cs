namespace Components;

using System;

public interface IHealth { Health Health { get; } }
public interface IDamageBlocker {
	bool TryBlockDamage(int amount);
}

public sealed class Health : Component<HealthData> {
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

	public Health(int max) : base(new HealthData { Max = max, Current = max }) { }
}

public static class HealthExtensions {
	public static void Heal(this IHealth entity, int amount) => entity.Health.Current += amount;
	public static void Hurt(this IHealth entity, int amount) {
		if(amount <= 0) {
			return;
		}
		if(entity is IDamageBlocker blocker && blocker.TryBlockDamage(amount)) {
			return;
		}
		entity.Health.Current -= amount;
	}
	public static void Heal(this IHealth entity) => entity.Health.Current = entity.Health.Max;
	public static void Hurt(this IHealth entity) => entity.Health.Current = 0;

	public static bool IsHealed(this IHealth entity) => entity.Health.Current == entity.Health.Max;
	public static bool IsDead(this IHealth entity) => entity.Health.Current == 0;
	public static bool IsHurt(this IHealth entity) => !IsHealed(entity);
	public static bool IsAlive(this IHealth entity) => !IsDead(entity);

	public static float Percent(this IHealth entity) => (float) entity.Health.Current / entity.Health.Max;

	public static Action WhenRestored(this IHealth entity, Action callback) {
		return entity.Health.When((from, to) => {
			if(from.Current < to.Max && to.Current == to.Max) { callback(); }
		});
	}

	public static Action WhenDead(this IHealth entity, Action callback) {
		return entity.Health.When((from, to) => {
			if(from.Current > 0 && to.Current == 0) { callback(); }
		});
	}
}

public readonly record struct HealthData : Services.ISaveData {
	public int Current { get; init; }
	public int Max { get; init; }
}
