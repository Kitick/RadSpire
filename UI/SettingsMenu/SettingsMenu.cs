using System;
using Godot;
using Services;

namespace UI.Settings {
	public sealed partial class SettingsMenu : Control, ISaveable<SettingsData> {
		private static readonly LogService Log = new(nameof(SettingsMenu), enabled: true);

		private const string SAVEFILE = "settings";

		[ExportCategory("Buttons")]
		[Export] private Button BackButton = null!;
		[Export] private Button ResetButton = null!;

		[ExportCategory("General")]
		[Export] private Control GeneralPanel = null!;
		[Export] private Button GeneralButton = null!;

		[ExportCategory("Display")]
		[Export] private DisplayPanel DisplayPanel = null!;
		[Export] private Button DisplayButton = null!;

		[ExportCategory("Sound")]
		[Export] private SoundPanel SoundPanel = null!;
		[Export] private Button SoundButton = null!;

		[ExportCategory("Controls")]
		[Export] private Control ControllerPanel = null!;
		[Export] private Button ControllerButton = null!;

		[ExportCategory("Mouse & Keyboard")]
		[Export] private Control MKPanel = null!;
		[Export] private Button MKButton = null!;

		[ExportCategory("Accessibility")]
		[Export] private Control AccessibilityPanel = null!;
		[Export] private Button AccessibilityButton = null!;

		private (Control panel, Button button)[] Panels => [
			(GeneralPanel, GeneralButton),
			(DisplayPanel, DisplayButton),
			(SoundPanel, SoundButton),
			(ControllerPanel, ControllerButton),
			(MKPanel, MKButton),
			(AccessibilityPanel, AccessibilityButton),
		];

		private Control InitialPanel => GeneralPanel;

		private event Action? OnExit;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			SetCallbacks();
			SetInputCallbacks();

			SwitchToPanel(InitialPanel);
		}

		private WorldEnvironment worldEnv = null!;

		private void FetchWorldEnviroment() {
			worldEnv = GetNode<WorldEnvironment>("/root/SceneDirector/GameManager/WorldEnvironment");
			DisplayPanel.SetWorldEnvironment(worldEnv);
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		private void SetCallbacks() {
			BackButton.Pressed += OnBackButtonPressed;
			ResetButton.Pressed += OnResetSettingsButtonPressed;

			foreach(var (panel, button) in Panels) {
				button.Pressed += () => SwitchToPanel(panel);
			}
		}

		private void OnBackButtonPressed() {
			CloseMenu();
		}

		private void OnResetSettingsButtonPressed() {
			//Implementation Here should make it so what ever panel is open it can be reset
		}

		private void SwitchToPanel(Control target) {
			foreach(var (panel, button) in Panels) {
				var isTarget = panel == target;
				panel.Visible = isTarget;
				button.Disabled = isTarget;
			}
		}

		public void OpenMenu(Action? onClose = null) {
			OnExit += onClose;
			FetchWorldEnviroment();
			LoadData();
		}

		private void CloseMenu() {
			SaveData();
			QueueFree();
		}

		private void SaveData() {
			this.Save(SAVEFILE);
			Log.Info("Settings saved");
		}

		private void LoadData() {
			if(SaveService.Exists(SAVEFILE)) {
				this.Load(SAVEFILE);
				Log.Info("Settings loaded");
			}
			else {
				Log.Info("No settings file found, using defaults");
			}
		}

		public SettingsData Export() => new SettingsData {
			DisplaySettings = DisplayPanel.Export(),
			SoundSettings = SoundPanel.Export(),
		};

		public void Import(SettingsData data) {
			DisplayPanel.Import(data.DisplaySettings);
			SoundPanel.Import(data.SoundSettings);
		}
	}

	public readonly record struct SettingsData : ISaveData {
		public DisplaySettings DisplaySettings { get; init; }
		public SoundSettings SoundSettings { get; init; }
	}
}
