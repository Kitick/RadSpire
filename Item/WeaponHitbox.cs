using Godot;
using System;

public partial class WeaponHitbox : Area3D
{
	public bool Active;

	public override void _Ready()
	{
		Monitoring = false;
		BodyEntered += OnBodyEntered;
	}

	public void Activate()
	{
		Active = true;
		Monitoring = true;
	}

	public void Deactivate()
	{
		Active = false;
		Monitoring = false;
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print($"[Hitbox] Body entered: {body.Name}, Active={Active}");

		if (!Active)
			return;
	}
}