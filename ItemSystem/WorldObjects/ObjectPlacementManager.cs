namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using Character;
using GameWorld;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using ItemSystem;
using ItemSystem.WorldObjects.Hierarchy;
using ItemSystem.WorldObjects.House;
using Root;
using Services;

public partial class ObjectPlacementManager : Node {
	private static readonly LogService Log = new(nameof(ObjectPlacementManager), enabled: true);
	private const float DefaultPlaceDistance = 2.0f;
	private const float PlaceHeightMaxDifference = 2.0f;
	private const float RayLength = 100.0f;
	private const float MinPlaceSurfaceNormalY = 0.6f;
	private const float PlacementLinecastHeight = 1.0f;
	private const float PlacementFineRotationStepRadians = Mathf.Pi / 36.0f;
	private const float PlacementSnapRotationStepRadians = Mathf.Pi / 4.0f;
	private const float WallPlacementBaseHeight = PlacementLinecastHeight;
	private const float WallHeightRaiseCap = 4.0f;
	private bool _isInitialized;
	private float CurrentPlacingRotationOffsetY;
	private float PlacementStartRotationOffsetY;
	private float CurrentPlacingDistanceCompensation;
	private string? CurrentSurfaceObjectId;
	private string? CurrentWallAnchorId;
	private Vector3 CurrentWallNormal = Vector3.Forward;
	private readonly List<PlacementAreaShapeTemplate> CurrentPlacementAreaShapes = new();

	public WorldObjectManager? WorldObjectManager { get; private set; }
	public InventoryManager? InventoryManager { get; private set; }
	public GameManager? GameManager { get; private set; }
	public Hotbar? PlayerHotbar { get; private set; }
	public Player? Player { get; private set; }
	public bool Initalized => WorldObjectManager != null && InventoryManager != null && GameManager != null && PlayerHotbar != null && Player != null;

	private enum PlaceState { Idle, FindingPlacableLocation, Placable };
	private StateMachine<PlaceState> PlaceStateMachine = new StateMachine<PlaceState>(PlaceState.Idle);
	public bool IsPlacementActive => PlaceStateMachine.CurrentState == PlaceState.FindingPlacableLocation || PlaceStateMachine.CurrentState == PlaceState.Placable;
	public string? CurrentPlacingItemId { get; private set; }
	public Vector3 CurrentPlacingPosition { get; private set; }
	public Vector3 CurrentPlacingRotation { get; private set; }
	public event Action<Vector3, Vector3>? OnPlacingObject;
	public event Action<bool>? OnPlacingObjectValidChanged;
	public event Action<string>? StartPlacingObject;
	public event Action? EndPlacingObject;

	private readonly record struct PlacementAreaShapeTemplate(Shape3D Shape, Transform3D LocalTransform);

	public void Initialize(WorldObjectManager worldObjectManager, InventoryManager inventoryManager, GameManager gameManager, Hotbar playerHotbar, Player player) {
		if(_isInitialized) {
			WorldObjectManager = worldObjectManager;
			InventoryManager = inventoryManager;
			GameManager = gameManager;
			PlayerHotbar = playerHotbar;
			Player = player;
			Log.Info("Initialize called more than once; refreshed placement dependencies.");
			return;
		}
		WorldObjectManager = worldObjectManager;
		InventoryManager = inventoryManager;
		GameManager = gameManager;
		PlayerHotbar = playerHotbar;
		Player = player;
		playerHotbar.OnSlotSelected += OnHotbarSlotSelected;
		ConfigureStateMachine();
		_isInitialized = true;
	}

