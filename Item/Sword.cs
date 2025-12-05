using Godot;

public partial class Sword : Area3D {
	private static readonly Logger Log = new(nameof(Sword), enabled: true);
	
	private Node3D Owner = null!;

	public int Damage = 20;

	public override void _Ready() {
		Owner = GetOwner<Node3D>(); 
		Monitoring = false;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body) {
		if (body == Owner)
			return;
		
		if (body is IDamageable damageable)
		{
			damageable.TakeDamage(Damage);
			Log.Info($"{body.Name} took damage.");
		}
	}
}