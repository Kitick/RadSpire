namespace Root;

using System.Threading.Tasks;
using GameWorld;
using Godot;
using Services;
using Settings;
using UI;
using UI.MainMenu;

public sealed partial class SceneDirector : Node {
	private static readonly LogService Log = new(nameof(SceneDirector), enabled: true);

	[ExportCategory("Scene References")]
	[Export] private PackedScene MainMenuScene = null!;
	[Export] private PackedScene GameWorldScene = null!;
	[Export] private PackedScene SplashPanelScene = null!;

	private Node? CurrentScene;

	public override void _Ready() {
		this.ValidateExports();

		SettingSystem.Load();
		SettingSystem.Apply();

		SwitchMainMenu();
	}

	private void SwitchScene(Node to) => SwitchScene(CurrentScene, to);
	private void SwitchScene(Node? from, Node to) {
		CurrentScene = to;
		AddChild(to);

		if(IsInstanceValid(from)) {
			from.QueueFree();
		}
	}

	private void SwitchMainMenu() {
		MainMenu menu = MainMenuScene.Instantiate<MainMenu>();
		SubscribeToEvents(menu);
		SwitchScene(menu);
	}

	private void SwitchGameScene(string? loadfile = null) {
		GameManager manager = GameWorldScene.Instantiate<GameManager>();
		SubscribeToEvents(manager);

		manager.InitGame(loadfile);
		SwitchScene(manager);
	}

	private void SubscribeToEvents(MainMenu menu) {
		menu.OnStartNewGame += () => {
			Log.Info("Starting new game from MainMenu");
			StartNewGame();
		};

		menu.OnContinueGame += () => {
			Log.Info("Continuing game from MainMenu");
			SwitchGameScene(Constants.AutosaveFile);
		};

		menu.OnLoadGame += fileName => {
			Log.Info($"Loading game '{fileName}' from MainMenu");
			SwitchGameScene(fileName);
		};

		menu.OnQuit += () => {
			Log.Info("Quitting application from MainMenu");
			GetTree().Quit();
		};
	}

	private async void StartNewGame() {
		AudioBus.Music.SetMuted(true);
		AudioBus.SFX.SetMuted(true);

		// Free the main menu immediately
		if(IsInstanceValid(CurrentScene)) {
			CurrentScene.QueueFree();
		}
		CurrentScene = null;

		// Load the game manager invisibly
		GameManager manager = GameWorldScene.Instantiate<GameManager>();
		SubscribeToEvents(manager);
		manager.InitGame();

		TaskCompletionSource managerReady = new();
		manager.Initialized += managerReady.SetResult;
		AddChild(manager);
		manager.HUDRef?.Hide();
		await managerReady.Task;

		// Show the splash, wait one frame for it to render, then play intro sound
		SplashPanel splashPanel = SplashPanelScene.Instantiate<SplashPanel>();
		AddChild(splashPanel);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		TaskCompletionSource introSoundDone = new();
		splashPanel.IntroSoundFinished += introSoundDone.SetResult;
		splashPanel.PlayIntroSound();
		await introSoundDone.Task;
		AudioBus.Music.SetMuted(false);
		manager.PlayGameMusic();

		// Wait for finish or skip
		TaskCompletionSource splashDone = new();
		splashPanel.Finished += splashDone.SetResult;
		await splashDone.Task;

		AudioBus.SFX.SetMuted(AudioSettings.IsMuted.Target);
		splashPanel.QueueFree();

		manager.HUDRef?.Show();
		CurrentScene = manager;
		GetTree().Paused = false;
	}

	private void SubscribeToEvents(GameManager manager) {
		manager.MainMenuRequested += () => {
			Log.Info("Returning to MainMenu from GameManager");
			SwitchMainMenu();
		};
	}
}
