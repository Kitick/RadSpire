using System;
using System.Collections.Generic;
using Godot;

public partial interface IInventoryUI {
    void SetUpInventoryUI();
    public event Action<int>? OnSlotClicked;
    void UpdateInventoryUI();
}