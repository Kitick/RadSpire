namespace Crafting;

using Godot;
using Root;

public readonly record struct RecipeItem(StringName ItemId, int Quantity);

public record CraftingRecipe(
	StringName RecipeName,
	RecipeItem[] Inputs,
	RecipeItem[] Outputs
) {
	public override string ToString() => RecipeName;
}

public static class Recipes {
	public static readonly CraftingRecipe CrackCoconutBrown = new(
		RecipeName: "Crack Brown Coconut",
		Inputs: [
			new(ItemID.CoconutBrown, 1),
				new(ItemID.Stone, 1)
		],
		Outputs: [
			new(ItemID.CoconutBrownOpen, 1),
				new(ItemID.Stone, 1)
		]
	);

	public static readonly CraftingRecipe CrackCoconutGreen = new(
		RecipeName: "Crack Green Coconut",
		Inputs: [
			new(ItemID.CoconutGreen, 1),
				new(ItemID.Stone, 1)
		],
		Outputs: [
			new(ItemID.CoconutGreenOpen, 1),
				new(ItemID.Stone, 1)
		]
	);

	// --- Smelting / Processing ---

	public static readonly CraftingRecipe SmeltIronChunk = new(
		RecipeName: "Process Iron Chunk",
		Inputs: [new(ItemID.IronOre, 1), new(ItemID.Wood, 1)],
		Outputs: [new(ItemID.IronChunk, 1)]
	);

	public static readonly CraftingRecipe SmeltIronBar = new(
		RecipeName: "Smelt Iron Bar",
		Inputs: [new(ItemID.IronChunk, 2), new(ItemID.Wood, 1)],
		Outputs: [new(ItemID.IronBar, 1)]
	);

	public static readonly CraftingRecipe SmeltGoldChunk = new(
		RecipeName: "Process Gold Chunk",
		Inputs: [new(ItemID.GoldOre, 2), new(ItemID.Wood, 1)],
		Outputs: [new(ItemID.GoldChunk, 1)]
	);

	public static readonly CraftingRecipe SmeltGoldBar = new(
		RecipeName: "Smelt Gold Bar",
		Inputs: [new(ItemID.GoldChunk, 2), new(ItemID.Wood, 1)],
		Outputs: [new(ItemID.GoldBar, 1)]
	);

	// --- Weapons ---

	public static readonly CraftingRecipe CraftSwordIron = new(
		RecipeName: "Craft Iron Sword",
		Inputs: [
			new(ItemID.IronBar, 2),
				new(ItemID.Stick, 2),
				new(ItemID.Stone, 1),
		],
		Outputs: [new(ItemID.SwordIron, 1)]
	);

	// --- Shields ---

	public static readonly CraftingRecipe CraftShieldIron = new(
		RecipeName: "Craft Iron Shield",
		Inputs: [
			new(ItemID.Wood, 2),
				new(ItemID.IronBar, 1)
		],
		Outputs: [new(ItemID.ShieldIron, 1)]
	);

	// --- Armor ---

	public static readonly CraftingRecipe CraftHeadpieceIron = new(
		RecipeName: "Craft Iron Headpiece",
		Inputs: [new(ItemID.IronBar, 3)],
		Outputs: [new(ItemID.HeadpieceIron, 1)]
	);

	public static readonly CraftingRecipe CraftChestpieceIron = new(
		RecipeName: "Craft Iron Chestpiece",
		Inputs: [new(ItemID.IronBar, 5)],
		Outputs: [new(ItemID.ChestpieceIron, 1)]
	);

	public static readonly CraftingRecipe CraftPantpieceIron = new(
		RecipeName: "Craft Iron Pants",
		Inputs: [new(ItemID.IronBar, 4)],
		Outputs: [new(ItemID.PantpieceIron, 1)]
	);

	// --- Structures / Storage ---

	public static readonly CraftingRecipe CraftBonfire = new(
		RecipeName: "Craft Bonfire",
		Inputs: [
			new(ItemID.Wood, 3),
				new(ItemID.Stick, 2),
				new(ItemID.Stone, 2)
		],
		Outputs: [new(ItemID.Bonfire, 1)]
	);

