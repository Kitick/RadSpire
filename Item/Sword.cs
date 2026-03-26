namespace ItemSystem;

using Components;
using Godot;
using Services;

public partial class Sword : Area3D {
	private static readonly LogService Log = new(nameof(Sword), enabled: true);

	private Node3D WeaponOwner = null!;

	public int Damage = 10;

	private AudioStreamPlayer? impactSound;

	public override void _Ready() {
		WeaponOwner = GetOwner<Node3D>();
		Monitoring = false;
		BodyEntered += OnBodyEntered;

		impactSound = GetNode<AudioStreamPlayer>("ImpactSound");
	}

	private void OnBodyEntered(Node3D body) {
		if(body == WeaponOwner) return;

		if(impactSound is { Playing: false }) impactSound.Play();

		if(body is IHealth health) {
			health.Hurt(Damage);
		}
	}
}
