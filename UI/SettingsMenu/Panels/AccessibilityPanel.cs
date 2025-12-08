using System.Diagnostics;
using System;
using Godot;
using SaveSystem;

namespace Settings {
	public sealed partial class AccessibilityPanel : VBoxContainer {

		[Export] private CheckBox SubtitlesCheckBox = null!;
		[Export] private HSlider SubtitleSizeSlider = null!;
		[Export] private OptionButton ColorblindModeOption = null!;
		[Export] private CheckBox TextToSpeechCheckBox = null!;
		[Export] private CheckBox HighContrastUICheckBox = null!;

		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		public void SetCallbacks() {
			SubtitlesCheckBox.Toggled += OnSubtitlesCheckBox;
			SubtitleSizeSlider.ValueChanged += OnSubtitleSizeChanged;
			ColorblindModeOption.ItemSelected += OnColorblindSelected;
			TextToSpeechCheckBox.Toggled += OnTextToSpeechCheckBox;
			HighContrastUICheckBox.Toggled += OnHighContrastUICheckBox;
		}

		// Callbacks
		private void OnSubtitlesCheckBox(bool check) {
			//Implementation Here
		}

		private void OnSubtitleSizeChanged(double value) {
			//Implementation Here
		}

		private void OnColorblindSelected(long selected) {
			//Implementation Here
		}

		private void OnTextToSpeechCheckBox(bool check) {
			//Implementation Here
		}

		private void OnHighContrastUICheckBox(bool check) {
			//Implementation Here
		}

		//ISaveable Implementation Goes Here

	}
}

// Update as needed
namespace SaveSystem {
	public readonly record struct AccessibilitySettings : ISaveData {

	}
}
