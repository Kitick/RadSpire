using Godot;
using System;

public partial class pabScript : Node
{
public override void _Ready()
{
		GD.Print("Hello, World!");

		var label = new Label();
		label.Text = "Hello, World!";
		AddChild(label);
	}
}
