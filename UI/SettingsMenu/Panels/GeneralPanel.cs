//This file was developed entirely by the RadSpire Development Team.

using System.Diagnostics;
using System;
using Godot;
using SaveSystem;

namespace Settings {
	public sealed partial class GeneralPanel : VBoxContainer {
		
		// Node Paths
		private const string LANGUAGE = "Language/OptionButton";
		private const string UI_SCALE = "UI_Scale/HSlider";
		private const string THEME = "Theme/OptionButton";

		// Main
		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		private void SetCallbacks() {
			GetNode<OptionButton>(LANGUAGE).ItemSelected += OnLanguageOptionSelected;
			GetNode<HSlider>(UI_SCALE).ValueChanged += OnUIScaleChanged;
			GetNode<OptionButton>(THEME).ItemSelected += OnThemeOptionSelected;
		}

		// Callbacks
		private void OnLanguageOptionSelected(long selected){
			//Implementation Here
		}

		private void OnUIScaleChanged(double value) {
            //Implementation Here
        }

		private void OnThemeOptionSelected(long selected) {
			//Implementation Here
		}

		// ISaveable Implementation Goes Here

	}
}

// Update As Needed
namespace SaveSystem {
	public readonly record struct GeneralSettings : ISaveData {
		
	}
}