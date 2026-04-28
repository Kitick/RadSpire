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
	private ObjectHoverTargetingController HoverTargetingController = null!;
	private WorldObjectManager WorldObjectManager = null!;
	private GameManager GameManager = null!;
	private InventoryUI BuildUI = null!;

	private Inventory? BuildInventory;
	private bool IsDragging;
	private bool IsSubscribedToBuildUISlots;
	private string DragItemId = string.Empty;
	private string DraggedWorldObjectId = string.Empty;
	private ObjectData? DraggedWorldObjectData;

	public bool IsBuildModeActive { get; private set; }
	public bool IsDraggingFurniture => IsBuildModeActive && IsDragging;

	public void Initialize(
		Player player,
		InventoryManager inventoryManager,
		ObjectPlacementManager placementManager,
		ObjectPlacementUI placementUI,
		ObjectHoverTargetingController hoverTargetingController,
		WorldObjectManager worldObjectManager,
		GameManager gameManager,
		InventoryUI buildUI
	) {
		Player = player;
		InventoryManager = inventoryManager;
		PlacementManager = placementManager;
		PlacementUI = placementUI;
		HoverTargetingController = hoverTargetingController;
		WorldObjectManager = worldObjectManager;
		GameManager = gameManager;
		BuildUI = buildUI;
		SubscribeToBuildUISlotEvents();
	}

	public override void _ExitTree() {
		base._ExitTree();
		UnsubscribeFromBuildUISlotEvents();
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
				// Build UI slot presses are handled via slot events.
				if(BuildUI.GetGlobalRect().HasPoint(mouseButton.GlobalPosition)) {
					return;
				}
				StartDragFromHoveredWorldObject();
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
		BuildInventory = new Inventory(3, 5) { Name = BuildInventoryName };
		BuildUI.Initialize(BuildInventory, Player);
		BuildUI.SetLabelText("Build");
		GameManager.HUDRef?.OpenBuildUI();
		InventoryManager.MovePlaceableItems(Player.Hotbar, BuildInventory);
		InventoryManager.MovePlaceableItems(Player.Inventory, BuildInventory);
		IsBuildModeActive = true;
		GameManager.GameWorldManagerRef?.RequestStructureInfoRefresh();
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
		ObjectNode? hoveredObject = HoverTargetingController.HoveredObjectNode;
		if(hoveredObject?.Data == null) {
			return;
		}
		ItemDefinition? itemDef = DatabaseManager.Instance.GetItemDefinitionById(hoveredObject.Data.ItemId);
		if(itemDef?.IsPlaceable != true || itemDef.Pickupable != true) {
			return;
		}

		DragItemId = hoveredObject.Data.ItemId;
		DraggedWorldObjectId = hoveredObject.Data.Id;
		DraggedWorldObjectData = WorldObjectManager.GetWorldObject(DraggedWorldObjectId)?.Export();
		float initialRotationY = hoveredObject.GlobalRotation.Y;
		if(!PlacementManager.BeginExternalPlacementPreview(DragItemId, initialRotationY)) {
			DragItemId = string.Empty;
			DraggedWorldObjectId = string.Empty;
			DraggedWorldObjectData = null;
			return;
		}

		if(!WorldObjectManager.RemoveWorldObject(DraggedWorldObjectId)) {
			PlacementManager.EndExternalPlacementPreview();
			DragItemId = string.Empty;
			DraggedWorldObjectId = string.Empty;
			DraggedWorldObjectData = null;
			return;
		}
		GameManager.GameWorldManagerRef?.RequestStructureInfoRefresh();

		IsDragging = true;
		if(!PlacementUI.BeginBuildDragPreview(DragItemId)) {
			StopDragging();
			return;
		}
	}

	private void TryStartDragFromBuildInventorySlot(int slotIndex) {
		if(IsDragging || BuildInventory == null) {
			return;
		}
		if(slotIndex < 0 || slotIndex >= BuildInventory.ItemSlots.Length) {
			return;
		}
		ItemSlot slot = BuildInventory.ItemSlots[slotIndex];
		if(slot.IsEmpty() || slot.Item == null || !slot.Item.IsPlaceable || !slot.Item.Pickupable) {
			// Explicit no-op when clicked slot has no draggable furniture item.
			return;
		}
		DragItemId = slot.Item.Id;
		DraggedWorldObjectId = string.Empty;
		int row = BuildInventory.GetRow(slotIndex);
		int column = BuildInventory.GetColumn(slotIndex);
		if(!BuildInventory.RemoveItem(row, column, 1)) {
			return;
		}
		float initialRotationY = Player.GlobalRotation.Y;
		if(!PlacementManager.BeginExternalPlacementPreview(DragItemId, initialRotationY)) {
			BuildInventory.AddItem(new ItemSlot(slot.Item, 1), row, column);
			DragItemId = string.Empty;
			return;
		}
		IsDragging = true;
		if(!PlacementUI.BeginBuildDragPreview(DragItemId)) {
			CancelDraggingToBuildInventory();
		}
	}

	private void SubscribeToBuildUISlotEvents() {
		if(IsSubscribedToBuildUISlots) {
			return;
		}
		BuildUI.OnSlotPressed += HandleBuildUISlotPressed;
		IsSubscribedToBuildUISlots = true;
	}

	private void UnsubscribeFromBuildUISlotEvents() {
		if(!IsSubscribedToBuildUISlots) {
			return;
		}
		BuildUI.OnSlotPressed -= HandleBuildUISlotPressed;
		IsSubscribedToBuildUISlots = false;
	}

	private void HandleBuildUISlotPressed(string inventoryName, int slotIndex, MouseButton button) {
		if(!IsBuildModeActive || button != MouseButton.Left) {
			return;
		}
		if(inventoryName != BuildInventoryName) {
			return;
		}
		TryStartDragFromBuildInventorySlot(slotIndex);
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
			if(DraggedWorldObjectData.HasValue) {
				placed = WorldObjectManager.CreateWorldObject(DraggedWorldObjectData.Value, position, rotation);
			}
			else {
				placed = WorldObjectManager.CreateWorldObject(DragItemId, position, rotation);
			}
		}
		if(placed) {
			GameManager.GameWorldManagerRef?.RequestStructureInfoRefresh();
		}
		if(!placed && BuildInventory != null && !string.IsNullOrWhiteSpace(DragItemId)) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(DragItemId);
			BuildInventory.AddItem(new ItemSlot(item, 1));
			HandleCanceledDraggedObjectInventory();
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
		DraggedWorldObjectData = null;
		PlacementManager.EndExternalPlacementPreview();
		PlacementUI.EndPreview();
	}

	private void HandleCanceledDraggedObjectInventory() {
		if(!DraggedWorldObjectData.HasValue || !DraggedWorldObjectData.Value.InventoryComponentData.HasValue || BuildInventory == null) {
			return;
		}
		InventoryData cachedInventoryData = DraggedWorldObjectData.Value.InventoryComponentData.Value.InventoryData;
		Inventory cachedInventory = new Inventory(cachedInventoryData.MaxSlotsRows, cachedInventoryData.MaxSlotsColumns);
		cachedInventory.Import(cachedInventoryData);
		foreach(ItemSlot cachedSlot in cachedInventory.ItemSlots) {
			if(cachedSlot.IsEmpty() || cachedSlot.Item == null) {
				continue;
			}
			ItemSlot movingSlot = new ItemSlot(cachedSlot.Item, cachedSlot.Quantity);
			ItemSlot remain;
			if(cachedSlot.Item.IsPlaceable && cachedSlot.Item.Pickupable) {
				remain = BuildInventory.AddItem(movingSlot);
				if(!remain.IsEmpty()) {
					remain = InventoryManager.AddItemSlotToPlayerInventory(remain);
				}
			}
			else {
				remain = InventoryManager.AddItemSlotToPlayerInventory(movingSlot);
			}
			if(!remain.IsEmpty()) {
				InventoryManager.DropItemSlot(remain);
			}
		}
	}
}
