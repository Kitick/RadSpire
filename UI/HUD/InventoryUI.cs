using System;
using Godot; 

public partial class InventoryUI: Control {
    private Player player = null!;
    private Inventory PlayerInventory = null!;

    public override void _Ready() {
        player = GetParent<HUD>().Player;
        PlayerInventory = player.PlayerInventory;
        PlayerInventory.OnInventoryChanged += updateInventoryUI;
        updateInventoryUI();
    }

    public void updateInventoryUI(){
        PlayerInventory = player.PlayerInventory;
        for(int i = 0; i < PlayerInventory.MaxSlotsRows - 1; i++){
            for(int j = 0; j < PlayerInventory.MaxSlotsColumns; j++){
                ItemSlot itemSlot = new ItemSlot();
                itemSlot = PlayerInventory.GetItemSlot(i, j);
                var slotUI = GetNode<Control>($"GridContainer/InvSlot{(i * PlayerInventory.MaxSlotsColumns) + j + 1}");
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
}