namespace Crafting.Interface;

using System;
using System.Collections.Generic;
using Crafting;
using Godot;
using InventorySystem;
using ItemSystem;
using Root;
using Services;

public sealed partial class CraftingUI : Control {
	private static readonly LogService Log = new(nameof(CraftingUI), enabled: true);

	[Export] private OptionButton CraftableDropdown = null!;
	[Export] private VBoxContainer RequirementsList = null!;
	[Export] private Button CraftButton = null!;
	[Export] private LineEdit QuantityDisplay = null!;
	[Export] private Button PosButton = null!;
	[Export] private Button NegButton = null!;
	[Export] private Control MainBackground = null!;
	[Export] private Control LightBackground = null!;
	[Export] private Label ItemName = null!;
	[Export] private TextureRect ItemIcon = null!;

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
		SelectedRecipe = CraftableDropdown.GetSelectedItem(Recipes.AllRecipes);
		UpdateSelectedItem();
		UpdateSelectedRequirements();
	}

	public void RefreshUI() {
		CraftableDropdown.Populate(Recipes.AllRecipes);

		SelectedRecipe = Recipes.AllRecipes.Length > 0 ? Recipes.AllRecipes[0] : null;

		UpdateSelectedItem();
		RefreshQuantity();
	}

	private void UpdateSelectedItem() {
		if(SelectedRecipe == null) {
			ItemName.Text = "";
			ItemIcon.Texture = null;
			return;
		}

		RecipeItem output = SelectedRecipe.Outputs[0];
		ItemDefinition? def = DatabaseManager.Instance.GetItemDefinitionById(output.ItemId);
		ItemName.Text = def?.Name ?? output.ItemId;
		ItemIcon.Texture = def?.IconTexture;
	}

	private void RefreshQuantity() {
		QuantityDisplay.Text = Quantity.ToString();
		UpdateSelectedRequirements();
	}

	private static readonly StyleBoxEmpty EmptyStyle = new();

	private void UpdateSelectedRequirements() {
		RequirementsList.AddThemeConstantOverride("separation", 2);

		foreach(Node child in RequirementsList.GetChildren()) {
			child.QueueFree();
		}

		if(SelectedRecipe == null) { return; }

		foreach(RecipeItem ingredient in SelectedRecipe.Inputs) {
			int need = ingredient.Quantity * Quantity;
			int have = CraftingSystem.CountAvailable(ingredient.ItemId, Inventories);
			bool sufficient = have >= need;
			Color color = sufficient ? Colors.White : Colors.Red;

			ItemDefinition? def = DatabaseManager.Instance.GetItemDefinitionById(ingredient.ItemId);
			string name = def?.Name ?? ingredient.ItemId;
			Texture2D? icon = def?.IconTexture;

			HBoxContainer row = new();
			row.AddThemeConstantOverride("separation", 10);

			Label needLabel = new() { Text = $"{need}x" };
			needLabel.AddThemeStyleboxOverride("normal", EmptyStyle);
			needLabel.AddThemeColorOverride("font_color", Colors.White);

			TextureRect iconRect = new() {
				Texture = icon,
				CustomMinimumSize = new Vector2(24, 24),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			};

			Label nameLabel = new() { Text = name };
			nameLabel.AddThemeStyleboxOverride("normal", EmptyStyle);
			nameLabel.AddThemeColorOverride("font_color", Colors.White);

			Label haveLabel = new() { Text = $"({have})", HorizontalAlignment = HorizontalAlignment.Center };
			haveLabel.AddThemeStyleboxOverride("normal", EmptyStyle);
			haveLabel.AddThemeColorOverride("font_color", color);

			row.AddChild(needLabel);
			row.AddChild(iconRect);
			row.AddChild(nameLabel);
			row.AddChild(haveLabel);
			RequirementsList.AddChild(row);
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
