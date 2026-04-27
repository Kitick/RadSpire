namespace ItemSystem.WorldObjects;

using System.Collections.Generic;
using Godot;

public static class MeshOutlineOverlayUtility {
	private static readonly Shader OutlineShader = new() {
		Code = @"
shader_type spatial;
render_mode unshaded, cull_front, depth_draw_opaque, world_vertex_coords;

uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float outline_width = 0.025;

void vertex() {
	VERTEX += normalize(NORMAL) * outline_width;
}

void fragment() {
	ALBEDO = outline_color.rgb;
	ALPHA = outline_color.a;
}
"
	};

	public static void ApplyOutline(Node root, Dictionary<MeshInstance3D, Material?> previousOverlayByMesh, float width = 0.025f) {
		foreach(MeshInstance3D mesh in EnumerateMeshes(root)) {
			previousOverlayByMesh[mesh] = mesh.MaterialOverlay;
			ShaderMaterial outlineMaterial = new() {
				Shader = OutlineShader
			};
			outlineMaterial.SetShaderParameter("outline_width", width);
			mesh.MaterialOverlay = outlineMaterial;
		}
	}

	public static void RestoreOutline(Dictionary<MeshInstance3D, Material?> previousOverlayByMesh) {
		foreach(KeyValuePair<MeshInstance3D, Material?> pair in previousOverlayByMesh) {
			MeshInstance3D mesh = pair.Key;
			if(GodotObject.IsInstanceValid(mesh)) {
				mesh.MaterialOverlay = pair.Value;
			}
		}
		previousOverlayByMesh.Clear();
	}

	private static IEnumerable<MeshInstance3D> EnumerateMeshes(Node root) {
		foreach(Node child in root.GetChildren()) {
			if(child is MeshInstance3D mesh) {
				yield return mesh;
			}
			foreach(MeshInstance3D nested in EnumerateMeshes(child)) {
				yield return nested;
			}
		}
	}
}
