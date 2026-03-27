namespace Components;

using System;
using System.Collections.Generic;
using Services;

public sealed class ComponentDictionary<TComp> where TComp : class {
	private static readonly LogService Log = new(nameof(ComponentDictionary<>), enabled: true);

	private readonly Dictionary<Type, TComp> Components = [];

	public int Count => Components.Count;

	public IReadOnlyDictionary<Type, TComp> All => Components;

	public bool Add<T>(T component) where T : class, TComp {
		if(component == null) {
			return false;
		}
		Type key = typeof(T);
		if(Components.ContainsKey(key)) {
			return false;
		}
		Components[key] = component;
		return true;
	}

	public bool Add(Type type, TComp component) {
		if(component == null || type == null) {
			return false;
		}
		if(Components.ContainsKey(type)) {
			return false;
		}
		Components[type] = component;
		return true;
	}

	public bool Has<T>() where T : class, TComp {
		return Components.ContainsKey(typeof(T));
	}

	public T Get<T>() where T : class, TComp {
		if(Has<T>()) {
			return (T) Components[typeof(T)];
		}
		else {
			Log.Info($"Component of type {typeof(T).Name} not found.");
			return null!;
		}
	}

	public bool Remove<T>() where T : class, TComp {
		return Components.Remove(typeof(T));
	}

	public bool Remove(Type type) {
		return Components.Remove(type);
	}

	public void Clear() {
		Components.Clear();
	}
}
