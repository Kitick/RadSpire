using Character;
using Components;
using Godot;
using Services;

namespace ItemSystem {
	public partial class WeaponHitbox : Area3D {
		private static readonly LogService Log = new(nameof(WeaponHitbox), enabled: true);

		private CharacterBase? Owner;
		public bool Active;

		public override void _Ready() {
			Monitoring = false;
			BodyEntered += OnBodyEntered;
		}

		public void Init(CharacterBase owner) {
			Owner = owner;
			owner.OnStateChanged += OnOwnerStateChanged;
		}

		private void OnOwnerStateChanged(CharacterBase.State from, CharacterBase.State to) {
			if(to == CharacterBase.State.Attacking) Activate();
			else if(from == CharacterBase.State.Attacking) Deactivate();
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

			if(!Active || Owner == null || body == Owner) return;

			if(body is IHealth healthTarget) {
				Log.Info($"WeaponHitbox hit: {body.Name}");
				Owner.Attack(healthTarget);
				Deactivate();
			}
		}
	}
}
