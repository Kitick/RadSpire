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
			new(ItemID.Wood, 4),
				new(ItemID.IronBar, 1)
		],
		Outputs: [new(ItemID.ChestCommon, 1)]
	);

	public static readonly CraftingRecipe CraftChestRare = new(
		RecipeName: "Craft Rare Chest",
		Inputs: [
			new(ItemID.ChestCommon, 1),
				new(ItemID.IronBar, 3),
				new(ItemID.Stone, 2)
		],
		Outputs: [new(ItemID.ChestRare, 1)]
	);

	public static readonly CraftingRecipe CraftChestPrecious = new(
		RecipeName: "Craft Precious Chest",
		Inputs: [
			new(ItemID.ChestRare, 1),
				new(ItemID.GoldBar, 3)
		],
		Outputs: [new(ItemID.ChestPrecious, 1)]
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
	];
}
