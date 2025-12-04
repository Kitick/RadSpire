using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryManager : Node {
    public Dictionary<string, Inventory> Inventories = new Dictionary<string, Inventory>();
    public InventoryUIManager InventoryUIManager = null!;
    public event Action<string>? InventoryChanged;

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

    public Inventory GetInventory(string name) {
        if(Inventories.ContainsKey(name)) {
            return Inventories[name];
        }
        GD.PrintErr($"[InventoryManager] GetInventory: Inventory with name {name} not found.");
        return null!;
    }
}