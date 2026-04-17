namespace Crafting;

using InventorySystem;

public enum CraftStatus {
	Success,
	InvalidRecipe,
	MissingIngredients,
	OutputInventoryFull,
}

public readonly struct CraftResult {
	public CraftStatus Status { get; init; }
	public ItemSlot[] Items { get; init; }
	public RecipeItem[] Missing { get; init; }

	public static CraftResult Ok(ItemSlot[] items) => new() {
		Status = CraftStatus.Success,
		Items = items,
		Missing = [],
	};

	public static CraftResult Fail(CraftStatus status, RecipeItem[]? missing = null) => new() {
		Status = status,
		Items = [],
		Missing = missing ?? [],
	};
}
