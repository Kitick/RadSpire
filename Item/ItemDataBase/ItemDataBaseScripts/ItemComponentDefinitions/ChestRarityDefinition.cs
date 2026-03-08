namespace ItemSystem {
    using Godot;

    public enum Rarity {
        Common,
        Rare,
        Precious,
    }
        
    [GlobalClass]
    public partial class ChestRarityDefinition : ItemComponentDefinition {
        [Export]
        public Rarity RarityLevel {
            get; set;
        } = Rarity.Common;
    }
}