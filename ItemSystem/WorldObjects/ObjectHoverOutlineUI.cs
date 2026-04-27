namespace ItemSystem.WorldObjects;

using System.Collections.Generic;
using Godot;

public sealed class ObjectHoverOutlineUI {
	private const float BaseOutlineWidth = 0.025f;
	private readonly ObjectHoverTargetingController HoverTargetingController;
	private readonly Shader OutlineShader;
	private readonly Dictionary<MeshInstance3D, Material?> PreviousOverlayByMesh = [];
	private ObjectNode? CurrentOutlinedObjectNode;

	public ObjectHoverOutlineUI(ObjectHoverTargetingController hoverTargetingController) {
		HoverTargetingController = hoverTargetingController;
		OutlineShader = CreateOutlineShader();
		HoverTargetingController.HoveredObjectNodeChanged += HandleHoveredObjectNodeChanged;
	}

	public void Dispose() {
		HoverTargetingController.HoveredObjectNodeChanged -= HandleHoveredObjectNodeChanged;
		ClearCurrentOutline();
	}

	private void HandleHoveredObjectNodeChanged(ObjectNode? objectNode) {
		if(CurrentOutlinedObjectNode == objectNode) {
			return;
		}

		ClearCurrentOutline();
		if(objectNode != null) {
			ApplyOutline(objectNode);
			CurrentOutlinedObjectNode = objectNode;
		}
	}

	private void ApplyOutline(ObjectNode objectNode) {
		foreach(MeshInstance3D mesh in EnumerateMeshes(objectNode)) {
			PreviousOverlayByMesh[mesh] = mesh.MaterialOverlay;
			ShaderMaterial outlineMaterial = new() {
				Shader = OutlineShader
			};
			outlineMaterial.SetShaderParameter("outline_width", BaseOutlineWidth);
			mesh.MaterialOverlay = outlineMaterial;
		}
	}

	private void ClearCurrentOutline() {
		foreach(KeyValuePair<MeshInstance3D, Material?> pair in PreviousOverlayByMesh) {
			MeshInstance3D mesh = pair.Key;
			if(GodotObject.IsInstanceValid(mesh)) {
				mesh.MaterialOverlay = pair.Value;
			}
		}

		PreviousOverlayByMesh.Clear();
		CurrentOutlinedObjectNode = null;
	}

	private static IEnumerable<MeshInstance3D> EnumerateMeshes(Node root) {
		foreach(Node child in root.GetChildren()) {
			if(child is MeshInstance3D mesh) {
				yield return mesh;
			}

			foreach(MeshInstance3D nestedMesh in EnumerateMeshes(child)) {
				yield return nestedMesh;
			}
		}
	}

	private static Shader CreateOutlineShader() {
		return new Shader {
			Code = @"
shader_type spatial;
render_mode unshaded, cull_front, depth_draw_opaque, world_vertex_coords;

uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float outline_width = 0.025;

void vertex() {
	// World-space extrusion keeps outline size stable even with parent/object scaling.
	VERTEX += normalize(NORMAL) * outline_width;
}

void fragment() {
	ALBEDO = outline_color.rgb;
	ALPHA = outline_color.a;
}
"
		};
	}
}
