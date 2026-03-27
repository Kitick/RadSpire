namespace InventorySystem.Interface;

using System;
using Godot;
using InventorySystem;
using ItemSystem;
using Services;

public partial class InvSlotUI : Panel {
	private static readonly LogService Log = new(nameof(InvSlotUI), enabled: true);

	[Export] public int SlotIndex { get; set; } = -1;
	[Export] public TextureRect IconTextureRect { get; set; } = null!;
	[Export] public Label ItemCountLabel { get; set; } = null!;
	public event Action<int, MouseButton>? OnSlotPressed;
	public event Action<int, MouseButton>? OnSlotReleased;
	public event Action<int>? OnSlotHovered;

	public override void _Ready() {
		base._Ready();
		IconTextureRect = GetNode<TextureRect>("TextureRect");
		ItemCountLabel = GetNode<Label>("ItemCountLabel");
		MouseEntered += OnMouseEntered;
	}

	public void UpdateSlotUI(ItemSlot itemSlot) {
		if(!itemSlot.IsEmpty()) {
			IconTextureRect.Texture = itemSlot.Item!.IconTexture;
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
			if(SlotIndex == -1) {
				Log.Error("SlotIndex is -1, cannot handle click.");
				return;
			}
			if(mouseEvent.ButtonIndex != MouseButton.Left && mouseEvent.ButtonIndex != MouseButton.Right) {
				return;
			}
			if(mouseEvent.Pressed) {
				OnSlotPressed?.Invoke(SlotIndex, mouseEvent.ButtonIndex);
				Log.Info($"Slot {SlotIndex} pressed.");
			}
			else {
				Log.Info($"Slot {SlotIndex} released.");
				OnSlotReleased?.Invoke(SlotIndex, mouseEvent.ButtonIndex);
			}
		}
	}

	public void OnMouseEntered() {
		if(SlotIndex == -1) {
			Log.Error("SlotIndex is -1, cannot handle hover.");
			return;
		}
		OnSlotHovered?.Invoke(SlotIndex);
		Log.Info($"Slot {SlotIndex} hovered.");
	}
}
