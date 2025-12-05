using Godot;

public partial class Sword : Area3D
{
	public int Damage = 20;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if(body is Enemy enemy) {
			enemy.TakeDamage(Damage);
			GD.Print("Enemy took damage.");
		}
	}
}