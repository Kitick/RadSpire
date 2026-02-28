namespace Objects {
	using System;
	using Godot;
	using Core;
	using Services;
	using ItemSystem;
	using Components;

	public partial class ObjectNode : Node3D {
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

		public override void _ExitTree() {
			Unsubscribe?.Invoke();
		}
	}
}
