using Godot;
using System;

public partial class Ui : CanvasLayer {

	private Button PauseButton = null!;
	[Export] private PackedScene PauseMenu = null!;
	private Control Paused = null!;
	public override void _Ready() {
		PauseMenu = GD.Load<PackedScene>("res://HUD/Pause_Menu.tscn");
		GetComponents();
		SetCallBacks();
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
			TogglePauseMenu();
	}

	private void GetComponents() {
		PauseButton = GetNode<Button>("Pause_Button");
	}

	private void SetCallBacks() {
		PauseButton.Pressed += OnPauseButtonPressed;
	}

	private void OnPauseButtonPressed() {
		TogglePauseMenu();
		GD.Print("Pause button was pressed!");
	}

	private void TogglePauseMenu()
	{
		if (Paused == null)
		{
			Paused = PauseMenu.Instantiate<Control>();
			AddChild(Paused);

			// Important:
			Paused.ProcessMode = Node.ProcessModeEnum.WhenPaused;
			Paused.Visible = false; // start hidden so first toggle shows it
		}

		bool showing = !Paused.Visible;
		Paused.Visible = showing;
		GetTree().Paused = showing;
		
		Input.MouseMode = showing
			? Input.MouseModeEnum.Visible
			: Input.MouseModeEnum.Captured;
		
	}
	
	


}
