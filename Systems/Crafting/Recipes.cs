namespace Services.Crafting {
	using Core;
	using Godot;

	public readonly record struct RecipeItem(StringName ItemId, int Quantity);

	public record CraftingRecipe(
		StringName RecipeName,
		RecipeItem[] Inputs,
		RecipeItem[] Outputs
	);

	public static class Recipes {
		public static readonly CraftingRecipe Sundae = new(
			RecipeName: "Sundae",
			Inputs: [
				new(ItemID.BananaYellow, 1),
				new(ItemID.StrawberryRed, 1),
				new(ItemID.BananaGreen, 1)
			],
			Outputs: [new(ItemID.Bonfire, 1)]
		);

		public static readonly CraftingRecipe[] AllRecipes = [
			Sundae
		];
	}
}
