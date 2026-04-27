namespace ItemSystem.WorldObjects;

using System.Collections.Generic;
using Godot;

public sealed class ObjectHoverOutlineUI {
	private const float BaseOutlineWidth = 0.025f;
	private readonly ObjectHoverTargetingController HoverTargetingController;
	private readonly Dictionary<MeshInstance3D, Material?> PreviousOverlayByMesh = [];
	private ObjectNode? CurrentOutlinedObjectNode;

	public ObjectHoverOutlineUI(ObjectHoverTargetingController hoverTargetingController) {
		HoverTargetingController = hoverTargetingController;
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
		MeshOutlineOverlayUtility.ApplyOutline(objectNode, PreviousOverlayByMesh, BaseOutlineWidth);
	}

	private void ClearCurrentOutline() {
		MeshOutlineOverlayUtility.RestoreOutline(PreviousOverlayByMesh);
		CurrentOutlinedObjectNode = null;
	}
}
