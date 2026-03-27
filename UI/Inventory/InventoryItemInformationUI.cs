namespace UI;

using System.Collections.Generic;
using Godot;
using ItemSystem;
using Services;

public partial class InventoryItemInformationUI : Control {
	private static readonly LogService Log = new(nameof(InventoryItemInformationUI), enabled: true);

	public PackedScene? ComponentLabelTemplate = null;
	public InventoryManager InventoryManager = null!;

	public Item CurrentItem { get; set; } = null!;

	[Export] private Label ItemNameLabel = null!;
	[Export] private Label ItemDescriptionLabel = null!;
	[Export] private TextureRect ItemIconTextureRect = null!;
	[Export] private Label ItemCountLabel = null!;
	[Export] private VBoxContainer ComponentsContainer = null!;

	private readonly List<Control> ComponentsLabels = [];

	public override void _Ready() {
		base._Ready();
	}

	public void SetUpInventoryItemInformationUI() {
		InventoryManager.ItemSlotHovered += UpdateInventoryItemInformationUI;
		ItemNameLabel = GetNode<Label>("ColorRect/ColorRect2/VBoxContainer/ItemNameLabel");
		ItemDescriptionLabel = GetNode<Label>("ColorRect/ColorRect2/VBoxContainer/ItemDescriptionLabel");
		ItemIconTextureRect = GetNode<TextureRect>("ColorRect/ColorRect2/VBoxContainer/GridContainer/InvSlot1/ItemIconTextureRect");
		ItemCountLabel = GetNode<Label>("ColorRect/ColorRect2/VBoxContainer/GridContainer/InvSlot1/ItemCountLabel");
		ComponentsContainer = GetNode<VBoxContainer>("ColorRect/ColorRect2/VBoxContainer/ComponentsContainer");
		ComponentLabelTemplate = GD.Load<PackedScene>("res://UI/Inventory/ComponentLabelTemplate.tscn");
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

		foreach(Node child in ComponentsContainer.GetChildren()) {
			child.QueueFree();
		}
		ComponentsLabels.Clear();

		foreach(IItemComponent component in CurrentItem.GetComponentsOrdered()) {
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
				componentlabel.GetNode<Label>("Panel/Label").Text = componentDescription;
				ComponentsContainer.AddChild(componentlabel);
				ComponentsLabels.Add(componentlabel);
			}
		}
	}
}

