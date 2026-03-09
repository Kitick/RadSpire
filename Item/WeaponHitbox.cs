using Godot;
using Services;

namespace ItemSystem {
	public partial class WeaponHitbox : Area3D {
		private static readonly LogService Log = new(nameof(WeaponHitbox), enabled: true);

		public bool Active;

		public override void _Ready() {
			Monitoring = false;
			BodyEntered += OnBodyEntered;
		}

		public void Activate() {
			Active = true;
			Monitoring = true;
		}

		public void Deactivate() {
			Active = false;
			Monitoring = false;
		}

		private void OnBodyEntered(Node3D body) {
			Log.Info($"Body entered: {body.Name}, Active={Active}");

			if(!Active)
				return;
		}
	}
}
