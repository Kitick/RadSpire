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
	}
}