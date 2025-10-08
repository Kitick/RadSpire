using System;
using Godot;

public partial class Script : Node {
	public override void _Ready() {
		GD.Print("Ready");
	}

	public override void _Process(double delta) {
		GD.Print($"Est FPS: {1/delta}");
	}
}
