using Godot;

public partial class Sword : Area3D {
	private static readonly Logger Log = new(nameof(Sword), enabled: true);

	public int Damage = 20;

	public override void _Ready() {
		Monitoring = false;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body) {
		if(body is Enemy enemy) {
			enemy.TakeDamage(Damage);
			Log.Info("Enemy took damage.");
		}
	}
}