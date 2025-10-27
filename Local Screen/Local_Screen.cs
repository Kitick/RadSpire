//Purpose: Simple Local Screen Layout and Buttons to be edited 

using Godot;
using System;

public partial class Local_Screen : Control {
    //Local Screen Button references
    private Button ContinueButton = null!;
	private Button LoadSavedButton = null!;
    private Button StartNewButton = null!;

    public override void _Ready() {
        //Get Button Components
        ContinueButton = GetNode<Button>("ColorRect/Local_Screen_Panel/Continue_Button");
        LoadSavedButton = GetNode<Button>("ColorRect/Local_Screen_Panel/Load_Saved_Button");
        StartNewButton = GetNode<Button>("ColorRect/Local_Screen_Panel/Start_New_Button");

        //Setting Call Backs
        ContinueButton.Pressed += OnContinueButtonPressed;
        LoadSavedButton.Pressed += OnLoadSavedButtonPressed;
        StartNewButton.Pressed += OnStartNewButtonPressed;
    }

    //Load Screen Buttons Handlers
    private void OnContinueButtonPressed() {
		GD.Print("Continue Game Button was pressed!");
	}

	private void OnLoadSavedButtonPressed() {
		GD.Print("Load Saved Games button was pressed!");
	}

	private void OnStartNewButtonPressed() {
        GD.Print("Start New Game button was pressed!");
        GetTree().ChangeSceneToFile("res://Initial Scene/initial_player_scene.tscn");
	}
}