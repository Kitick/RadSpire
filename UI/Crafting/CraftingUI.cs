namespace UI {
	using System;
	using System.Collections.Generic;
	using Godot;
	using ItemSystem;
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

		private readonly List<CraftingRecipe> craftableRecipes = [];
		private readonly List<CraftingRecipe> nonCraftableRecipes = [];

		private int Quantity {
			get;
			set {
				field = Math.Clamp(value, 1, 10);
				RefreshQuantity();
			}
		} = 1;

		public readonly List<Inventory> Inventories = [];

		public override void _Ready() {
			SetCallbacks();
		}

		private void SetCallbacks() {
			CraftButton.Pressed += OnCraftButtonPressed;
			PosButton.Pressed += () => Quantity++;
			NegButton.Pressed += () => Quantity--;

			CraftableDropdown.ItemSelected += OnCraftableSelected;
			NonCraftableDropdown.ItemSelected += OnNonCraftableSelected;
		}

		private void OnCraftableSelected(long index) {
			if(index >= 0 && index < craftableRecipes.Count) {
				UpdateRequirementsList(craftableRecipes[(int) index]);
			}
		}

		private void OnNonCraftableSelected(long index) {
			if(index >= 0 && index < nonCraftableRecipes.Count) {
				UpdateRequirementsList(nonCraftableRecipes[(int) index]);
			}
		}

		public void RefreshUI() {
			craftableRecipes.Clear();
			nonCraftableRecipes.Clear();

			CraftableDropdown.Clear();
			NonCraftableDropdown.Clear();

			foreach(var recipe in Recipes.AllRecipes) {
				if(CraftingSystem.CanCraft(recipe, Inventories, out _)) {
					craftableRecipes.Add(recipe);
					CraftableDropdown.AddItem(recipe.RecipeName);
				}
				else {
					nonCraftableRecipes.Add(recipe);
					NonCraftableDropdown.AddItem(recipe.RecipeName);
				}
			}

			RefreshQuantity();
			UpdateSelectedRequirements();
		}

		private void RefreshQuantity() {
			QuantityDisplay.Text = Quantity.ToString();
			UpdateSelectedRequirements();
		}

		private CraftingRecipe? GetSelectedRecipe() {
			int index = CraftableDropdown.Selected;

			if(index < 0 || index >= craftableRecipes.Count) { return null; }

			return craftableRecipes[index];
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

			var selectedRecipe = GetSelectedRecipe();

			if(selectedRecipe != null) {
				UpdateRequirementsList(selectedRecipe);
			}
		}

		private void OnCraftButtonPressed() {
			var selectedRecipe = GetSelectedRecipe();

			if(selectedRecipe == null) {
				Log.Warn("Craft button pressed but no recipe selected");
				return;
			}

			for(int i = 0; i < Quantity; i++) {
				CraftResult result = CraftingSystem.Craft(selectedRecipe, Inventories);

				if(result.Status == CraftStatus.Success) {
					foreach(var slot in result.Items) {
						Inventories[0].AddItem(slot);
					}
					Log.Info($"Crafted '{selectedRecipe.RecipeName}' x {Quantity}.");
				}
				else {
					Log.Warn($"Crafting failed: {result.Status}");
					break;
				}
			}

			RefreshUI();
		}
	}
}