//Last Editor Info
//Name: Pablo Macias
//Date: October 23, 2025
//Purpose: Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

using Godot;
using System;

public partial class Main_Menu : Control{
	//Button and Panel References
	private Button startButton;
	private Button settingsButton;
	private Button quitButton;

	private Control startButtonPanel;
	private Button onlineButton;
	private Button localButton;
	private Button privateMatchButton;

	public override void _Ready(){
		//Main Buttons
		startButton = GetNode<Button>("VBoxContainer/Start_Button");
		settingsButton = GetNode<Button>("VBoxContainer/Settings_Button");
		quitButton = GetNode<Button>("VBoxContainer/Quit_Button");

		startButton.Pressed += _on_Start_Button_pressed;
		settingsButton.Pressed += _on_Settings_Button_pressed;
		quitButton.Pressed += _on_Quit_Button_pressed;

		//Start Button Panel
		startButtonPanel = GetNode<Control>("Start_Button_Panel");
		onlineButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Online_Button");
		localButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Local_Button");
		privateMatchButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Private_Match_Button");

		//Hover Behavior
		startButton.MouseEntered += OnStartButtonHover;
		startButton.MouseExited += OnStartButtonUnhover;
		startButtonPanel.MouseExited += OnPanelMouseExited;

		//Click Handlers for Pop-up
		onlineButton.Pressed += () => OnModeSelected("Online");
		localButton.Pressed += () => OnModeSelected("Local");
		privateMatchButton.Pressed += () => OnModeSelected("Private Match");
	}

	//Main Button Handlers
	private void _on_Start_Button_pressed(){
		GD.Print("Start button was pressed!");
	}

	private void _on_Settings_Button_pressed(){
		GD.Print("Settings button was pressed");
	}

	private void _on_Quit_Button_pressed(){
		GetTree().Quit();
	}

	//Hover Pop-Up Logic
	private void OnStartButtonHover(){
		//Position the Pop-up next to the Start button
		Vector2 popupPosition = startButton.GlobalPosition + new Vector2(startButton.Size.X + -100, 150);
		startButtonPanel.GlobalPosition = popupPosition;
		startButtonPanel.Visible = true;
	}

	//Unhover Pop-Up Logic
	private void OnStartButtonUnhover(){
		//Wait a tiny bit before hiding, so the user can move the mouse into the panel
		GetTree().CreateTimer(0.1).Timeout += () =>{
			Vector2 mousePos = GetViewport().GetMousePosition();

			//Only hide if the mouse is NOT inside the Start_Button_Panel area
			if (!startButtonPanel.GetGlobalRect().HasPoint(mousePos))
				startButtonPanel.Visible = false;
		};
	}

	private void OnPanelMouseExited(){
		//Wait a bit, then check if the mouse is back on the Start_Button
		GetTree().CreateTimer(0.1).Timeout += () =>{
			Vector2 mousePos = GetViewport().GetMousePosition();

			//Only hide if mouse is outside BOTH Start_Button and Start_Button_Panel
			if (!startButton.GetGlobalRect().HasPoint(mousePos) && !startButtonPanel.GetGlobalRect().HasPoint(mousePos)){
				startButtonPanel.Visible = false;
			}
		};
	}

	private void OnModeSelected(string mode){
		GD.Print($"Selected mode: {mode}");
		startButtonPanel.Visible = false;

		switch (mode){
			case "Online":
				//Load online scene or setup logic here
				break;
			case "Local":
				//Local match logic
				break;
			case "Private Match":
				//Private match setup
				break;
		}
	}
}
