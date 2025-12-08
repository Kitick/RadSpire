using Core;
using Godot;

namespace Root {
	public partial class SceneDirector : Node {
		private static readonly Logger Log = new(nameof(SceneDirector), enabled: true);

		public Node? CurrentScene { get; private set; }

		[ExportCategory("Scene References")]
		[Export] private PackedScene MainMenuScene = null!;
		[Export] private PackedScene GameWorldScene = null!;

		public override void _Ready() {
			ShowMainMenu();
		}

		private void ShowMainMenu() {
			var mainMenu = MainMenuScene.Instantiate<MainMenu>();
			SubscribeToMainMenuEvents(mainMenu);
			SwitchScene(mainMenu);
		}

		private void SubscribeToMainMenuEvents(MainMenu mainMenu) {
			mainMenu.OnStartNewGame += () => {
				Log.Info("Starting new game from MainMenu");
				GameManager.Instance.StartNewGame();
			};

			mainMenu.OnContinueGame += () => {
				Log.Info("Continuing game from MainMenu");
				GameManager.Instance.ContinueGame();
			};

			mainMenu.OnLoadGame += fileName => {
				Log.Info($"Loading game '{fileName}' from MainMenu");
				GameManager.Instance.LoadGame(fileName);
			};

			mainMenu.OnQuit += () => {
				Log.Info("Quitting application from MainMenu");
				GameManager.Instance.ExitApplication();
			};
		}

		private void SwitchScene(Node to) => SwitchScene(CurrentScene, to);
		private void SwitchScene(Node? from, Node to) {
			Log.Info($"Switching scene from {from?.Name ?? "null"} to {to.Name}");
			CurrentScene = to;

			from?.QueueFree();
			AddChild(to);
		}
	}
}