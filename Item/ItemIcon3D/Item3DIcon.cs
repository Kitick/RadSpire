using System;
using Components;
using Core;
using SaveSystem;
using Godot;

public partial class Item3DIcon : Node3D {

    private static readonly Logger Log = new(nameof(Item3DIcon), enabled: false);

    [Export] public Item? Item { get; set; }
    [Export] public PackedScene? Item3DSceneTemplate { get; set; }
    private Node3D? CurrentItem3DScene;

    public override void _Ready() {
        if(Item3DSceneTemplate == null) {
            Item3DSceneTemplate = GD.Load<PackedScene>("res://Item/ItemIcon3D/ItemIcon3DTemplete.tscn");
        }
        if(Item3DSceneTemplate == null) {
            Log.Error("Item3DIcon _Ready: Failed to load fallback Item3DSceneTemplate.");
        }
    }

    public void SpawnItem3D(Vector3 position)
    {
        if (Item == null)
        {
            Log.Error("Item3DIcon.SpawnItem3D called but Item is null.");
            return;
        }

        if (Item.IconTexture == null)
        {
            Log.Info("Item has no IconTexture; skipping 3D icon spawn.");
            return;
        }

        // Clear old mesh
        if (CurrentItem3DScene != null)
        {
            CurrentItem3DScene.QueueFree();
            CurrentItem3DScene = null;
        }

        // Load template if needed
        if (Item3DSceneTemplate == null)
            Item3DSceneTemplate = GD.Load<PackedScene>("res://Item/ItemIcon3D/ItemIcon3DTemplete.tscn");

        CurrentItem3DScene = Item3DSceneTemplate.Instantiate<Node3D>();
        AddChild(CurrentItem3DScene);
        CurrentItem3DScene.GlobalPosition = position;

        // ---- SpriteMeshInstance is a MeshInstance3D ----
        var spriteMesh = CurrentItem3DScene.GetNodeOrNull<Node3D>("RigidBody3D/SpriteMeshInstance");

        if (spriteMesh == null)
        {
            Log.Error("Spawned scene missing 'SpriteMeshInstance' node.");
            return;
        }

        // Set texture
        spriteMesh.Set("texture", Item.IconTexture);

        // Generate mesh + material
        spriteMesh.Call("update_sprite_mesh");

        Log.Info("Item 3D mesh generated successfully.");

        // --- FIND COLLISIONSHAPE3D ---
        CollisionShape3D collisionShape = CurrentItem3DScene.GetNodeOrNull<CollisionShape3D>("RigidBody3D/CollisionShape3D");
        if (collisionShape == null)
        {
            Log.Error("CollisionShape3D not found in template.");
            return;
        }
        BoxShape3D boxShape = new BoxShape3D();
        boxShape.Size = new Vector3(0.5f, 0.5f, 0.1f);
        collisionShape.Shape = boxShape;
        collisionShape.Position = Vector3.Zero;
        RigidBody3D rigidBody = CurrentItem3DScene.GetNodeOrNull<RigidBody3D>("RigidBody3D");
        rigidBody.Mass = 0.5f;
        rigidBody.Position = Vector3.Zero;
        Log.Info("Collision shape updated to match generated mesh.");
    }

}