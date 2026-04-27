namespace Settings.Interface;

using System;
using Godot;
using Root;
using Services;
using Settings;
using UI;

public sealed partial class SettingsMenu : BaseUIControl {
	[ExportCategory("Buttons")]
	[Export] private Button BackButton = null!;
	[Export] private Button ResetButton = null!;

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

	private (Control panel, Button button)[] Panels => [
		(DisplayPanel, DisplayButton),
		(SoundPanel, SoundButton),
		(ControllerPanel, ControllerButton),
		(MKPanel, MKButton),
	];

	private Control InitialPanel => DisplayPanel;

	private Control ActivePanel = null!;

	protected override Control? DefaultFocus => ActivePanel switch {
		_ when ActivePanel == DisplayPanel => DisplayPanel.FirstControl,
		_ when ActivePanel == SoundPanel => SoundPanel.FirstControl,
		_ when ActivePanel == ControllerPanel => ControllerPanel.FirstControl,
		_ when ActivePanel == MKPanel => MKPanel.FirstControl,
		_ => null,
	};

	private event Action? OnExit;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.Always;

		SetCallbacks();
		SetInputCallbacks();

		SwitchToPanel(InitialPanel);
	}

	public override void _ExitTree() {
		base._ExitTree();
		OnExit?.Invoke();
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
	}

	private void SetCallbacks() {
		BackButton.Pressed += OnBackButtonPressed;
		ResetButton.Pressed += OnResetSettingsButtonPressed;

		foreach((Control panel, Button button) in Panels) {
			button.Pressed += () => SwitchToPanel(panel);
		}
	}

	private void OnBackButtonPressed() {
		QueueFree();
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
		foreach((Control panel, Button button) in Panels) {
			bool isTarget = panel == target;
			panel.Visible = isTarget;
			button.Disabled = isTarget;
		}
	}

	public void OpenMenu() {
		LoadData();
		OnOpen();
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
		ControllerPanel.Refresh();
		MKPanel.Refresh();
	}
}
