using Godot;
using System;
using System.Collections.Generic;
using Services.Crafting;
using ItemSystem;
using Character;


namespace UI {
    public partial class CraftingUI : Control {
        [Export] public OptionButton CraftableDropdown = null!;
        [Export] public OptionButton NonCraftableDropdown = null!;
        [Export] public ItemList RequirementsList = null!;
        [Export] public Button CraftButton = null!;
        [Export] public LineEdit QuantityDisplay = null!;
        [Export] public Button PosButton = null!;
        [Export] public Button NegButton = null!;


        private List<CraftingRecipe> craftableRecipes = new();
        private List<CraftingRecipe> nonCraftableRecipes = new();
        private int quantity = 1;


        public List<Inventory> PlayerInventory { get; set; } = new();


        public override void _Ready() {
            SetCallbacks();
        }


        private void SetCallbacks() {
            CraftButton.Pressed += OnCraftButtonPressed;
            PosButton.Pressed += OnPosButtonPressed;
            NegButton.Pressed += OnNegButtonPressed;


            CraftableDropdown.ItemSelected += OnCraftableSelected;
            NonCraftableDropdown.ItemSelected += OnNonCraftableSelected;
        }


        private void OnCraftableSelected(long index) {
            if (index >= 0 && index < craftableRecipes.Count) {
                UpdateRequirementsList(craftableRecipes[(int)index]);
            }
        }


        private void OnNonCraftableSelected(long index) {
            if (index >= 0 && index < nonCraftableRecipes.Count) {
                UpdateRequirementsList(nonCraftableRecipes[(int)index]);
            }
        }


        public void RefreshUI() {
            craftableRecipes.Clear();
            nonCraftableRecipes.Clear();


            CraftableDropdown.Clear();
            NonCraftableDropdown.Clear();


            foreach(var recipe in Recipes.AllRecipes) {
                if(CraftingSystem.CanCraft(recipe, PlayerInventory, out _)) {
                    craftableRecipes.Add(recipe);
                    CraftableDropdown.AddItem(recipe.RecipeName);
                }
                else {
                    nonCraftableRecipes.Add(recipe);
                    NonCraftableDropdown.AddItem(recipe.RecipeName);
                }
            }


            QuantityDisplay.Text = quantity.ToString();
            UpdateCurrentSelectedRequirements();
        }


        private void UpdateRequirementsList(CraftingRecipe recipe) {
            RequirementsList.Clear();
            if(recipe.Inputs == null) return;


            foreach(var ingredient in recipe.Inputs) {
                int totalCost = ingredient.Quantity * quantity;
                RequirementsList.AddItem($"{ingredient.ItemId} x {totalCost}");
            }
        }

        private void UpdateCurrentSelectedRequirements() {
            int index = CraftableDropdown.Selected;
           
            if (index >= 0 && index < craftableRecipes.Count) {
                UpdateRequirementsList(craftableRecipes[index]);
            }
            else {
                RequirementsList.Clear();
            }
        }


        private void OnCraftButtonPressed() {
            if(CraftableDropdown.Selected < 0) return;

            var selectedRecipe = craftableRecipes[CraftableDropdown.Selected];

            for(int i = 0; i < quantity; i++) {
                CraftResult result = CraftingSystem.Craft(selectedRecipe, PlayerInventory);
                if(result.Status != CraftStatus.Success) {
                    foreach (var slot in result.Items) {
                        PlayerInventory[0].AddItem(slot);
                    }
                    GD.Print($"Crafted '{selectedRecipe.RecipeName}' x {quantity}.");
                }
                else {
                    GD.PrintErr($"Crafting failed: {result.Status}");
                    break;
                }
            }
            RefreshUI();
        }


        public void OnPosButtonPressed() {
            quantity++;
            RefreshUI();
        }


        public void OnNegButtonPressed() {
            if(quantity > 1) {
                quantity--;
                RefreshUI();
            }
        }
    }
}