namespace Objects;

using System;
using Character;
using GameWorld;
using Godot;
using ItemSystem;
using Root;
using Services;
using UI.Hotbar;

public partial class ObjectPlacementManager : Node {
	private static readonly LogService Log = new(nameof(ObjectPlacementManager), enabled: true);
	private const float DefaultPlaceDistance = 2.0f;
	private const float PlaceHeightMaxDifference = 2.0f;
	private const float RayLength = 100.0f;
	private const float MinPlaceSurfaceNormalY = 0.6f;
	private const float PlacementLinecastHeight = 1.0f;
	private bool _isInitialized;

	public WorldObjectManager? WorldObjectManager { get; private set; }
	public InventoryManager? InventoryManager { get; private set; }
	public GameManager? GameManager { get; private set; }
	public Hotbar? PlayerHotbar { get; private set; }
	public Player? Player { get; private set; }
	public bool Initalized => WorldObjectManager != null && InventoryManager != null && GameManager != null && PlayerHotbar != null && Player != null;

	private enum PlaceState { Idle, FindingPlacableLocation, Placable };
	private StateMachine<PlaceState> PlaceStateMachine = new StateMachine<PlaceState>(PlaceState.Idle);
	public string? CurrentPlacingItemId { get; private set; }
	public Vector3 CurrentPlacingPosition { get; private set; }
	public Vector3 CurrentPlacingRotation { get; private set; }
	public event Action<Vector3, Vector3>? OnPlacingObject;
	public event Action<bool>? OnPlacingObjectValidChanged;
	public event Action<string>? StartPlacingObject;
	public event Action? EndPlacingObject;

	public void Initialize(WorldObjectManager worldObjectManager, InventoryManager inventoryManager, GameManager gameManager, Hotbar playerHotbar, Player player) {
		if(_isInitialized) {
			Log.Info("Initialize called more than once; ignoring duplicate initialization.");
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
			EndPlacingObject?.Invoke();
		});
		PlaceStateMachine.OnSpecific(PlaceState.Idle, PlaceState.FindingPlacableLocation, () => {
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
				CurrentPlacingPosition = GetPositionInFrontOfPlayer(Player!, out bool success);
				CurrentPlacingRotation = GetRotationFacingPlayer(Player!, CurrentPlacingPosition);
				OnPlacingObject?.Invoke(CurrentPlacingPosition, CurrentPlacingRotation);
				if(success) {
					PlaceStateMachine.TransitionTo(PlaceState.Placable);
				}
				break;
			case PlaceState.Placable:
				CurrentPlacingPosition = GetPositionInFrontOfPlayer(Player!, out bool stillValid);
				CurrentPlacingRotation = GetRotationFacingPlayer(Player!, CurrentPlacingPosition);
				if(!stillValid) {
					PlaceStateMachine.TransitionTo(PlaceState.FindingPlacableLocation);
					break;
				}
				OnPlacingObject?.Invoke(CurrentPlacingPosition, CurrentPlacingRotation);
				break;
		}
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
			string? selectedItemSlot = PlayerHotbar!.GetSelectedItemSlot()?.Item?.Id;
			if(!IsPlaceable(selectedItemSlot)) {
				Log.Info("PlaceRequested called but no item is currently selected for placing.");
				return;
			}
			CurrentPlacingItemId = selectedItemSlot;
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
		bool created = WorldObjectManager!.CreateWorldObject(currentItemId, CurrentPlacingPosition, CurrentPlacingRotation);
		if(!created) {
			Log.Error($"PlaceObject failed to create world object for ItemId '{CurrentPlacingItemId}'.");
			return;
		}
		InventoryManager!.ConsumeSelectedHotbar(PlayerHotbar!, 1);
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
		Vector3 rotation = GetRotationFacingPlayer(player, position);
		if(!success) {
			return success;
		}

		bool created = WorldObjectManager!.CreateWorldObject(itemId, position, rotation);
		if(!created) {
			Log.Error($"PlaceObjectInFrontOfPlayer failed to create world object for ItemId '{itemId}'.");
		}
		return created;
	}

	public Vector3 GetPositionInFrontOfPlayer(Player player, out bool success, float distance = DefaultPlaceDistance) {
		success = false;
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
		Vector3 position = player.GlobalPosition + (forward * placeDistance);
		position.Y = player.GlobalPosition.Y;
		Vector3 groundPosition = GetPositionOnGround(position, out success);
		if(success && IsPlacementObstructedByWall(player, groundPosition)) {
			success = false;
		}
		return groundPosition;
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

	public Vector3 GetPositionOnGround(Vector3 position, out bool success) {
		float height = position.Y;
		success = false;
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
			float heightDifference = Mathf.Abs(groundPosition.Y - height);
			if(heightDifference <= PlaceHeightMaxDifference) {
				success = true;
				return groundPosition;
			}
		}
		return position;
	}

	public bool IsPlaceable(string? itemId) {
		if(string.IsNullOrWhiteSpace(itemId)) {
			return false;
		}
		ItemDefinition? itemDef = ItemDataBaseManager.Instance.GetItemDefinitionById(itemId);
		return itemDef != null && itemDef.IsPlaceable;
	}
}

