namespace ItemSystem {
    using Godot;

    [GlobalClass]
    public partial class DurabilityDefinition : ItemComponentDefinition {
        [Export]
        public int MaxDurability {
            get; set;
        } = 100;
    }
}