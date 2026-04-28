namespace Crafting;

using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using ItemSystem;
using Services;

public static class CraftingSystem {
	private static readonly LogService Log = new(nameof(CraftingSystem), enabled: true);

	public static bool TryCraft(CraftingRecipe recipe, IEnumerable<Inventory> sources, out ItemSlot[] outputs) {
		CraftResult result = Craft(recipe, sources);
		outputs = result.Items;
		return result.Status == CraftStatus.Success;
	}

	public static bool CanCraft(CraftingRecipe recipe, IEnumerable<Inventory> sources, out RecipeItem[] missing, int quantity = 1) {
		List<RecipeItem> missingList = [];

		foreach(RecipeItem ingredient in recipe.Inputs) {
			int available = CountAvailable(ingredient.ItemId, sources);
			if(available < ingredient.Quantity * quantity) { missingList.Add(ingredient); }
		}

		missing = [.. missingList];
		return missingList.Count == 0;
	}

	public static CraftResult Craft(CraftingRecipe recipe, params IEnumerable<Inventory> sources) {
		Inventory[] inventories = [.. sources];
		if(!CanCraft(recipe, inventories, out RecipeItem[]? missing)) {
			Log.Warn($"Craft failed for '{recipe.RecipeName}': {missing.Length} ingredient(s) missing.");
			return CraftResult.Fail(CraftStatus.MissingIngredients, missing);
		}

		ItemSlot[] outputs = BuildOutputs(recipe);
		ConsumeIngredients(recipe, inventories);

		Log.Info($"Crafted '{recipe.RecipeName}': {outputs.Length} output slot(s).");
		return CraftResult.Ok(outputs);
	}

	public static int CountAvailable(string itemId, IEnumerable<Inventory> sources) {
		return sources.SelectMany(inv => inv.ItemSlots)
			.Where(slot => !slot.IsEmpty() && slot.Item!.Id == itemId)
			.Sum(slot => slot.Quantity);
	}

	private static Item? FindItem(string itemId, Inventory inventory) =>
		inventory.ItemSlots.FirstOrDefault(slot => !slot.IsEmpty() && slot.Item!.Id == itemId)?.Item;

	private static void ConsumeIngredients(CraftingRecipe recipe, params IEnumerable<Inventory> sources) {
		foreach(RecipeItem ingredient in recipe.Inputs) {
			int remaining = ingredient.Quantity;

			foreach(Inventory inventory in sources) {
				if(remaining <= 0) { break; }

				Item? item = FindItem(ingredient.ItemId, inventory);
				if(item == null) { continue; }

				// RemoveItem(ItemSlot) requires the full quantity to exist in that
				// one inventory, so we cap at what is actually available here.
				int available = inventory.GetTotalQuantity(item);
				int toRemove = Math.Min(remaining, available);
				if(toRemove > 0) {
					inventory.RemoveItem(new ItemSlot(item, toRemove));
					remaining -= toRemove;
				}
			}
		}
	}

	private static ItemSlot[] BuildOutputs(CraftingRecipe recipe) {
		List<ItemSlot> outputs = [];

		foreach(RecipeItem output in recipe.Outputs) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(output.ItemId);
			if(item == null) {
				Log.Error($"BuildOutputs: failed to create item with ID '{output.ItemId}'. Output skipped.");
				continue;
			}
			outputs.Add(new ItemSlot(item, output.Quantity));
		}

		return [.. outputs];
	}
}
