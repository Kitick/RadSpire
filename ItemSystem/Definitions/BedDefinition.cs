namespace ItemSystem;

    using Godot;
        
    [GlobalClass]
    public partial class BedDefinition : ItemComponentDefinition {
    [Export]
    public int RestoreAmount { get; set; } = 0;
    [Export]
    public Vector3 Location { get; set; } = Vector3.Zero;
}
