namespace ItemSystem {
    using Godot;

    [GlobalClass]
    public partial class InventoryDefinition : ItemComponentDefinition {
        [Export]
        public int Rows {
            get; set;
        } = 4;

        [Export]
        public int Columns {
            get; set;
        } = 8;
    }
}