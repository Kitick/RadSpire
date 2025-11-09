using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

public partial class Settings_Menu : Control {
    private Dictionary<string, string> buttonToPanelMap = new() {
        {"General_Button", "General_Panel"},
        {"Display_Button", "Display_Panel"},
        {"Sound_Button", "Sound_Panel"},
        {"Controller_Button", "Controller_Panel"},
        {"MK_Button", "MK_Panel"},
        {"Accessibility_Button", "Accessibility_Panel"},
        {"Extras_Button", "Extras_Panel"}
    };

    public override void _Ready() {

        foreach(var entry in buttonToPanelMap) {
            Button button = GetNode<Button>($"Top_Panel/{entry.Key}");
            string panelName = entry.Value;
            button.Pressed += () => OnCategoryPressed(entry.Value);
        }
    }

    private void OnCategoryPressed(string panelNameToShow) {
        foreach(var panelName in buttonToPanelMap.Values) {
            Control panel = GetNode<Control>($"{panelName}");
            panel.Visible = panelName == panelNameToShow;
        }
    }

    private void CallPanelMethod(string panelName, string methodName) {
        Node panel = GetNode(panelName);
        if(panel.HasMethod(methodName)) {
            panel.Call(methodName);
        }
        else {
            GD.PrintErr($"{panelName} does not have a method '{methodName}'");
        }
    }
}