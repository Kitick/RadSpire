using System;
using Godot;
using SaveSystem;

namespace Settings {
	public sealed partial class MkPanel : VBoxContainer {

		[Export] private HSlider MouseSenseSlider = null!;
		[Export] private CheckBox InvertedYAxisCheckBox = null!;
		[Export] private CheckBox RawInputCheckBox = null!;
		[Export] private Button RemapKeysButton = null!;

		public override void _Ready() {
			SetCallbacks();
		}

		public void SetCallbacks() {
			MouseSenseSlider.ValueChanged += OnMouseSenseChanged;
			InvertedYAxisCheckBox.Toggled += OnInvertedYAxisCheckBox;
			RawInputCheckBox.Toggled += OnRawInputCheckbox;
			RemapKeysButton.Pressed += OnRemapKeysPressed;
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
