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

public static class ItemID {
	public readonly static StringName AppleRed = "AppleRed";
	public readonly static StringName AppleYellow = "AppleYellow";
	public readonly static StringName AppleGreen = "AppleGreen";
	public readonly static StringName BananaYellow = "BananaYellow";
	public readonly static StringName BananaGreen = "BananaGreen";
	public readonly static StringName Barrel = "Barrel";
	public readonly static StringName BerryBlack = "BerryBlack";
	public readonly static StringName BerryGreen = "BerryGreen";
	public readonly static StringName BerryRed = "BerryRed";
	public readonly static StringName BlueberryBlue = "BlueberryBlue";
	public readonly static StringName BlueberryGreen = "BlueberryGreen";
	public readonly static StringName Bonfire = "Bonfire";
	public readonly static StringName CherryGreen = "CherryGreen";
	public readonly static StringName CherryRed = "CherryRed";
	public readonly static StringName ChestCommon = "ChestCommon";
	public readonly static StringName ChestpieceIron = "ChestpieceIron";
	public readonly static StringName ChestPrecious = "ChestPrecious";
	public readonly static StringName ChestRare = "ChestRare";
	public readonly static StringName CoconutBrown = "CoconutBrown";
	public readonly static StringName CoconutBrownOpen = "CoconutBrownOpen";
	public readonly static StringName CoconutGreen = "CoconutGreen";
	public readonly static StringName CoconutGreenOpen = "CoconutGreenOpen";
	public readonly static StringName GoldBar = "GoldBar";
	public readonly static StringName GoldChunk = "GoldChunk";
	public readonly static StringName GoldOre = "GoldOre";
	public readonly static StringName HeadpieceIron = "HeadpieceIron";
	public readonly static StringName IronBar = "IronBar";
	public readonly static StringName IronChunk = "IronChunk";
	public readonly static StringName IronOre = "IronOre";
	public readonly static StringName PantpieceIron = "PantpieceIron";
	public readonly static StringName ShieldIron = "ShieldIron";
	public readonly static StringName ShieldWood = "ShieldWood";
	public readonly static StringName Stick = "Stick";
	public readonly static StringName Stone = "Stone";
	public readonly static StringName StonePiece = "StonePiece";
	public readonly static StringName StrawberryGreen = "StrawberryGreen";
	public readonly static StringName StrawberryRed = "StrawberryRed";
	public readonly static StringName SwordGold = "SwordGold";
	public readonly static StringName SwordIron = "SwordIron";
	public readonly static StringName SwordWood = "SwordWood";
	public readonly static StringName Wood = "Wood";
}
