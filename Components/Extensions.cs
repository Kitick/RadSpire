using System;

namespace Components {
	public interface IOnChanged<T> {
		event Action<T, T> OnChanged;
	}

	public static class Extensions {
		public static Action When<TComp, TData>(this TComp target, Action<TData, TData> callback) where TComp : IOnChanged<TData> {
			target.OnChanged += callback;
			return () => target.OnChanged -= callback;
		}

		public static void Attack<T, T2>(this T source, T2 target) where T : IAttack where T2 : IHealth {
			target.Hurt(source.Attack.Damage);
		}

		public static void Consume<T, T2>(this T target, T2 item) where T : IHealth where T2 : IConsumable {
			target.Heal(item.Consumable.HealAmount);
		}
	}
}