namespace Components;

using System;
using System.Collections.Generic;
using Services;

public class ComponentDictionary<TComponent> where TComponent : class {
	private static readonly LogService Log = new(nameof(ComponentDictionary<TComponent>), enabled: true);
	private readonly Dictionary<Type, TComponent> Components = new();

	public int Count => Components.Count;

	public IReadOnlyDictionary<Type, TComponent> All => Components;

	public bool Add<T>(T component) where T : class, TComponent {
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

	public bool Add(Type type, TComponent component) {
		if(component == null || type == null) {
			return false;
		}
		if(Components.ContainsKey(type)) {
			return false;
		}
		Components[type] = component;
		return true;
	}

	public bool Has<T>() where T : class, TComponent {
		return Components.ContainsKey(typeof(T));
	}

	public T Get<T>() where T : class, TComponent {
		if(Has<T>()) {
			return (T) Components[typeof(T)];
		}
		else {
			Log.Info($"Component of type {typeof(T).Name} not found.");
			return null!;
		}
	}

	public bool Remove<T>() where T : class, TComponent {
		return Components.Remove(typeof(T));
	}

	public bool Remove(Type type) {
		return Components.Remove(type);
	}

	public void Clear() {
		Components.Clear();
	}
}
