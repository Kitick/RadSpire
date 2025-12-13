//This file was developed entirely by the RadSpire Development Team.

using System;
using System.Collections.Generic;
using Godot;

public partial interface IInventoryUI {
    Inventory Inventory { get; set; }
    Rect2 GetGlobalRect();
    void SetUpInventoryUI();
    public event Action<string, int>? OnSlotPressed;
    public event Action<string, int>? OnSlotReleased;
    void UpdateInventoryUI();
}