namespace Services.Crafting {
	using ItemSystem;

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

		public static CraftResult Ok(ItemSlot[] items) => new CraftResult {
			Status = CraftStatus.Success,
			Items = items,
			Missing = [],
		};

		public static CraftResult Fail(CraftStatus status, RecipeItem[]? missing = null) => new CraftResult {
			Status = status,
			Items = [],
			Missing = missing ?? [],
		};
	}
}
