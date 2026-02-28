using Core;
using Godot;
using Services;
using Services.Settings;
using UI;

namespace Root {
	public partial class SceneDirector : Node {
		private static readonly LogService Log = new(nameof(SceneDirector), enabled: true);

		[ExportCategory("Scene References")]
		[Export] private PackedScene MainMenuScene = null!;
		[Export] private PackedScene GameWorldScene = null!;

		private Node? CurrentScene;

		public override void _Ready() {
			SettingSystem.Load();
			SettingSystem.Apply();

			SwitchMainMenu();
		}

		private void SwitchScene(Node to) => SwitchScene(CurrentScene, to);
		private void SwitchScene(Node? from, Node to) {
			CurrentScene = to;
			from?.QueueFree();
			AddChild(to);
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
				SwitchGameScene();
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

		private void SubscribeToEvents(GameManager manager) {
			manager.MainMenuRequested += () => {
				Log.Info("Returning to MainMenu from GameManager");
				SwitchMainMenu();
			};
		}
	}
}