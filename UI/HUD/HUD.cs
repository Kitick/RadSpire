using System;
using Character;
using Core;
using Godot;
using Services;
using UI.Multiplayer;
using UI.Settings;

namespace UI {
	public sealed partial class HUD : Control {
		private static readonly LogService Log = new(nameof(HUD), enabled: true);

		public enum MenuState { Game, Paused, Settings, Inventory, Host, Death }

		private readonly StateMachine<MenuState> StateMachine = new();
		public MenuState State => StateMachine.CurrentState;

		// Player reference - set by GameManager, used by child UI elements
		public Player Player { get; set; } = null!;

		// Intent Events - HUD fires these, external systems handle the logic
		public event Action? OnResumePressed;
		public event Action? OnPausePressed;
		public event Action? OnSettingsPressed;
		public event Action? OnHostPressed;
		public event Action? OnMainMenuPressed;
		public event Action? OnRespawnPressed;
		public event Action? OnInventoryTogglePressed;
		public event Action<string>? OnSaveRequested;

		[ExportCategory("HUD Elements")]
		[Export] private Button PauseButton = null!;
		[Export] private PauseMenu PauseMenu = null!;
		[Export] private InventoryUI Inventory = null!;
		[Export] private Control QuestLog = null!;
		[Export] private Hotbar Hotbar = null!;
		[Export] private RespawnMenu RespawnMenu = null!;
		[Export] private ProgressBar HealthBar = null!;

		[ExportCategory("HUD Scenes")]
		[Export] private PackedScene SettingsScene = null!;
		[Export] private PackedScene SaveMenuScene = null!;
		[Export] private PackedScene HostPanelScene = null!;

		private event Action? OnExit;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			ConfigureStateMachine();
			SetInputCallbacks();
			SetButtonCallbacks();

			StateMachine.TransitionTo(MenuState.Game);
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
		}

		private void ConfigureStateMachine() {
			// Game state - normal gameplay
			StateMachine.OnEnter(MenuState.Game, () => {
				PauseMenu.CloseMenu();
				RespawnMenu.CloseMenu();
				Inventory.Visible = false;
			});

			// Paused state
			StateMachine.OnEnter(MenuState.Paused, () => {
				PauseMenu.OpenMenu();
			});
			StateMachine.OnExit(MenuState.Paused, () => {
				PauseMenu.CloseMenu();
			});

			// Settings state
			StateMachine.OnEnter(MenuState.Settings, OpenSettingsPanel);

			// Inventory state
			StateMachine.OnEnter(MenuState.Inventory, () => {
				Inventory.Visible = true;
				Hotbar.Visible = true;
			});
			StateMachine.OnExit(MenuState.Inventory, () => {
				Inventory.Visible = false;
			});

			// Host state
			StateMachine.OnEnter(MenuState.Host, OpenHostPanel);

			// Death state
			StateMachine.OnEnter(MenuState.Death, () => {
				RespawnMenu.OpenMenu();
			});
			StateMachine.OnExit(MenuState.Death, () => {
				RespawnMenu.CloseMenu();
			});
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuExit.WhenPressed(HandleMenuExitInput);
			OnExit += ActionEvent.Inventory.WhenPressed(HandleInventoryInput);
		}

		private void SetButtonCallbacks() {
			// Pause button in HUD
			PauseButton.Pressed += () => OnPausePressed?.Invoke();

			// Pause menu buttons
			PauseMenu.ResumeButton.Pressed += () => OnResumePressed?.Invoke();
			PauseMenu.SaveButton.Pressed += OpenSaveMenu;
			PauseMenu.HostButton.Pressed += () => OnHostPressed?.Invoke();
			PauseMenu.SettingsButton.Pressed += () => OnSettingsPressed?.Invoke();
			PauseMenu.MainMenuButton.Pressed += () => OnMainMenuPressed?.Invoke();

			// Respawn menu buttons
			RespawnMenu.RespawnButton.Pressed += () => OnRespawnPressed?.Invoke();
			RespawnMenu.MainMenuButton.Pressed += () => OnMainMenuPressed?.Invoke();
		}

		private void HandleMenuExitInput() {
			if(State == MenuState.Death) return;

			if(State == MenuState.Game) {
				OnPausePressed?.Invoke();
			}
			else {
				OnResumePressed?.Invoke();
			}
		}

		private void HandleInventoryInput() {
			if(State == MenuState.Death) return;
			OnInventoryTogglePressed?.Invoke();
		}

		private void OpenSettingsPanel() {
			var settings = SettingsScene.Instantiate<SettingsMenu>();
			settings.OpenMenu(onClose: () => StateMachine.TransitionTo(MenuState.Paused));
		}

		private void OpenHostPanel() {
			var hostPanel = HostPanelScene.Instantiate<HostPanel>();
			hostPanel.UpdateHostText("Host Game");
			hostPanel.OpenMenu(onClose: () => StateMachine.TransitionTo(MenuState.Paused));
		}

		private void OpenSaveMenu() {
			var saveMenu = SaveMenuScene.Instantiate<SaveMenu>();
			saveMenu.OnSave += fileName => OnSaveRequested?.Invoke(fileName);
			saveMenu.OpenMenu(SaveMenuMode.Save, onClose: () => StateMachine.TransitionTo(MenuState.Paused));
		}

		// Public API for external control
		public void SetState(MenuState state) {
			StateMachine.TransitionTo(state);
		}

		public void UpdateHealthBar(int current, int max) {
			HealthBar.MaxValue = max;
			HealthBar.Value = current;
		}

		public void ShowDeathScreen() {
			StateMachine.TransitionTo(MenuState.Death);
		}
	}
}