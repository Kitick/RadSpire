using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryManager : Node {
    public Dictionary<string, (Inventory, Control)> Inventories = new Dictionary<string, (Inventory, Control)>();
    public InventoryUIManager InventoryUIManager = null!;
    public event Action<ItemSlot>? StartMoveItemEvent;
    public event Action? EndMoveItemEvent;

    public override void _Ready() {
        base._Ready();
    }

    public void RegisterInventory(Inventory inventory, Control uiControl) {
        if(inventory == null) {
            GD.PrintErr("[InventoryManager] RegisterInventory: Inventory is null.");
            return;
        }
        if(Inventories.ContainsKey(inventory.Name)) {
            GD.PrintErr($"[InventoryManager] RegisterInventory: Inventory with name {inventory.Name} already registered.");
            return;
        }
        Inventories.Add(inventory.Name, (inventory, uiControl));

        GD.Print($"[InventoryManager] Registered inventory: {inventory.Name}");
    }

    public void UnregisterInventory(string name) {
        if(!Inventories.ContainsKey(name)) {
            GD.PrintErr($"[InventoryManager] UnregisterInventory: Inventory with name {name} not found.");
            return;
        }
        Inventories.Remove(name);
        GD.Print($"[InventoryManager] Unregistered inventory: {name}");
    }

    public Inventory GetInventory(string name) {
        if(Inventories.ContainsKey(name)) {
            return Inventories[name].Item1;
        }
        GD.PrintErr($"[InventoryManager] GetInventory: Inventory with name {name} not found.");
        return null!;
    }
}