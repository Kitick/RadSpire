namespace UI.MainMenu;

using System;
using Godot;
using Network.Panels;
using Root;
using Services;
using Settings.Interface;
using UI.SaveMenu;

public sealed partial class MainMenu : BaseUIControl {
	private static readonly LogService Log = new(nameof(MainMenu), enabled: true);

	[ExportCategory("Main Buttons")]
	[Export] private VBoxContainer ButtonPanel = null!;
	[Export] private Button SingleplayerButton = null!;
	[Export] private Button MultiplayerButton = null!;
	[Export] private Button SettingsButton = null!;
	[Export] private Button ExtrasButton = null!;
	[Export] private Button QuitButton = null!;

	[ExportCategory("Singleplayer Pop-up")]
	[Export] private VBoxContainer SingleplayerPanel = null!;
	[Export] private Button ContinueButton = null!;
	[Export] private Button LoadSavedButton = null!;
	[Export] private Button StartNewButton = null!;

	[ExportCategory("Multiplayer Pop-up")]
	[Export] private VBoxContainer MultiplayerPanel = null!;
	[Export] private Button HostNewButton = null!;
	[Export] private Button HostSavedButton = null!;
	[Export] private Button JoinGameButton = null!;

	[ExportCategory("Scene Refrences")]
	[Export] private PackedScene SettingsScene = null!;
	[Export] private PackedScene SaveMenuScene = null!;
	[Export] private PackedScene HostPanelScene = null!;
	[Export] private PackedScene JoinPanelScene = null!;

	private enum MenuState { Normal, SinglePopup, MultiPopup }

	private const float HideDelay = 0.25f;

	protected override Button? DefaultFocus => SingleplayerButton;

	public event Action? OnStartNewGame;
	public event Action? OnContinueGame;
	public event Action<string>? OnLoadGame;
	public event Action? OnQuit;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();

		UpdateContinueButtonState();
		SetCallbacks();
		OnOpen();
	}

	private void SetCallbacks() {
		// Main buttons
		StartNewButton.Pressed += () => OnStartNewGame?.Invoke();
		ContinueButton.Pressed += () => OnContinueGame?.Invoke();
		QuitButton.Pressed += () => OnQuit?.Invoke();

		// Local popup management
		SettingsButton.Pressed += OpenSettings;
		LoadSavedButton.Pressed += OpenSaveMenu;
		HostNewButton.Pressed += OpenHostPanel;
		JoinGameButton.Pressed += OpenJoinPanel;

		// Hover behavior for Singleplayer popup
		SingleplayerButton.MouseEntered += () => SetPopupState(MenuState.SinglePopup);
		SingleplayerButton.MouseExited += HidePopup;
		SingleplayerPanel.MouseExited += HidePopup;

		// Hover behavior for Multiplayer popup
		MultiplayerButton.MouseEntered += () => SetPopupState(MenuState.MultiPopup);
		MultiplayerButton.MouseExited += HidePopup;
		MultiplayerPanel.MouseExited += HidePopup;
	}

	private void SetPopupState(MenuState state) {
		SingleplayerPanel.Visible = state == MenuState.SinglePopup;
		MultiplayerPanel.Visible = state == MenuState.MultiPopup;
	}

	private void HidePopup() {
		if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) { return; }
		GetTree().CreateTimer(HideDelay).Timeout += () => {
			if(!IsMouseInside(SingleplayerButton, SingleplayerPanel, MultiplayerButton, MultiplayerPanel)) {
				SetPopupState(MenuState.Normal);
			}
		};
	}

	private bool IsMouseInside(params Control[] nodes) {
		Vector2 mousePos = GetViewport().GetMousePosition();
		foreach(Control node in nodes) {
			if(node.GetGlobalRect().HasPoint(mousePos)) {
				return true;
			}
		}
		return false;
	}

	private void UpdateContinueButtonState() {
		bool hasAutosave = SaveService.Exists(Constants.AutosaveFile);
		ContinueButton.Disabled = !hasAutosave;
	}

	private void OpenSettings() {
		SettingsMenu settings = this.AddScene<SettingsMenu>(SettingsScene);

		settings.TreeExited += () => {
			if(!IsInsideTree()) { return; }
			ButtonPanel.Visible = true;
			if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
				SettingsButton.GrabFocus();
			}
		};

		ButtonPanel.Visible = false;
		settings.OpenMenu();
	}

	private void OpenSaveMenu() {
		SaveMenu saveMenu = this.AddScene<SaveMenu>(SaveMenuScene);
		saveMenu.OnLoad += fileName => OnLoadGame?.Invoke(fileName);
		saveMenu.TreeExited += () => {
			if(!IsInsideTree()) { return; }
			if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
				LoadSavedButton.GrabFocus();
			}
		};
		saveMenu.OpenMenu(SaveMenu.SaveMode.Load);
	}

	private void OpenHostPanel() {
		HostPanel host = this.AddScene<HostPanel>(HostPanelScene);
		host.TreeExited += () => {
			if(!IsInsideTree()) { return; }
			if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
				HostNewButton.GrabFocus();
			}
		};
		host.OpenMenu();
	}

	private void OpenJoinPanel() {
		JoinPanel join = this.AddScene<JoinPanel>(JoinPanelScene);
		join.TreeExited += () => {
			if(!IsInsideTree()) { return; }
			if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
				JoinGameButton.GrabFocus();
			}
		};
		join.OpenMenu();
	}

	public override void _Process(double delta) {
		if(InputSystem.Instance.CurrentInputMode != InputSystem.InputMode.Controller) { return; }

		Control focused = GetViewport().GuiGetFocusOwner();

		if(focused == SingleplayerButton || focused == ContinueButton ||
		   focused == LoadSavedButton || focused == StartNewButton) {
			SetPopupState(MenuState.SinglePopup);
		}
		else if(focused == MultiplayerButton || focused == HostNewButton ||
				focused == HostSavedButton || focused == JoinGameButton) {
			SetPopupState(MenuState.MultiPopup);
		}
		else {
			SetPopupState(MenuState.Normal);
		}
	}
}
