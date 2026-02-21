namespace UI {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
    using System.Collections.Generic;

    public partial class InventoryItemInformationUI : Control {
        private static readonly LogService Log = new(nameof(InventoryItemInformationUI), enabled: true);
        public PackedScene? InventoryItemInformationUITemplate = null!;
        public PackedScene? AttributeLabelTemplate = null!;

        public InventoryManager InventoryManager = null!;

        public Item CurrentItem { get; set; } = null!;
        [Export] private Label ItemNameLabel = null!;
        [Export] private Label ItemDescriptionLabel = null!;
        [Export] private TextureRect ItemIconTextureRect = null!;
        [Export] private VBoxContainer ComponentsContainer = null!;
        private List<Control> ComponentsLabels = new List<Control>();

        public override void _Ready() {
            base._Ready();
            SetUpInventoryItemInformationUI();
        }

        public void SetUpInventoryItemInformationUI() {

        }
        
        public void UpdateInventoryItemInformationUI(Item item) {
            CurrentItem = item;
            ItemNameLabel.Text = item.Name;
            ItemDescriptionLabel.Text = item.Description;
            ItemIconTextureRect.Texture = item.IconTexture;

            foreach(Control label in ComponentsLabels) {
                label.QueueFree();
            }
            ComponentsLabels.Clear();

            foreach(IItemComponent component in item.Components) {
                Control label = ComponentsContainer.GetNode<Control>("ComponentLabelTemplate");

                ComponentsContainer.AddChild(label);
                ComponentsLabels.Add(label);
            }
        }
    }
}