using Godot;
using Services;

namespace UI.Settings {
	public sealed partial class GeneralPanel : VBoxContainer {

		[Export] private OptionButton LanguageOption = null!;
		[Export] private HSlider UIScaleSlider = null!;
		[Export] private OptionButton ThemeOption = null!;

		// Main
		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		private void SetCallbacks() {
			LanguageOption.ItemSelected += OnLanguageOptionSelected;
			UIScaleSlider.ValueChanged += OnUIScaleChanged;
			ThemeOption.ItemSelected += OnThemeOptionSelected;
		}

		// Callbacks
		private void OnLanguageOptionSelected(long selected) {
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

	// Update As Needed
	public readonly record struct GeneralSettings : ISaveData {

	}
}