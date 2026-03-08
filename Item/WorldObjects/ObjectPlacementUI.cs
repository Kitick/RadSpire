namespace Objects {
	using System.Collections.Generic;
	using Godot;
	using ItemSystem;
	using Services;

	public partial class ObjectPlacementUI : Node {
		private static readonly LogService Log = new(nameof(ObjectPlacementUI), enabled: true);

		private static readonly Color ValidPreviewColor = new Color(0.45f, 0.7f, 1.0f, 0.55f);
		private static readonly Color InvalidPreviewColor = new Color(1.0f, 0.2f, 0.2f, 0.65f);

		private Node3D? PreviewNode;
		public bool HasPreview => PreviewNode != null && GodotObject.IsInstanceValid(PreviewNode);

		public void Initalize(ObjectPlacementManager objectPlacementManager) {
			objectPlacementManager.OnPlacingObject += HandleOnPlacingObject;
			objectPlacementManager.OnPlacingObjectValidChanged += HandleOnPlacingObjectValidChanged;
			objectPlacementManager.StartPlacingObject += HandleStartPlacingObject;
			objectPlacementManager.EndPlacingObject += HandleEndPlacingObject;
		}

		public void HandleOnPlacingObject(Vector3 position, Vector3 rotation) {
			if(HasPreview) {
				UpdatePreview(position, rotation);
			}
		}

		public void HandleOnPlacingObjectValidChanged(bool isValid) {
			if(HasPreview) {
				if(isValid) {
					ApplyPreviewTint(PreviewNode!, ValidPreviewColor);
				}
				else {
					ApplyPreviewTint(PreviewNode!, InvalidPreviewColor);
				}
			}
		}

		public void HandleStartPlacingObject(string itemId) {
			BeginPreview(itemId);
		}
		
		public void HandleEndPlacingObject() {
			EndPreview();
		}

		public bool BeginPreview(string itemId) {
			EndPreview();

			ItemDefinition? itemDefinition = ItemDataBaseManager.Instance.GetItemDefinitionById(itemId);
			if(itemDefinition?.ItemScene == null) {
				Log.Error($"BeginPreview failed: ItemScene missing for ItemId '{itemId}'.");
				return false;
			}

			Node3D preview = itemDefinition.ItemScene.Instantiate<Node3D>();
			PreparePreviewNode(preview);
			ApplyPreviewTint(preview, InvalidPreviewColor);
			AddChild(preview);
			PreviewNode = preview;
			return true;
		}

		public void EndPreview() {
			if(HasPreview) {
				PreviewNode!.QueueFree();
			}

			PreviewNode = null;
		}

		public void UpdatePreview(Vector3 position, Vector3 rotation) {
			if(!HasPreview) {
				return;
			}

			PreviewNode!.GlobalPosition = position;
			PreviewNode.GlobalRotation = rotation;
		}

		private static void PreparePreviewNode(Node node) {
			foreach(Node child in node.GetChildren()) {
				PreparePreviewNode(child);
			}

			if(node is CollisionObject3D collisionObject) {
				collisionObject.CollisionLayer = 0;
				collisionObject.CollisionMask = 0;
			}

			if(node is RigidBody3D body) {
				body.Freeze = true;
				body.ProcessMode = ProcessModeEnum.Disabled;
			}
		}

		private static void ApplyPreviewTint(Node node, Color tint) {
			var meshes = new List<MeshInstance3D>();
			CollectMeshes(node, meshes);
			foreach(MeshInstance3D mesh in meshes) {
				StandardMaterial3D material = mesh.MaterialOverride as StandardMaterial3D ?? new StandardMaterial3D();
				material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
				material.AlbedoColor = tint;
				material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
				material.NoDepthTest = false;
				mesh.MaterialOverride = material;
			}
		}

		private static void CollectMeshes(Node node, List<MeshInstance3D> meshes) {
			if(node is MeshInstance3D meshInstance) {
				meshes.Add(meshInstance);
			}

			foreach(Node child in node.GetChildren()) {
				CollectMeshes(child, meshes);
			}
		}
	}
}
