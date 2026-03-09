using Godot;
using Services;
using Components;

namespace ItemSystem {
	public partial class Sword : Area3D {
		private static readonly LogService Log = new(nameof(Sword), enabled: true);

		private Node3D WeaponOwner = null!;

		public int Damage = 10;

		public override void _Ready() {
			WeaponOwner = GetOwner<Node3D>();
			Monitoring = false;
			BodyEntered += OnBodyEntered;
		}

		private void OnBodyEntered(Node3D body) {
			if(body == WeaponOwner)
				return;
		}
	}
}
