namespace Crafting.Interface;

using System;
using System.Collections.Generic;
using Crafting;
using Godot;
using InventorySystem;
using Root;
using Services;

public sealed partial class CraftingUI : Control {
	private static readonly LogService Log = new(nameof(CraftingUI), enabled: true);

	[Export] private OptionButton CraftableDropdown = null!;
	[Export] private ItemList RequirementsList = null!;
	[Export] private Button CraftButton = null!;
	[Export] private LineEdit QuantityDisplay = null!;
	[Export] private Button PosButton = null!;
	[Export] private Button NegButton = null!;
	[Export] private Control MainBackground = null!;
	[Export] private Control LightBackground = null!;

	private readonly List<CraftingRecipe> CraftableRecipes = [];
	private readonly List<CraftingRecipe> NonCraftableRecipes = [];

	public readonly List<Inventory> Inventories = [];

	private CraftingRecipe? SelectedRecipe;

	private int Quantity {
		get;
		set {
			field = Math.Clamp(value, 1, 10);
			RefreshQuantity();
		}
	} = 1;

	public override void _Ready() {
		this.ValidateExports();
		ConfigureMouseFilters();
		SetCallbacks();
	}

	private void ConfigureMouseFilters() {
		MouseFilter = MouseFilterEnum.Ignore;
		MainBackground.MouseFilter = MouseFilterEnum.Ignore;
		LightBackground.MouseFilter = MouseFilterEnum.Ignore;
	}

	private void SetCallbacks() {
		CraftButton.Pressed += OnCraftButtonPressed;
		PosButton.Pressed += () => Quantity++;
		NegButton.Pressed += () => Quantity--;

		CraftableDropdown.ItemSelected += (_) => OnCraftableSelected();
	}

	private void OnCraftableSelected() {
		SelectedRecipe = CraftableDropdown.GetSelectedItem(CraftableRecipes);
		UpdateSelectedRequirements();
	}

	public void RefreshUI() {
		CraftableRecipes.Clear();
		NonCraftableRecipes.Clear();

		foreach(CraftingRecipe recipe in Recipes.AllRecipes) {
			if(CraftingSystem.CanCraft(recipe, Inventories, out _)) {
				CraftableRecipes.Add(recipe);
			} else {
				NonCraftableRecipes.Add(recipe);
			}
		}

		CraftableDropdown.Populate(CraftableRecipes);

		if(CraftableRecipes.Count > 0) {
			SelectedRecipe = CraftableRecipes[0];
		} else if(NonCraftableRecipes.Count > 0) {
			SelectedRecipe = NonCraftableRecipes[0];
		} else {
			SelectedRecipe = null;
		}

		RefreshQuantity();
	}

	private void RefreshQuantity() {
		QuantityDisplay.Text = Quantity.ToString();
		UpdateSelectedRequirements();
	}

	private void UpdateRequirementsList(CraftingRecipe recipe) {
		RequirementsList.Clear();
		if(recipe.Inputs == null) { return; }

		foreach(RecipeItem ingredient in recipe.Inputs) {
			Log.Info($"Ingredient: {ingredient.ItemId} x {ingredient.Quantity}");
			int totalCost = ingredient.Quantity * Quantity;
			RequirementsList.AddItem($"{ingredient.ItemId} x {totalCost}");
		}
	}

	private void UpdateSelectedRequirements() {
		RequirementsList.Clear();

		if(SelectedRecipe != null) {
			UpdateRequirementsList(SelectedRecipe);
		}
	}

	private void OnCraftButtonPressed() {
		if(SelectedRecipe == null) {
			Log.Warn("Craft button pressed but no recipe selected");
			return;
		}

		bool craftedAtLeastOne = false;

		for(int i = 0; i < Quantity; i++) {
			CraftResult result = CraftingSystem.Craft(SelectedRecipe, Inventories);

			if(result.Status == CraftStatus.Success) {
				foreach(ItemSlot slot in result.Items) {
					Inventories[0].AddItem(slot);
				}
				Log.Info($"Crafted '{SelectedRecipe.RecipeName}' x {Quantity}.");
				craftedAtLeastOne = true;
			} else {
				Log.Warn($"Crafting failed: {result.Status}");
				break;
			}
		}

		if(craftedAtLeastOne) { RefreshUI(); }
	}
}
