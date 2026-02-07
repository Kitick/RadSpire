namespace ItemSystem {
    using System;
    using System.Collections.Generic;
    using Components;
    using Godot;
    using Services;

    //static definition of an item
    [GlobalClass]
    public partial class ItemDefinition : Resource {
        [Export]
        public string Id {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        } = "ItemDefault";

        [Export]
        public string Name {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        } = "Default Item";

        [Export]
        public string Description {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        } = "Default Description";

        [Export]
        public int MaxStackSize {
            get; set {
                if(value < 1) {
                    field = 1;
                    return;
                }
                field = value;
            }
        } = 1;

        public bool IsStackable => MaxStackSize > 1;

        [Export] public bool IsConsumable { get; set; } = true;

        [Export] public Texture2D IconTexture { get; set; } = null!;

        //Components
        [Export] public Godot.Collections.Array<ItemComponentDefinition> ComponentsResources { get; set; } = new();
    }
}