namespace Constants {
	public static class Numbers {
		public const float EPSILON = 0.001f;
		public const float GRAVITY = 9.8f;
	}

	public static class Scenes {
		public const string PauseMenu = "res://HUD/pause_menu.tscn";
		public const string HUD = "res://HUD/UI.tscn";

		public const string MainMenu = "res://Main Menu/Main_Menu.tscn";
		public const string SettingsMenu = "res://Settings Menu/Settings_Menu.tscn";

		public const string GameScene = "res://Initial Scene/initial_player_scene.tscn";
		public const string Player = "res://Character/Player/Player.tscn";

		public const string Camera = "res://Camera/TopDownCamera.tscn";
	}

	public static class Actions {
		public const string UICancel = "ui_cancel";

		public const string Forward = "move_forward";
		public const string Back = "move_back";
		public const string Left = "move_left";
		public const string Right = "move_right";

		public const string Jump = "jump";
		public const string Sprint = "sprint";
		public const string Crouch = "crouch";

		public const string CameraReset = "camera_reset";

		public const string Hotbar1 = "hotbar_1";
		public const string Hotbar2 = "hotbar_2";
		public const string Hotbar3 = "hotbar_3";
		public const string Hotbar4 = "hotbar_4";
		public const string Hotbar5 = "hotbar_5";
	}
}