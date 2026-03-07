using System;
using System.Linq;
using Godot;
using Services;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class SettingsMenu : Control {
		[ExportCategory("Buttons")]
		[Export] private Button BackButton = null!;
		[Export] private Button ResetButton = null!;

		[ExportCategory("General")]
		[Export] private GeneralPanel GeneralPanel = null!;
		[Export] private Button GeneralButton = null!;

		[ExportCategory("Display")]
		[Export] private DisplayPanel DisplayPanel = null!;
		[Export] private Button DisplayButton = null!;

		[ExportCategory("Sound")]
		[Export] private SoundPanel SoundPanel = null!;
		[Export] private Button SoundButton = null!;

		[ExportCategory("Controls")]
		[Export] private ControllerPanel ControllerPanel = null!;
		[Export] private Button ControllerButton = null!;

		[ExportCategory("Mouse & Keyboard")]
		[Export] private MkPanel MKPanel = null!;
		[Export] private Button MKButton = null!;

		[ExportCategory("Accessibility")]
		[Export] private AccessibilityPanel AccessibilityPanel = null!;
		[Export] private Button AccessibilityButton = null!;

		public Control[] Order => [
			GeneralPanel, DisplayPanel, SoundPanel, ControllerPanel, MKPanel, AccessibilityPanel, ResetButton, BackButton
		];

		private (Control panel, Button button)[] Panels => [
			(GeneralPanel, GeneralButton),
			(DisplayPanel, DisplayButton),
			(SoundPanel, SoundButton),
			(ControllerPanel, ControllerButton),
			(MKPanel, MKButton),
			(AccessibilityPanel, AccessibilityButton),
		];

		private Control InitialPanel => GeneralPanel;

		private Control ActivePanel = null!;

		private event Action? OnExit;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			SetCallbacks();
			SetInputCallbacks();

			SwitchToPanel(InitialPanel);
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
		}

		private void SetInputCallbacks() {
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
			if(ActivePanel == DisplayPanel) {
				DisplaySettings.Reset();
				DisplaySettings.Apply();
				DisplayPanel.Refresh();
			}
			else if(ActivePanel == SoundPanel) {
				AudioSettings.Reset();
				AudioSettings.Apply();
				SoundPanel.Refresh();
			}
			else if(ActivePanel == GeneralPanel) {
				GeneralSettings.Reset();
				GeneralSettings.Apply();
				GeneralPanel.Refresh();
			}
			else if(ActivePanel == AccessibilityPanel) {
				AccessibilitySettings.Reset();
				AccessibilitySettings.Apply();
				AccessibilityPanel.Refresh();
			}
			else if(ActivePanel == ControllerPanel) {
				ControllerSettings.Reset();
				ControllerSettings.Apply();
				ControllerPanel.Refresh();
			}
			else if(ActivePanel == MKPanel) {
				MouseKeyboardSettings.Reset();
				MouseKeyboardSettings.Apply();
				MKPanel.Refresh();
			}
		}

		private void SwitchToPanel(Control target) {
			ActivePanel = target;
			foreach(var (panel, button) in Panels) {
				var isTarget = panel == target;
				panel.Visible = isTarget;
				button.Disabled = isTarget;
			}
		}

		public void OpenMenu(Action? onClose = null) {
			OnExit += onClose;
			LoadData();

			if(ActivePanel == null) {
				GeneralButton.GrabFocus();
			}
			else {
				foreach(var (panel, button) in Panels) {
					if(panel == ActivePanel) {
						button.GrabFocus();
						break;
					}
				}
			}
		}

		private void CloseMenu() {
			SaveData();
			QueueFree();
		}

		private static void SaveData() {
			SettingSystem.Save();
		}

		private void LoadData() {
			DisplayPanel.Refresh();
			SoundPanel.Refresh();
			GeneralPanel.Refresh();
			AccessibilityPanel.Refresh();
			ControllerPanel.Refresh();
			MKPanel.Refresh();
		}
	}
}
