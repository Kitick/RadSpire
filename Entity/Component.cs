namespace Components;

using System;
using Services;

public interface IComponent;

public abstract class Component<TData> : IComponent, ISaveable<TData> where TData : struct, ISaveData {
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

	public static Action Once<TData>(this Component<TData> target, Action<TData, TData> callback) where TData : struct, ISaveData {
		void Handler(TData from, TData to) {
			target.OnChanged -= Handler;
			callback(from, to);
		}
		target.OnChanged += Handler;
		return () => target.OnChanged -= Handler;
	}
}
