using System;
using Components;
using Core;
using Godot;
using SaveSystem;

public partial class Item3DIcon : Node3D {
	private static readonly Logger Log = new(nameof(Item3DIcon), enabled: true);

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

	public void SpawnItem3D(Vector3 position) {
		if(Item == null) {
			Log.Error("Item3DIcon.SpawnItem3D called but Item is null.");
			return;
		}

		if(Item.IconTexture == null) {
			Log.Info("Item has no IconTexture; skipping 3D icon spawn.");
			return;
		}

		// Clear old mesh
		if(CurrentItem3DScene != null) {
			CurrentItem3DScene.QueueFree();
			CurrentItem3DScene = null;
		}

		// Load template if needed
		if(Item3DSceneTemplate == null)
			Item3DSceneTemplate = GD.Load<PackedScene>("res://Item/ItemIcon3D/ItemIcon3DTemplete.tscn");

		CurrentItem3DScene = Item3DSceneTemplate.Instantiate<Node3D>();
		AddChild(CurrentItem3DScene);
		CurrentItem3DScene.GlobalPosition = position;

		// ---- SpriteMeshInstance is a MeshInstance3D ----
		var spriteMesh = CurrentItem3DScene.GetNodeOrNull<Node3D>("RigidBody3D/SpriteMeshInstance");

		if(spriteMesh == null) {
			Log.Error("Spawned scene missing 'SpriteMeshInstance' node.");
			return;
		}

		// Set texture
		spriteMesh.Set("texture", Item.IconTexture);

		// Generate mesh + material
		spriteMesh.Call("update_sprite_mesh");

		float targetSize = 0.7f;

		MeshInstance3D meshInstance = spriteMesh as MeshInstance3D;
		if (meshInstance != null && meshInstance.Mesh != null) {
			Aabb bounds = meshInstance.Mesh.GetAabb();
			Vector3 size = bounds.Size;

			float largestAxis = Mathf.Max(size.X, Mathf.Max(size.Y, size.Z));

			if (largestAxis > 0.001f) {
				float scaleFactor = targetSize / largestAxis;

				meshInstance.Scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
			}
		}


		Log.Info("Item 3D mesh generated successfully.");

		// --- Collision ---
		CollisionShape3D collisionShape = CurrentItem3DScene.GetNodeOrNull<CollisionShape3D>("RigidBody3D/CollisionShape3D");
		if (collisionShape == null) {
			Log.Error("CollisionShape3D not found in template.");
			return;
		}

		BoxShape3D boxShape = new BoxShape3D();

		Aabb rawBounds = meshInstance.Mesh.GetAabb();
		Vector3 rawSize = rawBounds.Size;

		Vector3 scaledSize = new(
			rawSize.X * meshInstance.Scale.X,
			rawSize.Y * meshInstance.Scale.Y,
			rawSize.Z * meshInstance.Scale.Z
		);

		if (scaledSize.Z < 0.05f)
			scaledSize.Z = 0.05f;

		boxShape.Size = scaledSize;
		collisionShape.Shape = boxShape;

		collisionShape.Position = rawBounds.GetCenter() * meshInstance.Scale;

	}

}