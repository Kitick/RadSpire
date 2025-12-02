using System;
using Godot; 

public partial class InventoryUI: Control {
	private Player player = null!;
	private Inventory PlayerInventory = null!;

	public override void _Ready() {
		base._Ready();
		if((player = GetParent<HUD>().Player) == null) {
			GD.PrintErr("InventoryUI could not find Player node in parent HUD.");
			return;
		}
		GD.Print("InventoryUI successfully found Player node in parent HUD.");
		PlayerInventory = player.PlayerInventory;
		PlayerInventory.OnInventoryChanged += updateInventoryUI;
		updateInventoryUI();
	}

	public void updateInventoryUI(){
		player = GetParent<HUD>().Player;
		PlayerInventory = player.PlayerInventory;
		for(int i = 0; i < PlayerInventory.MaxSlotsRows - 1; i++){
			for(int j = 0; j < PlayerInventory.MaxSlotsColumns; j++){
				ItemSlot itemSlot = new ItemSlot();
				itemSlot = PlayerInventory.GetItemSlot(i, j);
				var slotUI = GetNode<Control>($"Background/GridBackground/GridContainer/InvSlot{(i * PlayerInventory.MaxSlotsColumns) + j + 1}");
				if(slotUI != null) {
					GD.Print($"Updating UI for slot {(i * PlayerInventory.MaxSlotsColumns) + j + 1} at row {i}, column {j}.");
				}
				var icon = slotUI.GetNode<TextureRect>("TextureRect");
				if(icon == null) {
					GD.PrintErr($"Icon TextureRect not found in slot UI for slot {(i * PlayerInventory.MaxSlotsColumns) + j + 1}.");
				}
				var quantityLabel = slotUI.GetNode<Label>("ItemCountLabel");
				if(quantityLabel == null) {
					GD.PrintErr($"Quantity Label not found in slot UI for slot {(i * PlayerInventory.MaxSlotsColumns) + j + 1}.");
				}
				GD.Print($"Checking slot {i},{j}: Item = {itemSlot.Item}, Quantity = {itemSlot.Quantity}");
				if(!itemSlot.IsEmpty()){
					GD.Print($"Slot {(i * PlayerInventory.MaxSlotsColumns) + j + 1} contains {itemSlot.Quantity} x {itemSlot.Item.Name}.");
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
}
