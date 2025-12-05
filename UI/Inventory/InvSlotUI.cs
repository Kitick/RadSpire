using Godot;
using System;

public partial class InvSlotUI : Panel {
	private static readonly Logger Log = new(nameof(InvSlotUI), enabled: true);

	[Export] public int SlotIndex { get; set; } = -1;
	[Export] public TextureRect IconTextureRect { get; set; } = null!;
	[Export] public Label ItemCountLabel { get; set; } = null!;
	public event Action<int>? OnSlotClicked;
	public event Action<int>? OnSlotUnclicked;

	public override void _Ready() {
		base._Ready();
		IconTextureRect = GetNode<TextureRect>("TextureRect");
		ItemCountLabel = GetNode<Label>("ItemCountLabel");
	}

	public void UpdateSlotUI(ItemSlot itemSlot) {
		if(!itemSlot.IsEmpty()) {
			IconTextureRect.Texture = itemSlot.Item.IconTexture;
			IconTextureRect.Visible = true;
			if(itemSlot.Quantity > 1) {
				ItemCountLabel.Text = itemSlot.Quantity.ToString();
				ItemCountLabel.Visible = true;
			}
			else {
				ItemCountLabel.Text = "";
				ItemCountLabel.Visible = false;
			}
		}
		else {
			IconTextureRect.Texture = null;
			IconTextureRect.Visible = false;
			ItemCountLabel.Text = "";
			ItemCountLabel.Visible = false;
		}
	}

	public override void _GuiInput(InputEvent @event) {
		if(@event is InputEventMouseButton mouseEvent) {
			if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed) {
				if(SlotIndex == -1) {
					Log.Error("SlotIndex is -1, cannot handle click.");
					return;
				}
				OnSlotClicked?.Invoke(SlotIndex);
				Log.Info($"Slot {SlotIndex} clicked.");
			}
			if(mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed) {
				Log.Info($"Slot {SlotIndex} mouse left released.");
				OnSlotUnclicked?.Invoke(SlotIndex);
			}
		}
	}

}