	public void ConfigureStateMachine() {
		PlaceStateMachine.OnEnter(PlaceState.Idle, () => {
			CurrentPlacingRotationOffsetY = 0.0f;
			PlacementStartRotationOffsetY = 0.0f;
			CurrentPlacingDistanceCompensation = 0.0f;
			CurrentSurfaceObjectId = null;
			CurrentWallAnchorId = null;
			CurrentWallNormal = Vector3.Forward;
			CurrentPlacementAreaShapes.Clear();
			EndPlacingObject?.Invoke();
		});
		PlaceStateMachine.OnSpecific(PlaceState.Idle, PlaceState.FindingPlacableLocation, () => {
			PlacementStartRotationOffsetY = CurrentPlacingRotationOffsetY;
			RefreshPlacementAreaShapeTemplates();
			StartPlacingObject?.Invoke(CurrentPlacingItemId!);
		});
		PlaceStateMachine.OnEnter(PlaceState.FindingPlacableLocation, () => {
			OnPlacingObjectValidChanged?.Invoke(false);
		});
		PlaceStateMachine.OnEnter(PlaceState.Placable, () => {
			OnPlacingObjectValidChanged?.Invoke(true);
		});
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if(!Initalized) {
			return;
		}
		switch(PlaceStateMachine.CurrentState) {
			case PlaceState.Idle:
				break;
			case PlaceState.FindingPlacableLocation:
				CurrentPlacingPosition = GetPositionInFrontOfPlayer(Player!, out bool surfaceValid);
				CurrentPlacingRotation = GetAdjustedPlacementRotation(Player!, CurrentPlacingPosition);
				bool success = surfaceValid && IsPlacementAreaClear(CurrentPlacingPosition, CurrentPlacingRotation);
				OnPlacingObject?.Invoke(CurrentPlacingPosition, CurrentPlacingRotation);
				if(success) {
					PlaceStateMachine.TransitionTo(PlaceState.Placable);
				}
				break;
			case PlaceState.Placable:
				CurrentPlacingPosition = GetPositionInFrontOfPlayer(Player!, out bool surfaceStillValid);
				CurrentPlacingRotation = GetAdjustedPlacementRotation(Player!, CurrentPlacingPosition);
				bool stillValid = surfaceStillValid && IsPlacementAreaClear(CurrentPlacingPosition, CurrentPlacingRotation);
				if(!stillValid) {
					PlaceStateMachine.TransitionTo(PlaceState.FindingPlacableLocation);
					break;
				}
				OnPlacingObject?.Invoke(CurrentPlacingPosition, CurrentPlacingRotation);
				break;
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		if(PlaceStateMachine.CurrentState != PlaceState.FindingPlacableLocation && PlaceStateMachine.CurrentState != PlaceState.Placable) {
			return;
		}
		if(!Initalized || Player == null || !GodotObject.IsInstanceValid(Player)) {
			return;
		}
		if(@event is not InputEventMouseButton mouseButtonEvent || !mouseButtonEvent.Pressed) {
			return;
		}

		int wheelDirection = 0;
		if(mouseButtonEvent.ButtonIndex == MouseButton.WheelUp) {
			wheelDirection = -1;
		}
		else if(mouseButtonEvent.ButtonIndex == MouseButton.WheelDown) {
			wheelDirection = 1;
		}

		if(wheelDirection == 0) {
			return;
		}
		if(IsCurrentPlacingItemWallObject()) {
			return;
		}

		Vector3 playerForward = Player.GlobalBasis.Z;
		playerForward.Y = 0;
		if(playerForward.LengthSquared() < 0.0001f) {
			playerForward = Vector3.Forward;
		}
		playerForward = playerForward.Normalized();

		Vector3 oldRotation = GetAdjustedPlacementRotation(Player, CurrentPlacingPosition);
		float oldOriginOffset = GetPlacementOriginOffset(playerForward, oldRotation);

		float oldRotationOffset = CurrentPlacingRotationOffsetY;
		bool middleMouseHeld = Input.IsMouseButtonPressed(MouseButton.Middle);
		if(middleMouseHeld) {
			CurrentPlacingRotationOffsetY = oldRotationOffset + (wheelDirection * PlacementFineRotationStepRadians);
		}
		else {
			CurrentPlacingRotationOffsetY = GetSnappedRotationOffset(oldRotationOffset, wheelDirection);
		}
		CurrentPlacingRotationOffsetY = Mathf.Wrap(CurrentPlacingRotationOffsetY, -Mathf.Pi, Mathf.Pi);

		Vector3 newRotation = GetAdjustedPlacementRotation(Player, CurrentPlacingPosition);
		float newOriginOffset = GetPlacementOriginOffset(playerForward, newRotation);

		// Keep the preview anchored relative to player while rotating by counteracting origin shift.
		CurrentPlacingDistanceCompensation += oldOriginOffset - newOriginOffset;
	}

	private float GetSnappedRotationOffset(float currentOffset, int wheelDirection) {
		float relativeFromStart = (currentOffset - PlacementStartRotationOffsetY) / PlacementSnapRotationStepRadians;
		int nearestStepIndex = Mathf.RoundToInt(relativeFromStart);
		int targetStepIndex = nearestStepIndex + wheelDirection;
		return PlacementStartRotationOffsetY + (targetStepIndex * PlacementSnapRotationStepRadians);
	}

	public override void _ExitTree() {
		if(PlayerHotbar != null) {
			PlayerHotbar.OnSlotSelected -= OnHotbarSlotSelected;
		}
	}

	public void PlaceRequested() {
		if(!Initalized) {
			Log.Error("PlaceRequested failed: ObjectPlacementManager is not initialized.");
			return;
		}
		if(PlaceStateMachine.CurrentState == PlaceState.Idle) {
			if(!TryGetSelectedPlaceableItemId(out string selectedItemId)) {
				Log.Info("PlaceRequested called but no item is currently selected for placing.");
				return;
			}
			CurrentPlacingItemId = selectedItemId;
			PlaceStateMachine.TransitionTo(PlaceState.FindingPlacableLocation);
		}
		if(PlaceStateMachine.CurrentState == PlaceState.Placable) {
			PlaceObject();
			PlaceStateMachine.TransitionTo(PlaceState.Idle);
		}
	}

	public void PlaceCanceled() {
		if(PlaceStateMachine.CurrentState == PlaceState.FindingPlacableLocation || PlaceStateMachine.CurrentState == PlaceState.Placable) {
			PlaceStateMachine.TransitionTo(PlaceState.Idle);
		}
	}

	public void OnHotbarSlotSelected(ItemSlot selectedSlot) {
		if(selectedSlot.IsEmpty()) {
			if(PlaceStateMachine.CurrentState == PlaceState.FindingPlacableLocation || PlaceStateMachine.CurrentState == PlaceState.Placable) {
				PlaceStateMachine.TransitionTo(PlaceState.Idle);
			}
			return;
		}
		string? itemId = selectedSlot.Item?.Id;
		if(!IsPlaceable(itemId)) {
			if(PlaceStateMachine.CurrentState == PlaceState.FindingPlacableLocation || PlaceStateMachine.CurrentState == PlaceState.Placable) {
				PlaceStateMachine.TransitionTo(PlaceState.Idle);
			}
			return;
		}
		CurrentPlacingItemId = itemId;
		PlaceCanceled();
	}

	private void PlaceObject() {
		if(!Initalized) {
			Log.Error("PlaceObject failed: ObjectPlacementManager is not initialized.");
			return;
		}
		string? currentItemId = CurrentPlacingItemId;
		if(currentItemId == null || !IsPlaceable(currentItemId)) {
			Log.Error("PlaceObject failed: CurrentPlacingItemId is not placeable.");
			return;
		}
		ItemSlot selectedSlot = PlayerHotbar!.GetSelectedItemSlot();
		if(selectedSlot.IsEmpty() || selectedSlot.Item?.Id != currentItemId) {
			Log.Info("PlaceObject canceled: selected hotbar slot no longer has the placing item.");
			return;
		}
		bool created = WorldObjectManager!.CreateWorldObject(currentItemId, CurrentPlacingPosition, CurrentPlacingRotation, CurrentWallAnchorId ?? string.Empty);
		if(!created) {
			Log.Error($"PlaceObject failed to create world object for ItemId '{CurrentPlacingItemId}'.");
			return;
		}
		InventoryManager!.ConsumeSelectedHotbar(PlayerHotbar!, 1);
	}

	private bool TryGetSelectedPlaceableItemId(out string itemId) {
		itemId = string.Empty;
		ItemSlot selectedSlot = PlayerHotbar!.GetSelectedItemSlot();
		if(selectedSlot.IsEmpty()) {
			return false;
		}
		string? selectedItemId = selectedSlot.Item?.Id;
		if(!IsPlaceable(selectedItemId)) {
			return false;
		}
		itemId = selectedItemId!;
		return true;
	}

	private void RefreshPlacementAreaShapeTemplates() {
		CurrentPlacementAreaShapes.Clear();
		if(string.IsNullOrWhiteSpace(CurrentPlacingItemId)) {
			return;
		}
		ItemDefinition? itemDef = DatabaseManager.Instance.GetItemDefinitionById(CurrentPlacingItemId);
		if(itemDef?.ItemScene == null) {
			return;
		}
		Node3D probe = itemDef.ItemScene.Instantiate<Node3D>();
		CollectPlacementAreaShapes(probe, Transform3D.Identity, false);
		probe.QueueFree();
	}

	private void CollectPlacementAreaShapes(Node node, Transform3D accumulatedTransform, bool insideArea) {
		Transform3D nextTransform = accumulatedTransform;
		if(node is Node3D node3D) {
			nextTransform = accumulatedTransform * node3D.Transform;
		}

		bool currentInsideArea = insideArea || node is Area3D;
		if(currentInsideArea && node is CollisionShape3D collisionShape && collisionShape.Shape != null) {
			CurrentPlacementAreaShapes.Add(new PlacementAreaShapeTemplate(collisionShape.Shape, nextTransform));
		}

		foreach(Node child in node.GetChildren()) {
			CollectPlacementAreaShapes(child, nextTransform, currentInsideArea);
		}
	}

	private bool IsPlacementAreaClear(Vector3 position, Vector3 rotation) {
		if(CurrentPlacementAreaShapes.Count == 0) {
			return true;
		}
		if(GameManager == null) {
			return true;
		}
		Viewport? viewport = GameManager.GetViewport();
		if(viewport?.World3D == null) {
			return true;
		}

		PhysicsDirectSpaceState3D spaceState = viewport.World3D.DirectSpaceState;
		Transform3D objectTransform = new Transform3D(Basis.FromEuler(rotation), position);

		foreach(PlacementAreaShapeTemplate shapeTemplate in CurrentPlacementAreaShapes) {
			PhysicsShapeQueryParameters3D query = new PhysicsShapeQueryParameters3D();
			query.Shape = shapeTemplate.Shape;
			query.Transform = objectTransform * shapeTemplate.LocalTransform;
			query.CollideWithAreas = true;
			query.CollideWithBodies = true;

			if(Player != null && GodotObject.IsInstanceValid(Player)) {
				query.Exclude = new Godot.Collections.Array<Rid> { Player.GetRid() };
			}

			Godot.Collections.Array<Godot.Collections.Dictionary> overlaps = spaceState.IntersectShape(query, 8);
			foreach(Godot.Collections.Dictionary overlap in overlaps) {
				if(IsBlockingPlacementOverlap(overlap)) {
					return false;
				}
			}
		}

		return true;
	}

	private bool IsBlockingPlacementOverlap(Godot.Collections.Dictionary overlap) {
		if(!overlap.ContainsKey("collider")) {
			return false;
		}
		Node? colliderNode = overlap["collider"].AsGodotObject() as Node;
		if(colliderNode is StaticBody3D) {
			return true;
		}
		if(colliderNode is not Area3D) {
			return false;
		}
		Node? current = colliderNode;
		while(current != null) {
			if(current is ObjectNode objectNode) {
				return objectNode.Data.Id != CurrentSurfaceObjectId;
			}
			current = current.GetParent();
		}
		return false;
	}

	public bool PlaceObjectInFrontOfPlayer(Player player, string itemId, float distance = DefaultPlaceDistance) {
		if(!Initalized) {
			Log.Error("ObjectPlacementManager is not initialized.");
			return false;
		}
		if(player == null || !GodotObject.IsInstanceValid(player)) {
			Log.Error("PlaceObjectInFrontOfPlayer failed: player is invalid.");
			return false;
		}
		if(!IsPlaceable(itemId)) {
			Log.Error("PlaceObjectInFrontOfPlayer failed: itemId is not placeable.");
			return false;
		}
		bool success = false;
		Vector3 position = GetPositionInFrontOfPlayer(player, out success, distance);
		Vector3 rotation = IsWallObject(itemId) ? GetWallPlacementRotation() : GetRotationFacingPlayer(player, position);
		if(!success) {
			return success;
		}

		bool created = WorldObjectManager!.CreateWorldObject(itemId, position, rotation, CurrentWallAnchorId ?? string.Empty);
		if(!created) {
			Log.Error($"PlaceObjectInFrontOfPlayer failed to create world object for ItemId '{itemId}'.");
		}
		return created;
	}

	public Vector3 GetPositionInFrontOfPlayer(Player player, out bool success, float distance = DefaultPlaceDistance) {
		success = false;
		CurrentSurfaceObjectId = null;
		CurrentWallAnchorId = null;
		if(player == null || !GodotObject.IsInstanceValid(player)) {
			Log.Error("Player is invalid.");
			return Vector3.Zero;
		}
		float placeDistance = Mathf.Max(0.5f, distance);
		Vector3 forward = player.GlobalBasis.Z;
		forward.Y = 0;
		if(forward.LengthSquared() < 0.0001f) {
			forward = Vector3.Forward;
		}
		forward = forward.Normalized();
		Vector3 previewRotation = GetAdjustedPlacementRotation(player, player.GlobalPosition + (forward * placeDistance));
		float originOffset = GetPlacementOriginOffset(forward, previewRotation);
		float targetDistance = placeDistance + originOffset + CurrentPlacingDistanceCompensation;
		Vector3 targetPosition = player.GlobalPosition + (forward * targetDistance);
		targetPosition.Y = player.GlobalPosition.Y;

		if(IsCurrentPlacingItemWallObject()) {
			return GetPositionOnWall(player, targetPosition, targetDistance, out success);
		}

		Vector3 groundPosition = GetPositionOnGround(targetPosition, out success, out string? surfaceObjectId);
		CurrentSurfaceObjectId = surfaceObjectId;
		if(success && IsPlacementObstructedByWall(player, groundPosition)) {
			success = false;
		}
		return groundPosition;
	}

	private Vector3 GetPositionOnWall(Player player, Vector3 targetPosition, float targetDistance, out bool success) {
		success = false;
		if(GameManager == null) {
			return targetPosition;
		}
		Viewport? viewport = GameManager.GetViewport();
		if(viewport?.World3D == null) {
			return targetPosition;
		}

		Vector3 playerOrigin = player.GlobalPosition + (Vector3.Up * PlacementLinecastHeight);
		Vector3 castDirection = targetPosition - player.GlobalPosition;
		castDirection.Y = 0.0f;
		if(castDirection.LengthSquared() < 0.0001f) {
			return targetPosition;
		}
		castDirection = castDirection.Normalized();
		Vector3 castEnd = playerOrigin + (castDirection * targetDistance);

		PhysicsDirectSpaceState3D spaceState = viewport.World3D.DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(playerOrigin, castEnd);
		query.CollideWithAreas = false;
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
		Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
		if(result.Count == 0 || !result.ContainsKey("position") || !result.ContainsKey("normal") || !result.ContainsKey("collider")) {
			return targetPosition;
		}

		Node? colliderNode = result["collider"].AsGodotObject() as Node;
		if(!TryGetWallAnchorIdFromCollider(colliderNode, out string wallAnchorId)) {
			return targetPosition;
		}

		Vector3 hitPosition = (Vector3) result["position"];
		Vector3 wallNormal = ((Vector3) result["normal"]).Normalized();
		float hitDistance = playerOrigin.DistanceTo(hitPosition);
		float closenessRatio = targetDistance <= Numbers.EPSILON
			? 0.0f
			: Mathf.Clamp((targetDistance - hitDistance) / targetDistance, 0.0f, 1.0f);
		float wallHeightOffset = closenessRatio * WallHeightRaiseCap;

		Vector3 wallPosition = hitPosition;
		wallPosition.Y = player.GlobalPosition.Y + WallPlacementBaseHeight + wallHeightOffset;
		CurrentWallNormal = wallNormal;
		CurrentWallAnchorId = wallAnchorId;
		success = true;
		return wallPosition;
	}

	private bool IsPlacementObstructedByWall(Player player, Vector3 targetPosition) {
		if(GameManager == null) {
			return false;
		}
		Viewport? viewport = GameManager.GetViewport();
		if(viewport?.World3D == null) {
			return false;
		}
		var spaceState = viewport.World3D.DirectSpaceState;
		Vector3 from = player.GlobalPosition + Vector3.Up * PlacementLinecastHeight;
		Vector3 to = targetPosition + Vector3.Up * PlacementLinecastHeight;
		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = false;
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
		var result = spaceState.IntersectRay(query);
		if(result.Count == 0 || !result.ContainsKey("normal")) {
			return false;
		}
		Vector3 hitNormal = (Vector3) result["normal"];
		return hitNormal.Y < MinPlaceSurfaceNormalY;
	}

	public Vector3 GetRotationFacingPlayer(Player player, Vector3 objectPosition) {
		if(player == null || !GodotObject.IsInstanceValid(player)) {
			Log.Error("Player is invalid.");
			return Vector3.Zero;
		}
		Vector3 directionToPlayer = (player.GlobalPosition - objectPosition).Normalized();
		float angle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
		return new Vector3(0, angle, 0);
	}

	private Vector3 GetAdjustedPlacementRotation(Player player, Vector3 objectPosition) {
		if(IsCurrentPlacingItemWallObject()) {
			return GetWallPlacementRotation();
		}
		Vector3 baseRotation = GetRotationFacingPlayer(player, objectPosition);
		return new Vector3(baseRotation.X, baseRotation.Y + CurrentPlacingRotationOffsetY, baseRotation.Z);
	}

	private Vector3 GetWallPlacementRotation() {
		Vector3 normal = CurrentWallNormal;
		normal.Y = 0.0f;
		if(normal.LengthSquared() < 0.0001f) {
			normal = Vector3.Forward;
		}
		normal = normal.Normalized();
		float angle = Mathf.Atan2(normal.X, normal.Z);
		return new Vector3(0, angle, 0);
	}

	private float GetPlacementOriginOffset(Vector3 forward, Vector3 rotation) {
		if(CurrentPlacementAreaShapes.Count == 0) {
			return 0.0f;
		}

		Basis placementBasis = Basis.FromEuler(rotation);
		float minimumProjection = 0.0f;
		bool hasProjection = false;

		foreach(PlacementAreaShapeTemplate shapeTemplate in CurrentPlacementAreaShapes) {
			ArrayMesh debugMesh = shapeTemplate.Shape.GetDebugMesh();
			Aabb localBounds = debugMesh.GetAabb();
			Vector3[] corners = GetAabbCorners(localBounds);

			foreach(Vector3 corner in corners) {
				Vector3 transformedCorner = placementBasis * (shapeTemplate.LocalTransform * corner);
				float projection = transformedCorner.Dot(forward);
				if(!hasProjection || projection < minimumProjection) {
					minimumProjection = projection;
					hasProjection = true;
				}
			}
		}

		if(!hasProjection) {
			return 0.0f;
		}

		return -minimumProjection;
	}

	private static Vector3[] GetAabbCorners(Aabb aabb) {
		Vector3 min = aabb.Position;
		Vector3 max = aabb.Position + aabb.Size;
		return [
			new Vector3(min.X, min.Y, min.Z),
			new Vector3(max.X, min.Y, min.Z),
			new Vector3(min.X, max.Y, min.Z),
			new Vector3(max.X, max.Y, min.Z),
			new Vector3(min.X, min.Y, max.Z),
			new Vector3(max.X, min.Y, max.Z),
			new Vector3(min.X, max.Y, max.Z),
			new Vector3(max.X, max.Y, max.Z),
		];
	}

	public Vector3 GetPositionOnGround(Vector3 position, out bool success, out string? surfaceObjectId) {
		float height = position.Y;
		success = false;
		surfaceObjectId = null;
		if(GameManager == null) {
			return position;
		}
		Viewport? viewport = GameManager.GetViewport();
		if(viewport?.World3D == null) {
			return position;
		}
		var spaceState = viewport.World3D.DirectSpaceState;
		var origin = position + Vector3.Up * PlaceHeightMaxDifference;
		var end = position + Vector3.Down * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end);
		query.CollideWithAreas = false;
		if(Player != null && GodotObject.IsInstanceValid(Player)) {
			query.Exclude = new Godot.Collections.Array<Rid> { Player.GetRid() };
		}
		var result = spaceState.IntersectRay(query);
		if(result.Count > 0 && result.ContainsKey("position") && result.ContainsKey("normal")) {
			Vector3 groundPosition = (Vector3) result["position"];
			Vector3 normal = (Vector3) result["normal"];
			if(normal.Y < MinPlaceSurfaceNormalY) {
				return position;
			}

			if(result.ContainsKey("collider")) {
				Node? surfaceCollider = result["collider"].AsGodotObject() as Node;
				ObjectNode? surfaceObjectNode = FindAncestorObjectNode(surfaceCollider);
				if(surfaceObjectNode != null) {
					ItemDefinition? surfaceDefinition = DatabaseManager.Instance.GetItemDefinitionById(surfaceObjectNode.Data.ItemId);
					if(surfaceDefinition?.Can_Object_Stack != true) {
						return position;
					}
					surfaceObjectId = surfaceObjectNode.Data.Id;
				}
			}

			float heightDifference = Mathf.Abs(groundPosition.Y - height);
			if(heightDifference <= PlaceHeightMaxDifference) {
				success = true;
				return groundPosition;
			}
		}
		return position;
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

	public bool IsPlaceable(string? itemId) {
		if(string.IsNullOrWhiteSpace(itemId)) {
			return false;
		}
		ItemDefinition? itemDef = DatabaseManager.Instance.GetItemDefinitionById(itemId);
		return itemDef != null && itemDef.IsPlaceable;
	}

	private bool IsCurrentPlacingItemWallObject() {
		if(string.IsNullOrWhiteSpace(CurrentPlacingItemId)) {
			return false;
		}
		return IsWallObject(CurrentPlacingItemId);
	}

	private static bool IsWallObject(string? itemId) {
		if(string.IsNullOrWhiteSpace(itemId)) {
			return false;
		}
		ItemDefinition? itemDef = DatabaseManager.Instance.GetItemDefinitionById(itemId);
		return itemDef?.IsWallObject == true;
	}

	private static Walls? FindWallFromNode(Node? node) {
		Node? current = node;
		while(current != null) {
			if(current is Walls wallNode) {
				return wallNode;
			}
			current = current.GetParent();
		}
		return null;
	}

	private static bool TryFindAnchorInWallBranch(Node node, out string anchorId) {
		anchorId = string.Empty;
		if(node is WorldObjectHierarchyAnchor anchor && !string.IsNullOrWhiteSpace(anchor.AnchorId)) {
			anchorId = anchor.AnchorId;
			return true;
		}
		foreach(Node child in node.GetChildren()) {
			if(TryFindAnchorInWallBranch(child, out anchorId)) {
				return true;
			}
		}
		return false;
	}

	private static bool TryGetWallAnchorIdFromCollider(Node? colliderNode, out string anchorId) {
		anchorId = string.Empty;
		Walls? wallNode = FindWallFromNode(colliderNode);
		if(wallNode == null || !GodotObject.IsInstanceValid(wallNode)) {
			return false;
		}
		return TryFindAnchorInWallBranch(wallNode, out anchorId);
	}
}

