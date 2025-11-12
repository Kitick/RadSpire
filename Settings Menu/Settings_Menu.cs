using System.Collections.Generic;
using Godot;

public partial class Settings_Menu : Control {
	private readonly Dictionary<string, string> ButtonToPanelMap = new() {
		{"General_Button", "General_Panel"},
		{"Display_Button", "Display_Panel"},
		{"Sound_Button", "Sound_Panel"},
		{"Controller_Button", "Controller_Panel"},
		{"MK_Button", "MK_Panel"},
		{"Accessibility_Button", "Accessibility_Panel"},
		{"Extras_Button", "Extras_Panel"}
	};

	public override void _Ready() {
		// Works both in main menu and paused game
		ProcessMode = ProcessModeEnum.Always;

		foreach(var entry in ButtonToPanelMap) {
			Button button = GetNode<Button>($"Top_Panel/{entry.Key}");
			button.Pressed += () => OnCategoryPressed(entry.Value);
		}
	}

	public override void _Input(InputEvent input) {
		// Esc
		if(input.IsActionPressed("ui_cancel")) {
			// close overlay and stop Esc from reaching game
			Visible = false;
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnCategoryPressed(string panelNameToShow) {
		foreach(var panelName in ButtonToPanelMap.Values) {
			bool shouldShow = panelName == panelNameToShow;
			GetNode<Control>(panelName).Visible = shouldShow;
		}
	}
}