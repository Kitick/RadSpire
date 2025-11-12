using System;
using System.Linq;
using Godot;

namespace SettingsPanels {
	public partial class Display_Panel : VBoxContainer {
		private OptionButton resolutionOption = null!;
		private CheckBox fullscreenCheckBox = null!;
		private CheckBox vsyncCheckBox = null!;
		private HSlider brightnessSlider = null!;
		private OptionButton fpsCapOption = null!;

		public override void _Ready() {
			GetComponents();
			PopulateResolutionOptions();
			PopulateFPSCapOptions();
			SetCallBacks();
		}

		private void GetComponents() {
			resolutionOption = GetNode<OptionButton>("Resolution/OptionButton");
			fullscreenCheckBox = GetNode<CheckBox>("Fullscreen/CheckBox");
			vsyncCheckBox = GetNode<CheckBox>("VSync/CheckBox");
			brightnessSlider = GetNode<HSlider>("Brightness/HSlider");
			fpsCapOption = GetNode<OptionButton>("FPS_Cap/OptionButton");
		}

		private void SetCallBacks() {
			resolutionOption.ItemSelected += OnResolutionSelected;
			fullscreenCheckBox.Toggled += OnFullscreenToggled;
			vsyncCheckBox.Toggled += OnVsyncToggled;
			brightnessSlider.ValueChanged += OnBrightnessChanged;
			fpsCapOption.ItemSelected += OnFPSCapSelected;
		}

		//Resolution
		private void PopulateResolutionOptions() {
			resolutionOption.Clear();
			resolutionOption.AddItem("1920x1080");
			resolutionOption.AddItem("1280x720");
		}

		private void OnResolutionSelected(long index) {
			string selected = resolutionOption.GetItemText((int)index);
			string[] parts = selected.Split('x');
			int width = int.Parse(parts[0]);
			int height = int.Parse(parts[1]);
			DisplayServer.WindowSetSize(new Vector2I(width, height));
		}

		//FullScreen
		private void OnFullscreenToggled(bool pressed) {
			DisplayServer.WindowSetMode(pressed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
			GD.Print($"Full Screen toggled: {pressed}");
		}

		//VSync
		private void OnVsyncToggled(bool pressed) {
			ProjectSettings.SetSetting("display/window/vsync/use_vysnc", pressed);
			ProjectSettings.Save();
		}

		//Brightness
		private void OnBrightnessChanged(double value) {
			GD.Print($"Brightness set to: {value}");
		}

		//FPS Cap
		private void PopulateFPSCapOptions() {
			fpsCapOption.Clear();
			fpsCapOption.AddItem("30 FPS");
			fpsCapOption.AddItem("60 FPS");
			fpsCapOption.AddItem("120FPS");
			fpsCapOption.AddItem("Unlimited");
		}

		private void OnFPSCapSelected(long index) {
			string selected = fpsCapOption.GetItemText((int)index);

			if(selected == "Unlimited") {
				Engine.MaxFps = 0;
				return;
			}

			//Extract digits
			string digitsOnly = new string(selected.Where(char.IsDigit).ToArray());

			if(int.TryParse(digitsOnly, out int fps)) {
				Engine.MaxFps = fps;
			}
			else {
				GD.PushError($"Failed to parse FPS value from: {selected}");
			}

		}
	}
}