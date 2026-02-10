namespace Core {
	public static class Constants {
		public const string AutosaveFile = "autosave";
	}

	public static class Numbers {
		public const float EPSILON = 0.001f;
		public const float GRAVITY = 9.8f;
	}

	public static class Actions {
		public const string MoveForward = "MoveForward";
		public const string MoveBack = "MoveBack";
		public const string MoveLeft = "MoveLeft";
		public const string MoveRight = "MoveRight";

		public const string Jump = "Jump";
		public const string Sprint = "Sprint";
		public const string Crouch = "Crouch";
		public const string Attack = "Attack";
		public const string Interact = "Interact";
		public const string Inventory = "Inventory";

		public const string MenuBack = "MenuBack";
		public const string MenuExit = "MenuExit";
		public const string CameraReset = "CameraReset";

		public const string HotbarNext = "HotbarNext";
		public const string HotbarPrev = "HotbarPrev";
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

	public static class Items {
		private const string Food = "res://Item/ItemDataBase/Food";

		public const string AppleRed = $"{Food}/AppleRed.tres";
		public const string AppleYellow = $"{Food}/AppleYellow.tres";
		public const string AppleGreen = $"{Food}/AppleGreen.tres";
		public const string BananaYellow = $"{Food}/BananaYellow.tres";
		public const string BananaGreen = $"{Food}/BananaGreen.tres";
		public const string StrawberryGreen = $"{Food}/StrawberryGreen.tres";
		public const string StrawberryRed = $"{Food}/StrawberryRed.tres";
	}

	public static class ItemID {
		public const string AppleRed = "AppleRed";
		public const string AppleYellow = "AppleYellow";
		public const string AppleGreen = "AppleGreen";
		public const string BananaYellow = "BananaYellow";
		public const string BananaGreen = "BananaGreen";
		public const string BerryBlack = "BerryBlack";
		public const string BerryGreen = "BerryGreen";
		public const string BerryRed = "BerryRed";
		public const string BlueberryBlue = "BlueberryBlue";
		public const string BlueberryGreen = "BlueberryGreen";
		public const string CherryGreen = "CherryGreen";
		public const string CherryRed = "CherryRed";
		public const string CoconutBrown = "CoconutBrown";
		public const string CoconutBrownOpen = "CoconutBrownOpen";
		public const string CoconutGreen = "CoconutGreen";
		public const string CoconutGreenOpen = "CoconutGreenOpen";
		public const string StrawberryGreen = "StrawberryGreen";
		public const string StrawberryRed = "StrawberryRed";
	}

}