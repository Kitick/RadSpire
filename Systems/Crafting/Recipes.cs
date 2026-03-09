using Core;
using ItemSystem;

namespace Services.Crafting {
	public readonly record struct RecipeItem(string ItemId, int Quantity);

	public record CraftingRecipe(
		string RecipeName,
		RecipeItem[] Inputs,
		RecipeItem[] Outputs
	);

	public static class Recipes {
		// Crack open a brown coconut to get the opened version with higher heal value.
		public static readonly CraftingRecipe CrackBrownCoconut = new(
			RecipeName: "Crack Brown Coconut",
			Inputs:  [new("CoconutBrown", 1)],
			Outputs: [new("CoconutBrownOpen", 1)]
		);

		// Crack open a green coconut for a refreshing drink.
		public static readonly CraftingRecipe CrackGreenCoconut = new(
			RecipeName: "Crack Green Coconut",
			Inputs:  [new("CoconutGreen", 1)],
			Outputs: [new("CoconutGreenOpen", 1)]
		);

		// Combine mixed berries into a blueberry.
		public static readonly CraftingRecipe MixedBerryBowl = new(
			RecipeName: "Mixed Berry Bowl",
			Inputs: [
				new("BerryRed", 2),
				new("BerryBlack", 2),
				new("BerryGreen", 2),
			],
			Outputs: [new("BlueberryBlue", 3)]
		);

		public static readonly CraftingRecipe Sundae = new(
			RecipeName: "Sundae",
			Inputs: [
				new(ItemID.BananaYellow, 1),
				new(ItemID.StrawberryRed, 1),
				new(ItemID.BananaGreen, 1)
			],
			Outputs: [new(ItemID.Bonfire, 2)]
		);
				

		public static readonly CraftingRecipe[] AllRecipes = new[] {
			CrackBrownCoconut,
			CrackGreenCoconut,
			MixedBerryBowl,
			Sundae
		};
	}
}
