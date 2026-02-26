namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;

	public partial class ChestNode : ObjectNode {
        private static readonly LogService Log = new(nameof(ChestNode), enabled: true);
        public Chest ChestData { get; private set; } = null!;

        public override void Bind(Object obj) {
            base.Bind(obj);
            if(obj is Chest chest) {
                ChestData = chest;
                return;
            }
            Log.Error($"Not a chest");
        }

        public override void _ExitTree() {
            base._ExitTree();
        }
    }

    public partial class Chest : Object, IInventory, ISaveable<ChestData> {
        private const int DefaultRows = 4;
        private const int DefaultColumns = 8;

        public Inventory Inventory { get; set; } = null!;

        public Chest() {
            Inventory = CreateInventory(DefaultRows, DefaultColumns);
        }

        public Chest(string itemId, Vector3 pos, Vector3 rot, int rows = DefaultRows, int columns = DefaultColumns) : base(itemId, pos, rot) {
            Inventory = CreateInventory(rows, columns);
        }

        private Inventory CreateInventory(int rows, int columns) {
            Inventory chestInventory = new Inventory(rows, columns);
            chestInventory.Name = Id;
            return chestInventory;
        }

        public new ChestData Export() => new ChestData {
            ObjectData = base.Export(),
            InventoryData = Inventory.Export()
        };

        public void Import(ChestData data) {
            base.Import(data.ObjectData);
            if(Inventory == null) {
                Inventory = CreateInventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
            }
            if(Inventory.MaxRows != data.InventoryData.MaxSlotsRows || Inventory.MaxColumns != data.InventoryData.MaxSlotsColumns) {
                Inventory = CreateInventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
            }
            Inventory.Import(data.InventoryData);
        }
    }
    
    public readonly record struct ChestData: ISaveData {
        public ObjectData ObjectData { get; init; }
        public InventoryData InventoryData { get; init; }
    }
}