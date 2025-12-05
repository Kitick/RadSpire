using System;
using Godot;

namespace Settings {
	public sealed partial class MkPanel : VBoxContainer {

		// Node Paths
		private const string MOUSE_SENSE = "Mouse_Sense/HSlider";
		private const string INVERTED_Y_AXIS = "Invert_Y-Axis/CheckBox";
		private const string RAW_INPUT = "Enable_Raw_Input/CheckBox";
		private const string REMAP_KEYS = "Remap_Keys";

		public override void _Ready() {
			SetCallbacks();
		}

		public void SetCallbacks() {
			GetNode<HSlider>(MOUSE_SENSE).ValueChanged += OnMouseSenseChanged;
			GetNode<CheckBox>(INVERTED_Y_AXIS).Toggled += OnInvertedYAxisCheckBox;
			GetNode<CheckBox>(RAW_INPUT).Toggled += OnRawInputCheckbox;
			GetNode<Button>(REMAP_KEYS).Pressed += OnRemapKeysPressed;
		}

		// Callbacks
		private void OnMouseSenseChanged(double value) {
			//Implementation Here
		}

		private void OnInvertedYAxisCheckBox(bool check) {
			//Implmentation Here
		}

		private void OnRawInputCheckbox(bool check) {
			//Implementation Here
		}

		private void OnRemapKeysPressed() {
			//Implementation Here
		}

		//ISaveable Implmentation Goes Here

	}
}

//Update as Needed
namespace SaveSystem {
	public readonly record struct MkSettings : ISaveData {
		
	}
}
