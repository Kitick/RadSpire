namespace Components;

using System;
using Services;

public abstract class Component<TData> : ISaveable<TData> where TData : struct, ISaveData {
	protected TData Data;

	public event Action<TData, TData>? OnChanged;

	protected Component(TData data) => Data = data;

	protected void SetData(TData newData) {
		if(Data.Equals(newData)) { return; }
		TData previous = Data;
		Data = newData;
		OnChanged?.Invoke(previous, newData);
	}

	public TData Export() => Data;
	public void Import(TData data) => SetData(data);
}

public static class Extensions {
	public static Action When<TData>(this Component<TData> target, Action<TData, TData> callback) where TData : struct, ISaveData {
		target.OnChanged += callback;
		return () => target.OnChanged -= callback;
	}
}

public static class Interactions {
	public static void Attack<TAttacker, TDefender>(this TAttacker attacker, TDefender defender)
	where TAttacker : IOffense
	where TDefender : IHealth {
		int physicalDamage = attacker.Offense.PhysicalDamage;
		int magicDamage = attacker.Offense.MagicDamage;

		int physicalDefense = 0;
		int magicDefense = 0;

		if(defender is IDefense defendable) {
			physicalDefense = defendable.Defense.PhysicalDefense;
			magicDefense = defendable.Defense.MagicDefense;
		}

		physicalDamage = Math.Max(0, physicalDamage - physicalDefense);
		magicDamage = Math.Max(0, magicDamage - magicDefense);

		defender.Hurt(physicalDamage + magicDamage);

		if(attacker is IDurable weapon) {
			weapon.Damage(1);
		}
	}

	public static void HealWith<TEntity, TItem>(this TEntity target, TItem item)
	where TEntity : IHealth
	where TItem : IHealItem {
		target.Heal(item.Heal.HealAmount);
	}
}
