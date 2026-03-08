namespace ItemSystem {
    using Godot;

    [GlobalClass]
    public partial class ChestRarityDefinition : ItemComponentDefinition {
        public enum Rarity {
            Common,
            Rare,
            Precious,
        }

        [Export]
        public Rarity RarityLevel {
            get; set;
        } = Rarity.Common;
    }
}