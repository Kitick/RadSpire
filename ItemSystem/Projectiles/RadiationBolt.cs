namespace ItemSystem;

using Character;
using Components;
using Godot;

public partial class RadiationBolt : Area3D {
	[Export] private float Speed = 18f;
	[Export] private float Lifetime = 1.5f;
	[Export] private int Damage = 8;
	[Export] private PackedScene? HitSparkScene;

	private Vector3 Direction = Vector3.Forward;
	private CharacterBase? OwnerCharacter;
	private float LifeTimer = 0f;

	public void Init(CharacterBase owner, Vector3 direction, int damage) {
		OwnerCharacter = owner;
		Direction = direction.Normalized();
		Damage = damage;
	}

	public void SetSpeed(float speed) {
		Speed = speed;
	}

	public override void _Ready() {
		BodyEntered += OnBodyEntered;
		LifeTimer = Lifetime;
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;
		GlobalPosition += Direction * Speed * dt;

		LifeTimer -= dt;
		if(LifeTimer <= 0f) {
			SpawnImpact();
			QueueFree();
		}
	}

	private void OnBodyEntered(Node3D body) {
		if(body == OwnerCharacter) { return; }

		if(body is IHealth healthTarget) {
			if(OwnerCharacter != null) {
				healthTarget.Hurt(Damage);
			}

			SpawnImpact();
			QueueFree();
		}
	}

	private void SpawnImpact() {
		if(HitSparkScene?.Instantiate() is not Node3D spark) {
			return;
		}

		GetParent()?.AddChild(spark);
		spark.GlobalPosition = GlobalPosition;

		if(spark.GetNodeOrNull<GpuParticles3D>("GPUParticles3D") is { } particles) {
			particles.Restart();
			particles.Emitting = true;
		}
	}
}
