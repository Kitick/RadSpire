using System;
using Core;
using Godot;
using Services;
using UI.Multiplayer;
using UI.Settings;

namespace UI {
	public sealed partial class MainMenu : Control, INavigatable {
		private static readonly LogService Log = new(nameof(MainMenu), enabled: true);

		[ExportCategory("Main Buttons")]
		[Export] private Button SingleplayerButton = null!;
		[Export] private Button MultiplayerButton = null!;
		[Export] private Button SettingsButton = null!;
		[Export] private Button ExtrasButton = null!;
		[Export] private Button QuitButton = null!;

		[ExportCategory("Singleplayer Pop-up")]
		[Export] private Control SingleplayerPanel = null!;
		[Export] private Button ContinueButton = null!;
		[Export] private Button LoadSavedButton = null!;
		[Export] private Button StartNewButton = null!;

		[ExportCategory("Multiplayer Pop-up")]
		[Export] private Control MultiplayerPanel = null!;
		[Export] private Button HostNewButton = null!;
		[Export] private Button HostSavedButton = null!;
		[Export] private Button JoinGameButton = null!;

		[ExportCategory("Scene Refrences")]
		[Export] private PackedScene SettingsScene = null!;
		[Export] private PackedScene SaveMenuScene = null!;
		[Export] private PackedScene HostPanelScene = null!;
		[Export] private PackedScene JoinPanelScene = null!;

		public Control[] Order => [
			SingleplayerButton, MultiplayerButton, SettingsButton, ExtrasButton, QuitButton
		];

		enum MenuState { Normal, SinglePopup, MultiPopup }

		private const float HideDelay = 0.25f;

		public event Action? OnStartNewGame;
		public event Action? OnContinueGame;
		public event Action<string>? OnLoadGame;
		public event Action? OnQuit;

		public override void _Ready() {
			UpdateContinueButtonState();
			SetCallbacks();

			Navigator.Instance.SetPanel(this);
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
			GetTree().CreateTimer(HideDelay).Timeout += () => {
				if(!IsMouseInside(SingleplayerButton, SingleplayerPanel, MultiplayerButton, MultiplayerPanel)) {
					SetPopupState(MenuState.Normal);
				}
			};
		}

		private bool IsMouseInside(params Control[] nodes) {
			Vector2 mousePos = GetViewport().GetMousePosition();
			foreach(var node in nodes) {
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

		// Local popup management
		private void OpenSettings() {
			var settings = this.AddScene<SettingsMenu>(SettingsScene);
			settings.OpenMenu();
		}

		private void OpenSaveMenu() {
			var saveMenu = this.AddScene<SaveMenu>(SaveMenuScene);
			saveMenu.OnLoad += fileName => OnLoadGame?.Invoke(fileName);
			saveMenu.OpenMenu(SaveMenuMode.Load);
		}

		private void OpenHostPanel() {
			var host = this.AddScene<HostPanel>(HostPanelScene);
			host.OpenMenu();
		}

		private void OpenJoinPanel() {
			var join = this.AddScene<JoinPanel>(JoinPanelScene);
			join.OpenMenu();
		}
	}
}