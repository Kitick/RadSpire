namespace UI;

using System;
using System.Collections.Generic;
using Godot;
using ItemSystem;
using Root;
using Services;
using Services.Crafting;

public sealed partial class CraftingUI : Control {
	private static readonly LogService Log = new(nameof(CraftingUI), enabled: true);

	[Export] public OptionButton CraftableDropdown = null!;
	[Export] public OptionButton NonCraftableDropdown = null!;
	[Export] public ItemList RequirementsList = null!;
	[Export] public Button CraftButton = null!;
	[Export] public LineEdit QuantityDisplay = null!;
	[Export] public Button PosButton = null!;
	[Export] public Button NegButton = null!;

	private readonly List<CraftingRecipe> CraftableRecipes = [];
	private readonly List<CraftingRecipe> NonCraftableRecipes = [];

	private CraftingRecipe? SelectedRecipe;

	private int Quantity {
		get;
		set {
			field = Math.Clamp(value, 1, 10);
			RefreshQuantity();
		}
	} = 1;

	public readonly List<Inventory> Inventories = [];

	public override void _Ready() {
		ConfigureMouseFilters();
		SetCallbacks();
	}

	private void ConfigureMouseFilters() {
		MouseFilter = MouseFilterEnum.Ignore;

		Control? mainBackground = GetNodeOrNull<Control>("MainBackground");
		if(mainBackground != null) {
			mainBackground.MouseFilter = MouseFilterEnum.Ignore;
		}

		Control? lightBackground = GetNodeOrNull<Control>("MainBackground/LightBackground");
		if(lightBackground != null) {
			lightBackground.MouseFilter = MouseFilterEnum.Ignore;
		}

		Control? tabBackground = GetNodeOrNull<Control>("TabBackground");
		if(tabBackground != null) {
			tabBackground.MouseFilter = MouseFilterEnum.Ignore;
		}
	}

	private void SetCallbacks() {
		CraftButton.Pressed += OnCraftButtonPressed;
		PosButton.Pressed += () => Quantity++;
		NegButton.Pressed += () => Quantity--;

		CraftableDropdown.ItemSelected += OnCraftableSelected;
		NonCraftableDropdown.ItemSelected += OnNonCraftableSelected;
	}

	private void OnCraftableSelected(long _) {
		SelectedRecipe = CraftableDropdown.GetSelectedItem(CraftableRecipes);
		UpdateSelectedRequirements();
	}

	private void OnNonCraftableSelected(long _) {
		SelectedRecipe = NonCraftableDropdown.GetSelectedItem(NonCraftableRecipes);
		UpdateSelectedRequirements();
	}

	public void RefreshUI() {
		CraftableRecipes.Clear();
		NonCraftableRecipes.Clear();

		foreach(var recipe in Recipes.AllRecipes) {
			if(CraftingSystem.CanCraft(recipe, Inventories, out _)) {
				CraftableRecipes.Add(recipe);
			}
			else {
				NonCraftableRecipes.Add(recipe);
			}
		}

		CraftableDropdown.Populate(CraftableRecipes);
		NonCraftableDropdown.Populate(NonCraftableRecipes);

		if(CraftableRecipes.Count > 0) { SelectedRecipe = CraftableRecipes[0]; }
		else if(NonCraftableRecipes.Count > 0) { SelectedRecipe = NonCraftableRecipes[0]; }
		else { SelectedRecipe = null; }

		RefreshQuantity();
	}

	private void RefreshQuantity() {
		QuantityDisplay.Text = Quantity.ToString();
		UpdateSelectedRequirements();
	}

	private void UpdateRequirementsList(CraftingRecipe recipe) {
		RequirementsList.Clear();
		if(recipe.Inputs == null) { return; }

		foreach(var ingredient in recipe.Inputs) {
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
				foreach(var slot in result.Items) {
					Inventories[0].AddItem(slot);
				}
				Log.Info($"Crafted '{SelectedRecipe.RecipeName}' x {Quantity}.");
				craftedAtLeastOne = true;
			}
			else {
				Log.Warn($"Crafting failed: {result.Status}");
				break;
			}
		}

		if(craftedAtLeastOne) { RefreshUI(); }
	}
}
