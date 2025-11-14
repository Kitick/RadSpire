using System;
using Constants;
using Godot;

public partial class UI : CanvasLayer {
	private Button PauseButton = null!;
	[Export] private PackedScene PauseMenu = null!;
	private Control Paused = null!;

	private const string PAUSE_BUTTON = "Pause_Button";

	public override void _Ready() {
		PauseMenu = GD.Load<PackedScene>(Scenes.PauseMenu);
		GetComponents();
		SetCallBacks();
	}
	public override void _UnhandledInput(InputEvent input) {
		if(input.IsActionPressed(Actions.UICancel))
			TogglePauseMenu();
	}

	private void GetComponents() {
		PauseButton = GetNode<Button>(PAUSE_BUTTON);
	}

	private void SetCallBacks() {
		PauseButton.Pressed += OnPauseButtonPressed;
	}

	private void OnPauseButtonPressed() {
		TogglePauseMenu();
		GD.Print("Pause button was pressed!");
	}

	private void TogglePauseMenu() {
		if(Paused == null) {
			Paused = PauseMenu.Instantiate<Control>();
			AddChild(Paused);

			// Important:

			Paused.ProcessMode = ProcessModeEnum.WhenPaused;
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
