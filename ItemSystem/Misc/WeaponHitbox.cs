namespace ItemSystem;

using Character;
using Components;
using Godot;
using Services;

public partial class WeaponHitbox : Area3D {
	private static readonly LogService Log = new(nameof(WeaponHitbox), enabled: true);

	private CharacterBase? OwnerCharacter;
	public bool Active;

	public override void _Ready() {
		Monitoring = false;
		BodyEntered += OnBodyEntered;
	}

	public void Init(CharacterBase owner) {
		OwnerCharacter = owner;
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

		if(!Active || OwnerCharacter == null || body == OwnerCharacter) return;

		if(body is IHealth healthTarget) {
			Log.Info($"WeaponHitbox hit: {body.Name}");
			OwnerCharacter?.Attack(healthTarget);
			Deactivate();
		}
	}
}
