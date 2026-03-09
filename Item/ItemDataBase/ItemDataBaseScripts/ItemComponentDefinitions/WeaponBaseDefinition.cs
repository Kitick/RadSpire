namespace ItemSystem {
    using Godot;

    [GlobalClass]
    public partial class WeaponBaseDefinition : ItemComponentDefinition {
        [Export]
        public int BaseAttack {
            get; set;
        } = 10;

        [Export]
        public float AttackSpeed {
            get; set;
        } = 1f;
    }
}