	public static readonly CraftingRecipe CraftBarrel = new(
		RecipeName: "Craft Barrel",
		Inputs: [
			new(ItemID.Wood, 5),
				new(ItemID.IronBar, 1)
		],
		Outputs: [new(ItemID.Barrel, 1)]
	);

	public static readonly CraftingRecipe CraftChestCommon = new(
		RecipeName: "Craft Common Chest",
		Inputs: [
			new(ItemID.Wood, 6),
				new(ItemID.Stick, 2)
		],
		Outputs: [new(ItemID.ChestCommon, 1)]
	);

	public static readonly CraftingRecipe CraftChestRare = new(
		RecipeName: "Craft Rare Chest",
		Inputs: [
			new(ItemID.ChestCommon, 1),
				new(ItemID.IronBar, 4),
				new(ItemID.Stone, 6)
		],
		Outputs: [new(ItemID.ChestRare, 1)]
	);

	public static readonly CraftingRecipe CraftChestPrecious = new(
		RecipeName: "Craft Precious Chest",
		Inputs: [
			new(ItemID.ChestRare, 1),
				new(ItemID.GoldBar, 5),
				new(ItemID.IronBar, 2)
		],
		Outputs: [new(ItemID.ChestPrecious, 1)]
	);

	public static readonly CraftingRecipe CraftChair = new(
		RecipeName: "Craft Wooden Chair",
		Inputs: [new(ItemID.Wood, 2), new(ItemID.Stick, 1)],
		Outputs: [new(ItemID.Chair, 1)]
	);

	public static readonly CraftingRecipe CraftTable = new(
		RecipeName: "Craft Table",
		Inputs: [new(ItemID.Wood, 4), new(ItemID.Stick, 2)],
		Outputs: [new(ItemID.Table, 1)]
	);

	public static readonly CraftingRecipe CraftCounter = new(
		RecipeName: "Craft Wooden Counter",
		Inputs: [new(ItemID.Wood, 5), new(ItemID.IronBar, 1)],
		Outputs: [new(ItemID.Counter, 1)]
	);

	public static readonly CraftingRecipe CraftCrate = new(
		RecipeName: "Craft Crate",
		Inputs: [new(ItemID.Wood, 3), new(ItemID.Stick, 2)],
		Outputs: [new(ItemID.Crate, 1)]
	);

	public static readonly CraftingRecipe CraftCrateBig = new(
		RecipeName: "Craft Large Crate",
		Inputs: [new(ItemID.Crate, 1), new(ItemID.Wood, 3), new(ItemID.IronBar, 1)],
		Outputs: [new(ItemID.CrateBig, 1)]
	);

	public static readonly CraftingRecipe CraftFirePlace = new(
		RecipeName: "Craft Fireplace",
		Inputs: [new(ItemID.Stone, 10), new(ItemID.Wood, 2)],
		Outputs: [new(ItemID.FirePlace, 1)]
	);

	public static readonly CraftingRecipe CraftOverhang = new(
		RecipeName: "Craft Overhang",
		Inputs: [new(ItemID.Wood, 4), new(ItemID.Stick, 3)],
		Outputs: [new(ItemID.Overhang, 1)]
	);

	public static readonly CraftingRecipe CraftShelfSmallDown = new(
		RecipeName: "Craft Small Shelf (Lower)",
		Inputs: [new(ItemID.Wood, 2), new(ItemID.Stick, 2)],
		Outputs: [new(ItemID.ShelfSmallDown, 1)]
	);

	public static readonly CraftingRecipe CraftShelfSmallUp = new(
		RecipeName: "Craft Small Shelf (Upper)",
		Inputs: [new(ItemID.Wood, 2), new(ItemID.Stick, 2)],
		Outputs: [new(ItemID.ShelfSmallUp, 1)]
	);

	public static readonly CraftingRecipe CraftShelfBigDown = new(
		RecipeName: "Craft Large Shelf (Lower)",
		Inputs: [new(ItemID.ShelfSmallDown, 1), new(ItemID.Wood, 3), new(ItemID.IronBar, 1)],
		Outputs: [new(ItemID.ShelfBigDown, 1)]
	);

