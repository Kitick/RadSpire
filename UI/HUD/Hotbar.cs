using System;
using System.Collections.Generic;
using Godot;
using InputSystem;

public sealed partial class Hotbar : Control {
	public static readonly bool Debug = false;

	public int SelectedSlot {
		get;
		set {
			value += HotbarSlots.Count;
			value %= HotbarSlots.Count;
			field = value;
			SelectSlot(HotbarSlots[value]);
		}
	}

	private readonly List<Panel> HotbarSlots = [];

	private static readonly Color NormalColor = Colors.White;
	private static readonly Color SelectColor = new Color(1f, 1f, 0.3f);
	private static readonly Vector2 SelectedScale = new Vector2(1.05f, 1.05f);

	private const string HOTBAR = "Background/GridBackground/HotbarSlots";

	private event Action? OnExit;

    private Player player = null!;
	private Inventory PlayerInventory = null!;

	public override void _EnterTree() {
		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
		GetComponents();
		SelectedSlot = 0;
        player = GetParent<HUD>().Player;
        PlayerInventory = player.PlayerInventory;
		PlayerInventory.OnInventoryChanged += updateHotBarUI;
		updateHotBarUI();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	private void GetComponents() {
		var container = GetNode<GridContainer>(HOTBAR);

		foreach(var child in container.GetChildren()) {
			if(child is Panel slot) {
				HotbarSlots.Add(slot);
			}
		}
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.Hotbar1.WhenPressed(() => SelectedSlot = 0);
		OnExit += ActionEvent.Hotbar2.WhenPressed(() => SelectedSlot = 1);
		OnExit += ActionEvent.Hotbar3.WhenPressed(() => SelectedSlot = 2);
		OnExit += ActionEvent.Hotbar4.WhenPressed(() => SelectedSlot = 3);
		OnExit += ActionEvent.Hotbar5.WhenPressed(() => SelectedSlot = 4);

		OnExit += ActionEvent.HotbarNext.WhenPressed(() => SelectedSlot++);
		OnExit += ActionEvent.HotbarPrev.WhenPressed(() => SelectedSlot--);
	}

	private void SelectSlot(Panel slot) {
		if(Debug) {
			GD.Print($"Hotbar: Selecting slot {HotbarSlots.IndexOf(slot)}"); 
			GD.Print($"Hotbar: Selected item: {GetSelectedItem().Name}");
		}

		foreach(var other in HotbarSlots) {
			bool selected = other == slot;

			other.SelfModulate = selected ? SelectColor : NormalColor;
			other.Scale = selected ? SelectedScale : Vector2.One;
		}
	}

	public ItemSlot GetSelectedItemSlot() {
		int index = SelectedSlot;
		return PlayerInventory.GetItemSlot(PlayerInventory.MaxSlotsRows - 1, index);
	}

	public Item GetSelectedItem() {
		int index = SelectedSlot;
		if(PlayerInventory.ItemSlots[index].IsEmpty()) {
			return null;
		}
		return PlayerInventory.GetItem(PlayerInventory.MaxSlotsRows - 1, index);
	}

    public void updateHotBarUI(){
		PlayerInventory = player.PlayerInventory;
        for(int i = 0; i < PlayerInventory.MaxSlotsColumns - 1; i++){
			ItemSlot itemSlot = new ItemSlot();
			itemSlot = PlayerInventory.GetItemSlot(PlayerInventory.MaxSlotsRows - 1, i);
			var slotUI = GetNode<Control>($"HotbarSlots/Slot{i + 1}");
			var icon = slotUI.GetNode<TextureRect>("TextureRect");
			var quantityLabel = slotUI.GetNode<Label>("ItemCountLabel");
			if(!itemSlot.IsEmpty()){
				icon.Texture = itemSlot.Item.IconTexture;
				icon.Visible = true;
				if(itemSlot.Quantity > 1){
					quantityLabel.Text = itemSlot.Quantity.ToString();
					quantityLabel.Visible = true;
				}
				else{
					quantityLabel.Text = "";
					quantityLabel.Visible = false;
				}
			}
			else{
				icon.Texture = null;
				icon.Visible = false;
				quantityLabel.Text = "";
				quantityLabel.Visible = false;
			}
        }
    }
}
