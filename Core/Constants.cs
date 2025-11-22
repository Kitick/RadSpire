namespace Core {
	public static class Numbers {
		public const float EPSILON = 0.001f;
		public const float GRAVITY = 9.8f;
	}

	public static class Scenes {
		public const string PauseMenu = "res://UI/HUD/PauseMenu.tscn";
		public const string HUD = "res://UI/HUD/HUD.tscn";

		public const string MainMenu = "res://UI/MainMenu/MainMenu.tscn";
		public const string SettingsMenu = "res://UI/SettingsMenu/SettingsMenu.tscn";

		public const string GameScene = "res://GameWorld/World.tscn";
		public const string Player = "res://Character/Player/Player.tscn";

		public const string Camera = "res://Camera/Camera.tscn";
	}

	public static class Actions {
		public const string MoveForward = "MoveForward";
		public const string MoveBack = "MoveBack";
		public const string MoveLeft = "MoveLeft";
		public const string MoveRight = "MoveRight";

		public const string Jump = "Jump";
		public const string Sprint = "Sprint";
		public const string Crouch = "Crouch";

		public const string MenuBack = "MenuBack";
		public const string MenuExit = "MenuExit";
		public const string CameraReset = "CameraReset";

		public const string HotbarNext = "HotbarNext";
		public const string HotbarPrev = "HobarPrev";
		public const string Hotbar1 = "Hotbar1";
		public const string Hotbar2 = "Hotbar2";
		public const string Hotbar3 = "Hotbar3";
		public const string Hotbar4 = "Hotbar4";
		public const string Hotbar5 = "Hotbar5";

		public const string CameraPan = "PanCamera";
		public const string CameraRotate = "RotateCamera";
		public const string ZoomIn = "ZoomIn";
		public const string ZoomOut = "ZoomOut";
	}
}