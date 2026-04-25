namespace Components;

using System;
using System.Collections.Generic;

public interface IEntity {
	void SetComponent<T>(T component) where T : class, IComponent;
	bool AddComponent<T>(T component) where T : class, IComponent;
	bool HasComponent<T>() where T : class, IComponent;
	bool RemoveComponent<T>() where T : class, IComponent;
	T? GetComponent<T>() where T : class, IComponent;
	bool GetComponent<T>(out T component) where T : class, IComponent;
}

public class Entity : IEntity {
	private readonly Dictionary<Type, IComponent> Components = [];
	private readonly Dictionary<Type, Action<IComponent>> Setters = [];
	private readonly HashSet<Type> Registered = [];

	internal void RegisterComponent<T>(Action<T> setter) where T : class, IComponent {
		Type type = typeof(T);
		Registered.Add(type);
		Setters[type] = comp => setter((T) comp);
	}

	public void SetComponent<T>(T component) where T : class, IComponent {
		Type type = typeof(T);
		Components[type] = component;
		if(Setters.TryGetValue(type, out Action<IComponent>? setter)) {
			setter(component);
		}
	}

	public bool AddComponent<T>(T component) where T : class, IComponent {
		if(Components.ContainsKey(typeof(T))) { return false; }
		Components[typeof(T)] = component;
		return true;
	}

	public bool HasComponent<T>() where T : class, IComponent => Components.ContainsKey(typeof(T));

	public bool RemoveComponent<T>() where T : class, IComponent {
		if(Registered.Contains(typeof(T))) { return false; }
		return Components.Remove(typeof(T));
	}

	public T? GetComponent<T>() where T : class, IComponent {
		Components.TryGetValue(typeof(T), out IComponent? value);
		return value as T;
	}

	public bool GetComponent<T>(out T component) where T : class, IComponent {
		if(Components.TryGetValue(typeof(T), out IComponent? value)) {
			component = (T) value;
			return true;
		}
		component = null!;
		return false;
	}
}
