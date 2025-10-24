//Last Editor Info
//Name: Pablo Macias
//Date: October 23, 2025
//Purpose: Simple Main Menu Screen Layout and Buttons

using Godot;
using System;

public partial class Main_Menu : Control{
	
	public override void _Ready(){
	//Definitions
		var startButton = GetNode<Button>("VBoxContainer/Start_Button");
		startButton.Pressed += _on_Start_Button_pressed;
		
		var quitButton = GetNode<Button>("VBoxContainer/Quit_Button");
		quitButton.Pressed += _on_Quit_Button_pressed;
		
		var settingsButton = GetNode<Button>("VBoxContainer/Settings_Button");
		settingsButton.Pressed += _on_Settings_Button_pressed;
	}

	//Implementations
	//Start Button
	private void _on_Start_Button_pressed(){
		GD.Print("Start button was pressed!");
	}
	
	//Settings Button
	private void _on_Settings_Button_pressed(){
		GD.Print("Settings button was pressed");
	}
	
	//Quit Button 
	private void _on_Quit_Button_pressed(){
		GetTree().Quit();
	}
}
