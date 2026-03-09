using Core;
using Godot;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class GeneralPanel : VBoxContainer {
		[Export] private OptionButton LanguageOption = null!;
		[Export] private HSlider UIScaleSlider = null!;
		[Export] private OptionButton ThemeOption = null!;

		public override void _Ready() {
			this.ValidateExports();
			SetCallbacks();
		}

		private void SetCallbacks() {
			LanguageOption.ItemSelected += (index) => GeneralSettings.Language.Apply(LanguageOption.GetItemText((int) index));
			UIScaleSlider.ValueChanged += (value) => GeneralSettings.UIScale.Apply((float) value);
			ThemeOption.ItemSelected += (index) => GeneralSettings.Theme.Apply(ThemeOption.GetItemText((int) index));
		}

		public void Refresh() {
			LanguageOption.SelectItem(GeneralSettings.Language.Target);
			UIScaleSlider.Value = GeneralSettings.UIScale.Target;
			ThemeOption.SelectItem(GeneralSettings.Theme.Target);
		}
	}
}