using System;
using Components;
using Core;
using SaveSystem;
using Godot;

public partial class Item3DIcon : Node3D {
    [Export] public Item? Item { get; set; }
    [Export] public PackedScene? Item3DSceneTemplate { get; set; }
    private Node3D? CurrentItem3DScene;

    public override void _Ready() {
        if (Item3DSceneTemplate == null) {
            Item3DSceneTemplate = GD.Load<PackedScene>("res://Item/ItemIcon3D/ItemIcon3DTemplete.tscn");
        }
        if (Item3DSceneTemplate != null) {
            CurrentItem3DScene = Item3DSceneTemplate.Instantiate<Node3D>();
            AddChild(CurrentItem3DScene);
        } else {
            GD.PrintErr("Item3DSceneTemplate not set and fallback load failed.");
        }
    }

    public void SpawnItem3D(Vector3 position) {
        if (Item == null) {
            GD.PrintErr("Item3DIcon.SpawnItem3D called but Item is null.");
            return;
        }

        if (Item.IconTexture == null) {
            GD.Print("Item has no IconTexture; skipping 3D icon spawn.");
            return;
        }

        if (Item3DSceneTemplate == null) {
            GD.PrintErr("Item3DSceneTemplate is null; cannot spawn item 3D.");
            return;
        }

        if (CurrentItem3DScene != null) {
            CurrentItem3DScene.QueueFree();
            CurrentItem3DScene = null;
        }

        CurrentItem3DScene = Item3DSceneTemplate.Instantiate<Node3D>();
        AddChild(CurrentItem3DScene);

        CurrentItem3DScene.GlobalPosition = position;

        var spriteMeshNode = CurrentItem3DScene.GetNodeOrNull<Node3D>("SpriteMeshInstance");
        if (spriteMeshNode != null) {
            spriteMeshNode.Set("texture", Item.IconTexture);
        } else {
            GD.PrintErr("Spawned scene missing 'SpriteMeshInstance' node.");
        }
    }
}

