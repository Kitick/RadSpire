using System.Diagnostics;
using System;
using Godot;
using SaveSystem;

namespace Settings {
	public sealed partial class AccessibilityPanel : VBoxContainer {
		
		// Node Paths
		private const string SUBTITLES = "Subtitles/CheckBox";
		private const string SUBTITLES_SIZE = "Subtitle_Size/HSlider";
		private const string COLORBLIND_MODE = "Colorblind_Mode/OptionButton";
		private const string TEXT_TO_SPEECH = "Text-to-Speech/CheckBox";
		private const string HIGH_CONTRAST_UI = "High_Contrast_UI/CheckBox";

		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		public void SetCallbacks() {
			GetNode<CheckBox>(SUBTITLES).Toggled += OnSubtitlesCheckBox;
			GetNode<HSlider>(SUBTITLES_SIZE).ValueChanged += OnSubtitleSizeChanged;
			GetNode<OptionButton>(COLORBLIND_MODE).ItemSelected += OnColorblindSelected;
			GetNode<CheckBox>(TEXT_TO_SPEECH).Toggled += OnTextToSpeechCheckBox;
			GetNode<CheckBox>(HIGH_CONTRAST_UI).Toggled += OnHighContrastUICheckBox;
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