	public static readonly CraftingRecipe CraftShelfBigUp = new(
		RecipeName: "Craft Large Shelf (Upper)",
		Inputs: [new(ItemID.ShelfSmallUp, 1), new(ItemID.Wood, 3), new(ItemID.IronBar, 1)],
		Outputs: [new(ItemID.ShelfBigUp, 1)]
	);

	public static readonly CraftingRecipe CraftCoffeeTableBrown = new(
		RecipeName: "Craft Coffee Table (Brown)",
		Inputs: [new(ItemID.Wood, 3), new(ItemID.Stick, 2)],
		Outputs: [new(ItemID.TableCoffeeBrown, 1)]
	);

	public static readonly CraftingRecipe CraftCoffeeTableGray = new(
		RecipeName: "Craft Coffee Table (Gray)",
		Inputs: [new(ItemID.TableCoffeeBrown, 1), new(ItemID.Stone, 3)],
		Outputs: [new(ItemID.TableCoffeeGray, 1)]
	);

	public static readonly CraftingRecipe CraftCoffeeTableBlack = new(
		RecipeName: "Craft Coffee Table (Black)",
		Inputs: [new(ItemID.TableCoffeeBrown, 1), new(ItemID.IronBar, 2)],
		Outputs: [new(ItemID.TableCoffeeBlack, 1)]
	);

	public static readonly CraftingRecipe CraftBarrelDrink = new(
		RecipeName: "Craft Drink Barrel",
		Inputs: [new(ItemID.Barrel, 1), new(ItemID.Wood, 2), new(ItemID.IronBar, 1)],
		Outputs: [new(ItemID.BarrelDrink, 1)]
	);

	public static readonly CraftingRecipe CraftAppleCider = new(
		RecipeName: "Craft Apple Cider",
		Inputs: [new(ItemID.BarrelDrink, 1), new(ItemID.AppleRed, 2), new(ItemID.AppleGreen, 2)],
		Outputs: [new(ItemID.AppleCider, 1)]
	);

	public static readonly CraftingRecipe CraftCoffee = new(
		RecipeName: "Craft Coffee Setup",
		Inputs: [new(ItemID.BarrelDrink, 1), new(ItemID.Wood, 2), new(ItemID.Stone, 1)],
		Outputs: [new(ItemID.Coffee, 1)]
	);

	public static readonly CraftingRecipe CraftTent = new(
		RecipeName: "Craft Tent",
		Inputs: [new(ItemID.Wood, 8), new(ItemID.Stick, 6)],
		Outputs: [new(ItemID.Tent, 1)]
	);

	public static readonly CraftingRecipe CraftHouseSmall = new(
		RecipeName: "Craft Small House",
		Inputs: [new(ItemID.Wood, 20), new(ItemID.Stone, 12), new(ItemID.Stick, 8)],
		Outputs: [new(ItemID.HouseSmall, 1)]
	);

	public static readonly CraftingRecipe CraftHouseMedium = new(
		RecipeName: "Craft Medium House",
		Inputs: [new(ItemID.Wood, 32), new(ItemID.Stone, 20), new(ItemID.IronBar, 6), new(ItemID.Stick, 10)],
		Outputs: [new(ItemID.HouseMedium, 1)]
	);

	public static readonly CraftingRecipe[] AllRecipes = [
		SmeltIronChunk,
		SmeltIronBar,
		SmeltGoldChunk,
		SmeltGoldBar,
		CrackCoconutBrown,
		CrackCoconutGreen,
		CraftSwordIron,
		CraftShieldIron,
		CraftHeadpieceIron,
		CraftChestpieceIron,
		CraftPantpieceIron,
		CraftBonfire,
		CraftBarrel,
		CraftChestCommon,
		CraftChestRare,
		CraftChestPrecious,
		CraftChair,
		CraftTable,
		CraftCounter,
		CraftCrate,
		CraftCrateBig,
		CraftFirePlace,
		CraftOverhang,
		CraftShelfSmallDown,
		CraftShelfSmallUp,
		CraftShelfBigDown,
		CraftShelfBigUp,
		CraftCoffeeTableBrown,
		CraftCoffeeTableGray,
		CraftCoffeeTableBlack,
		CraftBarrelDrink,
		CraftAppleCider,
		CraftCoffee,
		CraftTent,
		CraftHouseSmall,
		CraftHouseMedium,
	];
}
