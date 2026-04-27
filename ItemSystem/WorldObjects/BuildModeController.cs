namespace ItemSystem.WorldObjects;

using Character;
using GameWorld;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using ItemSystem;
using Services;

public sealed partial class BuildModeController : Node {
	private static readonly LogService Log = new(nameof(BuildModeController), enabled: true);
	private const string BuildInventoryName = "BuildInventory";

	private Player Player = null!;
	private InventoryManager InventoryManager = null!;
	private ObjectPlacementManager PlacementManager = null!;
	private ObjectPlacementUI PlacementUI = null!;
	private WorldObjectManager WorldObjectManager = null!;
	private GameManager GameManager = null!;
	private InventoryUI BuildUI = null!;

	private Inventory? BuildInventory;
	private bool IsDragging;
	private string DragItemId = string.Empty;
	private string DraggedWorldObjectId = string.Empty;

	public bool IsBuildModeActive { get; private set; }

	public void Initialize(
		Player player,
		InventoryManager inventoryManager,
		ObjectPlacementManager placementManager,
		ObjectPlacementUI placementUI,
		WorldObjectManager worldObjectManager,
		GameManager gameManager,
		InventoryUI buildUI
	) {
		Player = player;
		InventoryManager = inventoryManager;
		PlacementManager = placementManager;
		PlacementUI = placementUI;
		WorldObjectManager = worldObjectManager;
		GameManager = gameManager;
		BuildUI = buildUI;
	}

	public void ToggleBuildMode() {
		if(IsBuildModeActive) {
			ExitBuildMode();
			return;
		}
		if(!GameManager.IsPlayerInInteriorWorld()) {
			Log.Info("Build mode blocked: player is not in an interior world.");
			return;
		}
		EnterBuildMode();
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if(!IsBuildModeActive) {
			return;
		}
		PlacementManager.HandleExternalPlacementWheelInput(@event);
		if(@event is not InputEventMouseButton mouseButton) {
			return;
		}

		if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
			if(!IsDragging) {
				// Prefer world-object drag when clicking directly on furniture.
				StartDragFromHoveredWorldObject();
				// Fallback to BuildUI source drag when no world drag started.
				if(!IsDragging) {
					TryStartDragFromBuildInventory(mouseButton.GlobalPosition);
				}
			}
			return;
		}

		if(mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed) {
			FinalizeLeftMouseRelease(mouseButton.GlobalPosition);
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if(!IsBuildModeActive || !IsDragging) {
			return;
		}
		if(!PlacementManager.TryGetExternalPlacementPose(out Vector3 position, out Vector3 rotation, out bool isValid)) {
			return;
		}
		PlacementUI.UpdateBuildDragPreview(position, rotation, isValid);
	}

	public void ExitBuildMode() {
		if(!IsBuildModeActive) {
			return;
		}
		CancelDraggingToBuildInventory();
		ReturnBuildInventoryToPlayer();
		GameManager.HUDRef?.CloseBuildUI();
		IsBuildModeActive = false;
	}

	private void EnterBuildMode() {
		BuildInventory = new Inventory(4, 8) { Name = BuildInventoryName };
		BuildUI.Initialize(BuildInventory, Player);
		BuildUI.SetLabelText("Build");
		GameManager.HUDRef?.OpenBuildUI();
		InventoryManager.MovePlaceableItems(Player.Hotbar, BuildInventory);
		InventoryManager.MovePlaceableItems(Player.Inventory, BuildInventory);
		IsBuildModeActive = true;
	}

	private void ReturnBuildInventoryToPlayer() {
		if(BuildInventory == null) {
			return;
		}
		InventoryManager.ReturnInventoryToPlayerOrDrop(BuildInventory);
	}

