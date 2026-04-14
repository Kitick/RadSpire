namespace Root;

using Godot;

public static class Constants {
	public const string AutosaveFile = "autosave";
}

public static class Numbers {
	public const float EPSILON = 0.001f;
	public const float GRAVITY = 9.8f;
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

public static class QuestID {
	public static readonly StringName Stage0_KillPatrol = "Stage0_KillPatrol";
}

public static class ItemID {
	public static readonly StringName AppleRed = "AppleRed";
	public static readonly StringName AppleYellow = "AppleYellow";
	public static readonly StringName AppleGreen = "AppleGreen";
	public static readonly StringName BananaYellow = "BananaYellow";
	public static readonly StringName BananaGreen = "BananaGreen";
	public static readonly StringName Barrel = "Barrel";
	public static readonly StringName BerryBlack = "BerryBlack";
	public static readonly StringName BerryGreen = "BerryGreen";
	public static readonly StringName BerryRed = "BerryRed";
	public static readonly StringName BlueberryBlue = "BlueberryBlue";
	public static readonly StringName BlueberryGreen = "BlueberryGreen";
	public static readonly StringName Bonfire = "Bonfire";
	public static readonly StringName CherryGreen = "CherryGreen";
	public static readonly StringName CherryRed = "CherryRed";
	public static readonly StringName ChestCommon = "ChestCommon";
	public static readonly StringName ChestpieceIron = "ChestpieceIron";
	public static readonly StringName ChestPrecious = "ChestPrecious";
	public static readonly StringName ChestRare = "ChestRare";
	public static readonly StringName CoconutBrown = "CoconutBrown";
	public static readonly StringName CoconutBrownOpen = "CoconutBrownOpen";
	public static readonly StringName CoconutGreen = "CoconutGreen";
	public static readonly StringName CoconutGreenOpen = "CoconutGreenOpen";
	public static readonly StringName GoldBar = "GoldBar";
	public static readonly StringName GoldChunk = "GoldChunk";
	public static readonly StringName GoldOre = "GoldOre";
	public static readonly StringName HeadpieceIron = "HeadpieceIron";
	public static readonly StringName IronBar = "IronBar";
	public static readonly StringName IronChunk = "IronChunk";
	public static readonly StringName IronOre = "IronOre";
	public static readonly StringName PantpieceIron = "PantpieceIron";
	public static readonly StringName ShieldIron = "ShieldIron";
	public static readonly StringName ShieldWood = "ShieldWood";
	public static readonly StringName Stick = "Stick";
	public static readonly StringName Stone = "Stone";
	public static readonly StringName StonePiece = "StonePiece";
	public static readonly StringName StrawberryGreen = "StrawberryGreen";
	public static readonly StringName StrawberryRed = "StrawberryRed";
	public static readonly StringName SwordGold = "SwordGold";
	public static readonly StringName SwordIron = "SwordIron";
	public static readonly StringName SwordWood = "SwordWood";
	public static readonly StringName Wood = "Wood";
}
