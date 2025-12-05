using System.Diagnostics;
using System;
using Godot;
using SaveSystem;

namespace Settings {
	public sealed partial class ControllerPanel : VBoxContainer{
		
		// Node Paths 
		private const string ENABLE_CONTROLLER = "Enable_Controller/CheckBox";
		private const string VIBRATION = "Vibration/CheckBox";
		private const string DEADZONE = "Deadzone/HSlider";
		private const string REMAP_BUTTONS = "Remap_Buttons";

		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		public void SetCallbacks() {
			GetNode<CheckBox>(ENABLE_CONTROLLER).Toggled += OnEnableControllerCheckBox;
			GetNode<CheckBox>(VIBRATION).Toggled += OnVibrationCheckbox;
			GetNode<HSlider>(DEADZONE).ValueChanged += OnDeadzoneChanged;
			GetNode<Button>(REMAP_BUTTONS).Pressed += OnRemapButtonsPressed;
		}

		// Callbacks
		private void OnEnableControllerCheckBox(bool check) {
			//Implementation Here
		}

		private void OnVibrationCheckbox(bool check) {
			//Implementation Here
		}

		private void OnDeadzoneChanged(double value) {
			//Implementation Here
		}

		private void OnRemapButtonsPressed(){
			//Implementation Here
		}

		//ISaveable Implementation Goes Here

	}
}

// Update as Needed
namespace SaveSystem {
	public readonly record struct ControllerSettings : ISaveData {
		
	}
}
