using System;
using System.Collections.Generic;
using Godot;
using InputSystem;
using SaveSystem;

namespace Settings {
	public sealed partial class SettingsMenu : Control, ISaveable<SettingsData> {
		private static readonly Logger Log = new(nameof(SettingsMenu), enabled: true);

		private const string SAVEFILE = "settings";

		private const string GENERAL = "General";
		private const string DISPLAY = "Display";
		private const string SOUND = "Sound";
		private const string CONTROLLER = "Controller";
		private const string MK = "MK";
		private const string ACCESSIBILITY = "Accessibility";
		private const string HEADER = "Top_Panel";
		private const string TOPANEL = "_Panel";
		private const string TOBUTTON = "_Button";
		private readonly string[] Tabs = [GENERAL, DISPLAY, SOUND, CONTROLLER, MK, ACCESSIBILITY];

		private const string BACK_BUTTON = "BackButton";
		private const string RESET_SETTINGS_BUTTON = "ResetSettingsButton";

		private readonly Dictionary<string, (VBoxContainer panel, Button button)> Nodes = [];

		public event Action? OnMenuClosed;
		private event Action? OnExit;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			GetComponents();
			SetCallbacks();
			SetInputCallbacks();
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		private void GetComponents() {
			var header = GetNode<HBoxContainer>(HEADER);

			foreach(var path in Tabs) {
				var panel = GetNode<VBoxContainer>(path + TOPANEL);
				var button = header.GetNode<Button>(path + TOBUTTON);

				Nodes[path] = (panel, button);
				button.Pressed += () => SwitchToPanel(panel);
			}
		}

		private void SetCallbacks() {
			GetNode<Button>(BACK_BUTTON).Pressed += OnBackButtonPressed;
			GetNode<Button>(RESET_SETTINGS_BUTTON).Pressed += OnResetSettingsButtonPressed;
		}

		private void OnBackButtonPressed() {
			CloseMenu();
		}

		private void OnResetSettingsButtonPressed() {
			//Implementation Here should make it so what ever panel is open it can be reset
		}

		private void SwitchToPanel(VBoxContainer target) {
			foreach(var (panel, _) in Nodes.Values) {
				panel.Visible = panel == target;
			}
		}

		public void OpenMenu() {
			LoadData();
		}

		private void CloseMenu() {
			SaveData();
			QueueFree();
		}

		private void SaveData() {
			SaveService.Save(SAVEFILE, Serialize());
			Log.Info("Settings saved");
		}

		private void LoadData() {
			if(SaveService.Exists(SAVEFILE)) {
				var data = SaveService.Load<SettingsData>(SAVEFILE);
				Deserialize(data);
				Log.Info("Settings loaded");
			}
			else {
				Log.Info("No settings file found, using defaults");
			}
		}

		private ISaveable<T> CastISaveable<T>(string path) where T : ISaveData {
			return (ISaveable<T>) Nodes[path].panel;
		}

		public SettingsData Serialize() => new SettingsData {
			DisplaySettings = CastISaveable<DisplaySettings>(DISPLAY).Serialize(),
			SoundSettings = CastISaveable<SoundSettings>(SOUND).Serialize(),
		};

		public void Deserialize(in SettingsData data) {
			CastISaveable<DisplaySettings>(DISPLAY).Deserialize(data.DisplaySettings);
			CastISaveable<SoundSettings>(SOUND).Deserialize(data.SoundSettings);
		}
	}
}

namespace SaveSystem {
	public readonly record struct SettingsData : ISaveData {
		public DisplaySettings DisplaySettings { get; init; }
		public SoundSettings SoundSettings { get; init; }
	}
}
