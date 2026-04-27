namespace ItemSystem.WorldObjects;

using System;
using Character;
using Godot;

public sealed partial class ObjectHoverTargetingController : Node {
	private const float DefaultRayLength = 1000f;
	private const int MaxRayHitsToCheck = 32;
	private readonly float RayLength;
	private Player? Player;
	private ObjectPickup? ObjectPickup;
	private ObjectNode? CurrentHoveredObjectNode;

	public event Action<ObjectNode?>? HoveredObjectNodeChanged;

	public ObjectHoverTargetingController(float rayLength = DefaultRayLength) {
		RayLength = rayLength;
	}

	public void Initialize(Player player, ObjectPickup objectPickup) {
		Player = player;
		ObjectPickup = objectPickup;
	}

	public override void _PhysicsProcess(double delta) {
		if(!IsInstanceValid(Player) || ObjectPickup == null) {
			SetHoveredObjectNode(null);
			return;
		}

		ObjectNode? hoveredObjectNode = ResolveHoveredObjectNode();
		SetHoveredObjectNode(hoveredObjectNode);
		if(hoveredObjectNode != null) {
			ObjectPickup.SetHoverOverride(hoveredObjectNode);
		}
		else {
			ObjectPickup.ClearHoverOverride();
		}
	}

	public override void _ExitTree() {
		base._ExitTree();
		ObjectPickup?.ClearHoverOverride();
		SetHoveredObjectNode(null);
	}

	private ObjectNode? ResolveHoveredObjectNode() {
		Camera3D? camera = GetViewport().GetCamera3D();
		if(!IsInstanceValid(camera) || camera.GetWorld3D() == null) {
			return null;
		}

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 origin = camera.ProjectRayOrigin(mousePosition);
		Vector3 normal = camera.ProjectRayNormal(mousePosition);
		Vector3 to = origin + normal * RayLength;
		Vector3 direction = (to - origin).Normalized();
		PhysicsDirectSpaceState3D spaceState = camera.GetWorld3D().DirectSpaceState;
		Godot.Collections.Array<Rid> exclude = [];
		float remainingDistance = RayLength;
		Vector3 rayStart = origin;

		for(int i = 0; i < MaxRayHitsToCheck && remainingDistance > 0f; i++) {
			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayStart, rayStart + direction * remainingDistance);
			query.CollideWithAreas = true;
			query.CollideWithBodies = true;
			query.Exclude = exclude;

			Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
			if(result.Count == 0 || !result.ContainsKey("collider")) {
				return null;
			}

			Node? collider = result["collider"].AsGodotObject() as Node;
			ObjectNode? hoveredObjectNode = FindAncestorObjectNode(collider);
			if(hoveredObjectNode != null && hoveredObjectNode.Data != null && IsInstanceValid(Player)) {
				float distance = hoveredObjectNode.GlobalPosition.DistanceTo(Player.GlobalPosition);
				if(distance <= ObjectPickup!.HoverTargetDistance) {
					return hoveredObjectNode;
				}
				return null;
			}

			if(result.ContainsKey("rid")) {
				Rid hitRid = (Rid) result["rid"];
				if(hitRid.IsValid) {
					exclude.Add(hitRid);
				}
			}

			if(result.ContainsKey("position")) {
				Vector3 hitPosition = (Vector3) result["position"];
				float advanced = rayStart.DistanceTo(hitPosition) + 0.01f;
				rayStart = hitPosition + direction * 0.01f;
				remainingDistance -= advanced;
			}
			else {
				break;
			}
		}

		return null;
	}

	private void SetHoveredObjectNode(ObjectNode? hoveredObjectNode) {
		if(CurrentHoveredObjectNode == hoveredObjectNode) {
			return;
		}

		CurrentHoveredObjectNode = hoveredObjectNode;
		HoveredObjectNodeChanged?.Invoke(CurrentHoveredObjectNode);
	}

	private static ObjectNode? FindAncestorObjectNode(Node? node) {
		Node? current = node;
		while(current != null) {
			if(current is ObjectNode objectNode) {
				return objectNode;
			}
			current = current.GetParent();
		}

		return null;
	}
}
