using Core;
using Godot;
using Settings;

namespace Root {
	public partial class SceneDirector : Node {
		private static readonly Logger Log = new(nameof(SceneDirector), enabled: true);

		public enum MenuState { MainMenu, Settings }

		private readonly StateMachine<MenuState> StateMachine = new();

		public MenuState CurrentState => StateMachine.CurrentState;
		public Node? CurrentScene { get; private set; }

		[ExportGroup("Menu Scenes")]
		[Export] private PackedScene MainMenu = null!;
		[Export] private PackedScene Settings = null!;

		[ExportGroup("Game Scenes")]
		[Export] private PackedScene GameWorld = null!;

		public SceneDirector() {
			SetupStateMachine();
		}

		public override void _Ready() {
			StateMachine.TransitionTo(MenuState.MainMenu);
		}

		private void SetupStateMachine() {
			StateMachine.OnEnter(MenuState.MainMenu, () => SwitchScene(MainMenu.Instantiate<MainMenu>()));

			StateMachine.OnEnter(MenuState.Settings, () => {
				Log.Info("Entering Settings Menu");
				Settings.Instantiate<SettingsMenu>().OpenMenu(
					onClose: () => StateMachine.TransitionTo(MenuState.MainMenu)
				);
			});
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