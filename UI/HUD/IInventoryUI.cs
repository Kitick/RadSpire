using System;
using System.Collections.Generic;
using Godot;

public partial interface IInventoryUI {
    Inventory Inventory { get; set; }
    Rect2 GetGlobalRect();
    void SetUpInventoryUI();
    public event Action<string, int>? OnSlotClicked;
    public event Action<string, int>? OnSlotUnclicked;
    void UpdateInventoryUI();
}