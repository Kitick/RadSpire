namespace UI {
    using System;
    using Godot;
    using Services;
    using ItemSystem;
    using System.Collections.Generic;

    public partial class InventoryItemInformationUI : Control {
        private static readonly LogService Log = new(nameof(InventoryItemInformationUI), enabled: true);
        public PackedScene? ComponentLabelTemplate = null!;

        public InventoryManager InventoryManager = null!;

        public Item CurrentItem { get; set; } = null!;
        [Export] private Label ItemNameLabel = null!;
        [Export] private Label ItemDescriptionLabel = null!;
        [Export] private TextureRect ItemIconTextureRect = null!;
        [Export] private Label ItemCountLabel = null!;
        [Export] private VBoxContainer ComponentsContainer = null!;
        private List<Control> ComponentsLabels = new List<Control>();

        public override void _Ready() {
            base._Ready();
            SetUpInventoryItemInformationUI();
        }

        public void SetUpInventoryItemInformationUI() {
            InventoryManager.ItemSlotHovered += UpdateInventoryItemInformationUI;
        }
        
        public void UpdateInventoryItemInformationUI(ItemSlot itemSlot) {
            if(itemSlot.IsEmpty()) {
                return;
            }
            CurrentItem = itemSlot.Item!;
            ItemNameLabel.Text = CurrentItem.Name;
            ItemDescriptionLabel.Text = CurrentItem.Description;
            ItemIconTextureRect.Texture = CurrentItem.IconTexture;
            ItemCountLabel.Text = itemSlot.Quantity.ToString();

            foreach(Control label in ComponentsLabels) {
                label.QueueFree();
            }
            ComponentsLabels.Clear();
            ComponentsContainer.QueueFree();

            foreach(IItemComponent component in CurrentItem.Components) {
                if(ComponentLabelTemplate == null) {
                    Log.Error("ComponentLabelTemplate is null.");
                    return;
                }
                string[] componentDescriptions = component.getComponentDescription();
                if(componentDescriptions == null || componentDescriptions.Length == 0) {
                    continue;
                }
                foreach(string componentDescription in componentDescriptions) {
                    Control componentlabel = ComponentLabelTemplate.Instantiate<Control>();
                    componentlabel.GetNode<Label>("Label").Text = componentDescription;
                    ComponentsContainer.AddChild(componentlabel);
                    ComponentsLabels.Add(componentlabel);
                }
            }
        }
    }
}