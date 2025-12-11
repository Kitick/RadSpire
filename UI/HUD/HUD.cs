using System;
using Character;
using Core;
using Godot;
using Services;
using UI.Multiplayer;
using UI.Settings;
using MenuState = Root.GameManager.MenuState;

namespace UI {
	public sealed partial class HUD : Control {
		private static readonly LogService Log = new(nameof(HUD), enabled: true);

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

		public Player Player = null!;

		public event Action? ResumeRequested;
		public event Action? PauseRequested;
		public event Action? SettingsRequested;
		public event Action? HostRequested;
		public event Action? MainMenuRequested;
		public event Action? RespawnRequested;
		public event Action<string>? SaveRequested;

		public void Init(Player player, StateMachine<MenuState> stateMachine) {
			Player = player;
			ConfigureStateMachine(stateMachine);
		}

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			SetCallbacks();
			UpdateHealthBar();
		}

		private void ConfigureStateMachine(StateMachine<MenuState> stateMachine) {
			// Game state - normal gameplay
			stateMachine.OnEnter(MenuState.Game, () => {
				PauseMenu.CloseMenu();
				RespawnMenu.CloseMenu();
				Inventory.Visible = false;
			});

			// Paused state
			stateMachine.OnEnter(MenuState.Paused, PauseMenu.OpenMenu);
			stateMachine.OnExit(MenuState.Paused, PauseMenu.CloseMenu);

			// Settings state
			stateMachine.OnEnter(MenuState.Settings, OpenSettingsPanel);

			// Inventory state
			stateMachine.OnEnter(MenuState.Inventory, () => {
				Inventory.Visible = true;
				Hotbar.Visible = true;
			});

			stateMachine.OnExit(MenuState.Inventory, () => {
				Inventory.Visible = false;
			});

			// Host state
			stateMachine.OnEnter(MenuState.Host, OpenHostPanel);

			// Death state
			stateMachine.OnEnter(MenuState.Death, RespawnMenu.OpenMenu);
			stateMachine.OnExit(MenuState.Death, RespawnMenu.CloseMenu);
		}

		private void SetCallbacks() {
			// Pause button in HUD
			PauseButton.Pressed += () => PauseRequested?.Invoke();

			// Pause menu buttons
			PauseMenu.ResumeButton.Pressed += () => ResumeRequested?.Invoke();
			PauseMenu.SaveButton.Pressed += OpenSaveMenu;
			PauseMenu.HostButton.Pressed += () => HostRequested?.Invoke();
			PauseMenu.SettingsButton.Pressed += () => SettingsRequested?.Invoke();
			PauseMenu.MainMenuButton.Pressed += () => MainMenuRequested?.Invoke();

			// Respawn menu buttons
			RespawnMenu.RespawnButton.Pressed += () => RespawnRequested?.Invoke();
			RespawnMenu.MainMenuButton.Pressed += () => MainMenuRequested?.Invoke();

			// Health bar updates
			Player.Health.OnHealthChanged += (from, to) => UpdateHealthBar();
		}

		private void UpdateHealthBar() {
			int current = Player.Health.CurrentHealth;
			int max = Player.Health.MaxHealth;

			HealthBar.MaxValue = max;
			HealthBar.Value = current;
		}

		private void OpenSettingsPanel() {
			var settings = SettingsScene.Instantiate<SettingsMenu>();
			settings.OpenMenu();
		}

		private void OpenHostPanel() {
			var hostPanel = HostPanelScene.Instantiate<HostPanel>();
			hostPanel.UpdateHostText("Host Game");
			hostPanel.OpenMenu();
		}

		private void OpenSaveMenu() {
			var saveMenu = SaveMenuScene.Instantiate<SaveMenu>();
			saveMenu.OnSave += fileName => SaveRequested?.Invoke(fileName);
			saveMenu.OpenMenu(SaveMenuMode.Save);
		}
	}
}