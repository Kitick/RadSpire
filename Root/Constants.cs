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

public enum Group { Player, Enemy, QuestLocation }

public enum EnemyType { None, MeldoranWarrior }

public enum NPCID { None, Sera, Dag }

public enum LocationID { None, OfficeBuilding, GasStation }

public enum QuestID { LeftForDead, ArmYourself, ArmYourselfSide, ADealIsADeal }

public static class ItemID {
	public static readonly StringName AppleCider = "AppleCider";
	public static readonly StringName AppleRed = "AppleRed";
	public static readonly StringName AppleYellow = "AppleYellow";
	public static readonly StringName AppleGreen = "AppleGreen";
	public static readonly StringName BananaYellow = "BananaYellow";
	public static readonly StringName BananaGreen = "BananaGreen";
	public static readonly StringName Barrel = "Barrel";
	public static readonly StringName BarrelDrink = "BarrelDrink";
	public static readonly StringName BerryBlack = "BerryBlack";
	public static readonly StringName BerryGreen = "BerryGreen";
	public static readonly StringName BerryRed = "BerryRed";
	public static readonly StringName BlueberryBlue = "BlueberryBlue";
	public static readonly StringName BlueberryGreen = "BlueberryGreen";
	public static readonly StringName Bonfire = "Bonfire";
	public static readonly StringName Chair = "Chair";
	public static readonly StringName CherryGreen = "CherryGreen";
	public static readonly StringName CherryRed = "CherryRed";
	public static readonly StringName ChestCommon = "ChestCommon";
	public static readonly StringName ChestpieceIron = "ChestpieceIron";
	public static readonly StringName ChestPrecious = "ChestPrecious";
	public static readonly StringName ChestRare = "ChestRare";
	public static readonly StringName Coffee = "Coffee";
	public static readonly StringName CoconutBrown = "CoconutBrown";
	public static readonly StringName CoconutBrownOpen = "CoconutBrownOpen";
	public static readonly StringName CoconutGreen = "CoconutGreen";
	public static readonly StringName CoconutGreenOpen = "CoconutGreenOpen";
	public static readonly StringName Counter = "Counter";
	public static readonly StringName Crate = "Crate";
	public static readonly StringName CrateBig = "CrateBig";
	public static readonly StringName FirePlace = "FirePlace";
	public static readonly StringName GoldBar = "GoldBar";
	public static readonly StringName GoldChunk = "GoldChunk";
	public static readonly StringName GoldOre = "GoldOre";
	public static readonly StringName HeadpieceIron = "HeadpieceIron";
	public static readonly StringName House1 = "House1";
	public static readonly StringName HouseMedium = "HouseMedium";
	public static readonly StringName HouseSmall = "HouseSmall";
	public static readonly StringName IronBar = "IronBar";
	public static readonly StringName IronChunk = "IronChunk";
	public static readonly StringName IronOre = "IronOre";
	public static readonly StringName Overhang = "Overhang";
	public static readonly StringName PantpieceIron = "PantpieceIron";
	public static readonly StringName ReturnDoor = "ReturnDoor";
	public static readonly StringName ReturnDoor2 = "ReturnDoor2";
	public static readonly StringName ReturnDoorTent = "ReturnDoorTent";
	public static readonly StringName ShelfBigDown = "ShelfBigDown";
	public static readonly StringName ShelfBigUp = "ShelfBigUp";
	public static readonly StringName ShieldIron = "ShieldIron";
	public static readonly StringName ShieldWood = "ShieldWood";
	public static readonly StringName ShelfSmallDown = "ShelfSmallDown";
	public static readonly StringName ShelfSmallUp = "ShelfSmallUp";
	public static readonly StringName Stick = "Stick";
	public static readonly StringName Stone = "Stone";
	public static readonly StringName StonePiece = "StonePiece";
	public static readonly StringName StrawberryGreen = "StrawberryGreen";
	public static readonly StringName StrawberryRed = "StrawberryRed";
	public static readonly StringName SwordGold = "SwordGold";
	public static readonly StringName SwordIron = "SwordIron";
	public static readonly StringName SwordWood = "SwordWood";
	public static readonly StringName Table = "Table";
	public static readonly StringName TableCoffeeBlack = "TableCoffeeBlack";
	public static readonly StringName TableCoffeeBrown = "TableCoffeeBrown";
	public static readonly StringName TableCoffeeGray = "TableCoffeeGray";
	public static readonly StringName Tent = "Tent";
	public static readonly StringName Wood = "Wood";
}
