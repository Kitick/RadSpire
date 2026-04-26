namespace Components;

using System;
using System.Collections.Generic;

public interface IEntity {
	Storage<IComponent> Components { get; }
}

public sealed class Storage<T> {
	private readonly Dictionary<Type, T> Items = [];

	public void Set<TComp>(TComp component) where TComp : T => Items[typeof(TComp)] = component;

	public bool Add<TComp>(TComp component) where TComp : T {
		if(Items.ContainsKey(typeof(TComp))) { return false; }
		Set(component);
		return true;
	}

	public bool Has<TComp>() where TComp : T => Items.ContainsKey(typeof(TComp));

	public bool Remove<TComp>() where TComp : T => Items.Remove(typeof(TComp));

	public TComp? Get<TComp>() where TComp : T {
		Items.TryGetValue(typeof(TComp), out T? value);
		return (TComp?) value;
	}

	public bool Get<TComp>(out TComp component) where TComp : T {
		if(Items.TryGetValue(typeof(TComp), out T? value)) {
			component = (TComp) value!;
			return true;
		}

		component = default!;
		return false;
	}

	public IEnumerable<T> All() => Items.Values;

	public void Clear() => Items.Clear();
}
