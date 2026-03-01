namespace Components {
    using System;
    using ItemSystem;
    using Objects;
    using Services;
    using Godot;
    using Core;

    public interface IInventoryComponent { InventoryComponent InventoryComponent { get; set; } }

    public sealed class InventoryComponent : ISaveable<InventoryComponentData>, IObjectComponent {
        public Objects.Object ComponentOwner { get; init; }
        public Inventory Inventory { get; private set; }

        public InventoryComponent(int rows, int columns, Objects.Object owner) {
            Inventory = new Inventory(rows, columns);
            ComponentOwner = owner;
        }

        public InventoryComponentData Export() => new InventoryComponentData {
            InventoryData = Inventory.Export(),
        };

        public void Import(InventoryComponentData data) {
            if(Inventory == null) {
                Inventory = new Inventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
            }
            if(Inventory.MaxRows != data.InventoryData.MaxSlotsRows || Inventory.MaxColumns != data.InventoryData.MaxSlotsColumns) {
                Inventory = new Inventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
            }
            Inventory.Import(data.InventoryData);
        }
    }

    public static class InventoryComponentExtensions {

    }
    
    public readonly record struct InventoryComponentData : ISaveData {
        public InventoryData InventoryData { get; init; }
    }
}