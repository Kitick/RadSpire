using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryManager : Node {
    Dictionary<string, Inventory> Inventories = new Dictionary<string, Inventory>();

    public override void _Ready() {
        base._Ready();
    }

    public void RegisterInventory(Inventory inventory) {
        if(inventory == null) {
            GD.PrintErr("[InventoryManager] RegisterInventory: Inventory is null.");
            return;
        }
        if(Inventories.ContainsKey(inventory.Name)) {
            GD.PrintErr($"[InventoryManager] RegisterInventory: Inventory with name {inventory.Name} already registered.");
            return;
        }
        Inventories.Add(inventory.Name, inventory);
        GD.Print($"[InventoryManager] Registered inventory: {inventory.Name}");
    }

    
}