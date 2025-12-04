using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUIManager : Control {
    public InventoryManager InventoryManager = null!;
    public PackedScene? InvSlotTemplate = null!;
    public bool HeldItemSlotExist = false;
    public InvSlotUI? HeldItemSlotUI = null;

    public override void _Ready() {
        base._Ready();
        SetProcess(true);
        InventoryManager = GetParent<InventoryUI>().GetParent<HUD>().Player.InventoryManager;
        GD.Print("InventoryUIManager ready; processing enabled.");
        LoadTemplate();
        InventoryManager.StartMoveItemEvent += CreateHeldItemSlotUI;
        InventoryManager.EndMoveItemEvent += DestroyHeldItemSlotUI;
    }

    public override void _Process(double delta) {
        base._Process(delta);
        if(HeldItemSlotExist) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            Vector2 half = new Vector2(16, 16);
            HeldItemSlotUI!.GlobalPosition = mousePosition - half;
        }
    }

    public void LoadTemplate() {
        if(InvSlotTemplate == null) {
            InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
        }   
    }

    public void CreateHeldItemSlotUI(ItemSlot itemSlot) {
        GD.Print("CreateHeldItemSlotUI called");
        ItemSlot itemSlotCopy = itemSlot.Copy();
        LoadTemplate();
        DestroyHeldItemSlotUI();

        HeldItemSlotUI = InvSlotTemplate.Instantiate<InvSlotUI>();
        AddChild(HeldItemSlotUI);
        GD.Print("HeldItemSlotUI created; inside tree=" + HeldItemSlotUI.IsInsideTree() + ", parent=" + (HeldItemSlotUI.GetParent()?.Name ?? "<null>"));

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
        HeldItemSlotExist = true;
    }

    public void DestroyHeldItemSlotUI() {
        if(HeldItemSlotExist) {
            GD.Print("DestroyHeldItemSlotUI called");
            HeldItemSlotUI!.QueueFree();
            HeldItemSlotUI = null;
            HeldItemSlotExist = false;
        }
    }
}