	private void StartDragFromHoveredWorldObject() {
		if(IsDragging || BuildInventory == null) {
			return;
		}
		ObjectNode? hoveredObject = Player.ObjectPickup?.CurrentTargetObjectNode;
		if(hoveredObject?.Data == null) {
			return;
		}
		ItemDefinition? itemDef = DatabaseManager.Instance.GetItemDefinitionById(hoveredObject.Data.ItemId);
		if(itemDef?.IsPlaceable != true) {
			return;
		}

		DragItemId = hoveredObject.Data.ItemId;
		DraggedWorldObjectId = hoveredObject.Data.Id;
		float initialRotationY = hoveredObject.GlobalRotation.Y;
		if(!PlacementManager.BeginExternalPlacementPreview(DragItemId, initialRotationY)) {
			DragItemId = string.Empty;
			DraggedWorldObjectId = string.Empty;
			return;
		}

		if(!WorldObjectManager.RemoveWorldObject(DraggedWorldObjectId)) {
			PlacementManager.EndExternalPlacementPreview();
			DragItemId = string.Empty;
			DraggedWorldObjectId = string.Empty;
			return;
		}

		IsDragging = true;
		if(!PlacementUI.BeginBuildDragPreview(DragItemId)) {
			StopDragging();
			return;
		}
	}

	private void TryStartDragFromBuildInventory(Vector2 mousePosition) {
		if(IsDragging || BuildInventory == null || !BuildUI.GetGlobalRect().HasPoint(mousePosition)) {
			return;
		}
		for(int i = 0; i < BuildInventory.ItemSlots.Length; i++) {
			ItemSlot slot = BuildInventory.ItemSlots[i];
			if(slot.IsEmpty() || slot.Item == null || !slot.Item.IsPlaceable) {
				continue;
			}
			// Start from first available build item when click is inside BuildUI.
			DragItemId = slot.Item.Id;
			DraggedWorldObjectId = string.Empty;
			if(!BuildInventory.RemoveItem(BuildInventory.GetRow(i), BuildInventory.GetColumn(i), 1)) {
				return;
			}
			float initialRotationY = Player.GlobalRotation.Y;
			if(!PlacementManager.BeginExternalPlacementPreview(DragItemId, initialRotationY)) {
				BuildInventory.AddItem(new ItemSlot(slot.Item, 1), BuildInventory.GetRow(i), BuildInventory.GetColumn(i));
				DragItemId = string.Empty;
				return;
			}
			IsDragging = true;
			if(!PlacementUI.BeginBuildDragPreview(DragItemId)) {
				CancelDraggingToBuildInventory();
			}
			return;
		}
	}

	private void FinalizeLeftMouseRelease(Vector2 mousePosition) {
		if(!IsDragging) {
			return;
		}
		FinalizeDrag(mousePosition);
	}

	private void FinalizeDrag(Vector2 mousePosition) {
		bool overBuildUI = BuildUI.GetGlobalRect().HasPoint(mousePosition);
		bool placed = false;
		if(!overBuildUI && PlacementManager.TryGetExternalPlacementPose(out Vector3 position, out Vector3 rotation, out bool isValid) && isValid) {
			placed = WorldObjectManager.CreateWorldObject(DragItemId, position, rotation);
		}
		if(!placed && BuildInventory != null && !string.IsNullOrWhiteSpace(DragItemId)) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(DragItemId);
			BuildInventory.AddItem(new ItemSlot(item, 1));
		}
		StopDragging();
	}

	private void CancelDraggingToBuildInventory() {
		if(!IsDragging) {
			return;
		}
		if(BuildInventory != null && !string.IsNullOrWhiteSpace(DragItemId)) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(DragItemId);
			BuildInventory.AddItem(new ItemSlot(item, 1));
		}
		StopDragging();
	}

	private void StopDragging() {
		IsDragging = false;
		DragItemId = string.Empty;
		DraggedWorldObjectId = string.Empty;
		PlacementManager.EndExternalPlacementPreview();
		PlacementUI.EndPreview();
	}
}
