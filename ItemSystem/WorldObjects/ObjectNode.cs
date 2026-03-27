namespace ItemSystem.WorldObjects;

using System;
using Components;
using Godot;
using ItemSystem;
using Root;
using Services;

public partial class ObjectNode : Node3D {
	private static readonly LogService Log = new(nameof(ObjectNode), enabled: true);
	public Object Data { get; private set; } = null!;
	private Action? Unsubscribe;

	public virtual void Bind(Object obj) {
		Data = obj;

		GlobalPosition = obj.WorldLocation.Position;
		GlobalRotation = obj.WorldLocation.Rotation;

		Unsubscribe = obj.WorldLocation.When((from, to) => {
			GlobalPosition = to.Position;
			GlobalRotation = to.Rotation;
		});
	}

	public bool Interact<TEntity>(TEntity interactor) {
		bool success = false;
		foreach(var component in Data.ComponentDictionary.All.Values) {
			if(component is IInteract interactComponent) {
				success |= interactComponent.Interact(interactor);
			}
		}
		return success;
	}

	public override void _ExitTree() {
		Unsubscribe?.Invoke();
	}
}

