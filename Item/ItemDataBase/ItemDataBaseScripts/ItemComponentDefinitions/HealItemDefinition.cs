namespace ItemSystem {
    using Godot;

    [GlobalClass]
    public partial class HealItemDefinition : ItemComponentDefinition {
        [Export]
        public int HealAmount {
            get; set;
        } = 10;
    }
}