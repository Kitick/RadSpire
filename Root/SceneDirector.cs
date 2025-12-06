using Godot;

namespace Root {
	public partial class SceneDirector : Node {
		private static readonly Logger Log = new(nameof(SceneDirector), enabled: true);

		public enum MenuState { MainMenu, Settings }

		public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
		public Node? CurrentScene { get; private set; }

		[ExportGroup("Menu Scenes")]
		[Export] private PackedScene MainMenu = null!;
		[Export] private PackedScene Settings = null!;

		[ExportGroup("Game Scenes")]
		[Export] private PackedScene GameWorld = null!;

		public override void _Ready() {
			ChangeState(CurrentState);
		}

		private void ChangeState(MenuState to) => ChangeState(CurrentState, to);
		private void ChangeState(MenuState from, MenuState to) {
			CurrentState = to;

			if(to == MenuState.MainMenu) {
				SwitchScene(MainMenu.Instantiate<MainMenu>());
			}
		}

		private void SwitchScene(Node to) => SwitchScene(CurrentScene, to);
		private void SwitchScene(Node? from, Node to) {
			CurrentScene = to;

			from?.QueueFree();
			AddChild(to);
		}
	}
}