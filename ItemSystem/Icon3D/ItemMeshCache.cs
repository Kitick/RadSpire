namespace ItemSystem.Icons;

using System.Collections.Generic;
using Godot;
using ItemSystem;
using Services;

public sealed partial class ItemMeshCache : Node {
	private static readonly LogService Log = new(nameof(ItemMeshCache), enabled: true);

	public static ItemMeshCache Instance { get; private set; } = null!;

	public readonly record struct CachedItemMesh(Mesh Mesh, Aabb RawBounds);

	private readonly Dictionary<string, CachedItemMesh> Cache = [];
	private Node3D? TempScene;
	private MeshInstance3D? SpriteMesh;

	public override void _Ready() {
		Instance = this;

		PackedScene template = GD.Load<PackedScene>("res://ItemSystem/Icon3D/ItemIcon3DTemplete.tscn");
		if(template == null) {
			Log.Error("ItemMeshCache: failed to load ItemIcon3DTemplete.tscn");
			return;
		}

		TempScene = template.Instantiate<Node3D>();
		TempScene.Visible = false;
		AddChild(TempScene);

		Node3D? spriteMeshNode = TempScene.GetNodeOrNull<Node3D>("RigidBody3D/SpriteMeshInstance");
		if(spriteMeshNode == null) {
			Log.Error("ItemMeshCache: SpriteMeshInstance node not found in template.");
			TempScene.QueueFree();
			TempScene = null;
			return;
		}

		SpriteMesh = (MeshInstance3D) spriteMeshNode;
	}

	public bool TryGetMesh(string id, out CachedItemMesh result) {
		if(Cache.TryGetValue(id, out result)) {
			return true;
		}

		return TryGenerate(id, out result);
	}

	private bool TryGenerate(string id, out CachedItemMesh result) {
		result = default;

		if(SpriteMesh == null) {
			Log.Error($"ItemMeshCache: cannot generate mesh for '{id}', generator not ready.");
			return false;
		}

		ItemDefinition? def = DatabaseManager.Instance.GetItemDefinitionById(id);
		if(def == null || def.IconTexture == null) {
			Log.Error($"ItemMeshCache: no definition or texture for '{id}'.");
			return false;
		}

		SpriteMesh.Set("texture", def.IconTexture);
		SpriteMesh.Call("update_sprite_mesh");

		if(SpriteMesh.Mesh == null) {
			Log.Error($"ItemMeshCache: mesh generation produced null for '{id}'.");
			return false;
		}

		result = new CachedItemMesh(SpriteMesh.Mesh, SpriteMesh.Mesh.GetAabb());
		Cache[id] = result;
		Log.Info($"ItemMeshCache: generated and cached mesh for '{id}'.");
		return true;
	}
}
