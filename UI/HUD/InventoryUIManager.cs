using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUIManager : Node {
    public InventoryManager InventoryManager = null!;
    public PackedScene? InvSlotTemplate = null!;
    public bool HeldItemSlotExist => HeldItemSlotUI != null;
    public InvSlotUI? HeldItemSlotUI = null;

    public InventoryUIManager(InventoryManager inventoryManager) {
        InventoryManager = inventoryManager;
    }

    public override void _Ready() {
        base._Ready();
        LoadTemplate();
        InventoryManager.StartMoveItemEvent += CreateHeldItemSlotUI;
        InventoryManager.EndMoveItemEvent += DestroyHeldItemSlotUI;
    }

    public override void _Process(double delta) {
        base._Process(delta);
        if(HeldItemSlotExist) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            HeldItemSlotUI!.Position = mousePosition;
        }
    }

    public void LoadTemplate() {
        if(InvSlotTemplate == null) {
            InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
        }   
    }

    public void CreateHeldItemSlotUI(ItemSlot itemSlot) {
        ItemSlot itemSlotCopy = itemSlot.Copy();
        LoadTemplate();
        DestroyHeldItemSlotUI();

        HeldItemSlotUI = InvSlotTemplate.Instantiate<InvSlotUI>();
        AddChild(HeldItemSlotUI);

        HeldItemSlotUI.MouseFilter = Control.MouseFilterEnum.Ignore;
        foreach(var child in HeldItemSlotUI.GetChildren()) {
            if(child is Control ctrl)
                ctrl.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        HeldItemSlotUI.ZIndex = 100;
        HeldItemSlotUI.Visible = true;
        var style = new StyleBoxFlat();
        style.BgColor = new Color(1, 1, 1, 0);
        HeldItemSlotUI.AddThemeStyleboxOverride("panel", style);
        HeldItemSlotUI.UpdateSlotUI(itemSlotCopy);
    }

    public void DestroyHeldItemSlotUI() {
        if(HeldItemSlotExist) {
            HeldItemSlotUI!.QueueFree();
            HeldItemSlotUI = null;
        }
    }
}