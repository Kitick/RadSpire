using System.Collections.Generic;
using Core;
using Godot;
using SaveSystem;

namespace SettingsPanels {
	public partial class SettingsMenu : Control, ISaveable<SettingsData> {
		private const string SETTINGS_FILENAME = "settings";

		private const string GENERAL_PANEL = "General_Panel";
		private const string DISPLAY_PANEL = "Display_Panel";
		private const string SOUND_PANEL = "Sound_Panel";
		private const string CONTROLLER_PANEL = "Controller_Panel";
		private const string MK_PANEL = "MK_Panel";
		private const string ACCESSIBILITY_PANEL = "Accessibility_Panel";
		private const string EXTRAS_PANEL = "Extras_Panel";

		private readonly Dictionary<string, string> ButtonToPanelMap = new() {
			{"Top_Panel/General_Button", GENERAL_PANEL},
			{"Top_Panel/Display_Button", DISPLAY_PANEL},
			{"Top_Panel/Sound_Button", SOUND_PANEL},
			{"Top_Panel/Controller_Button", CONTROLLER_PANEL},
			{"Top_Panel/MK_Button", MK_PANEL},
			{"Top_Panel/Accessibility_Button", ACCESSIBILITY_PANEL},
			{"Top_Panel/Extras_Button", EXTRAS_PANEL}
		};

		public override void _Ready() {
			// Works both in main menu and paused game
			ProcessMode = ProcessModeEnum.Always;

			//LoadData();
			SetCallbacks();
		}

		public override void _Input(InputEvent input) {
			if(input.IsActionPressed(Actions.UICancel)) {
				GetViewport().SetInputAsHandled();
				SaveData();
				Visible = false;
			}
		}

		private void SetCallbacks() {
			foreach(var (buttonPath, panelPath) in ButtonToPanelMap) {
				Button button = GetNode<Button>(buttonPath);
				button.Pressed += () => OnCategoryPressed(panelPath);
			}
		}

		private void OnCategoryPressed(string panelNameToShow) {
			foreach(var panelName in ButtonToPanelMap.Values) {
				bool shouldShow = panelName == panelNameToShow;
				GetNode<Control>(panelName).Visible = shouldShow;
			}
		}

		public void SaveData() {
			var data = Serialize();
			SaveService.Save(SETTINGS_FILENAME, data);
		}

		public void LoadData() {
			if(SaveService.Exists(SETTINGS_FILENAME)) {
				var data = SaveService.Load<SettingsData>(SETTINGS_FILENAME);
				Deserialize(data);
			}
		}

		public SettingsData Serialize() {
			return new SettingsData {
				DisplaySettings = GetNode<DisplayPanel>(DISPLAY_PANEL).Serialize(),
				SoundSettings = GetNode<SoundPanel>(SOUND_PANEL).Serialize()
			};
		}

		public void Deserialize(in SettingsData data) {
			GetNode<DisplayPanel>(DISPLAY_PANEL).Deserialize(data.DisplaySettings);
			GetNode<SoundPanel>(SOUND_PANEL).Deserialize(data.SoundSettings);
		}
	}
}

namespace SaveSystem {
	public readonly record struct SettingsData : ISaveData {
		public DisplaySettings DisplaySettings { get; init; }
		public SoundSettings SoundSettings { get; init; }
	}
}
