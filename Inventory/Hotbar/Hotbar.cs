namespace InventorySystem.Interface;

using System;
using System.Collections.Generic;
using Character;
using InventorySystem;
using Godot;
using ItemSystem;
using Services;

public sealed partial class Hotbar : Control, IInventoryUI {
	private static readonly LogService Log = new(nameof(Hotbar), enabled: true);

	public static readonly bool Debug = false;

	public int SelectedSlot {
		get;
		set {
			if(HotbarSlots.Count == 0) {
				field = 0;
				return;
			}
			ItemSlot? previousItemSlot = GetItemSlotAtSlotIndex(field);
			int idx = value;
			idx += HotbarSlots.Count;
			idx %= HotbarSlots.Count;
			if(idx == field) {
				return;
			}
			field = idx;
			if(previousItemSlot != null && !previousItemSlot.IsEmpty()) {
				OnSlotDeselected?.Invoke(previousItemSlot);
			}
			SelectSlot(HotbarSlots[idx]);
		}
	}

	private readonly List<Panel> HotbarSlots = new List<Panel>();

	private static readonly Color NormalColor = Colors.White;
	private static readonly Color SelectColor = new Color(1f, 1f, 0.3f);
	private static readonly Vector2 SelectedScale = new Vector2(1.05f, 1.05f);

	private const string HOTBAR = "Background/GridBackground/HotbarSlots";

	private event Action? OnExit;

	private Player Player = null!;
	private bool IsReady = false;
	private bool IsInitialized = false;
	private bool IsRegisteredToInventory = false;
	public Inventory Inventory { get; set; } = null!;
	private List<InvSlotUI> HotbarSlotUIs = new List<InvSlotUI>();
	private int NumHotbarSlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;
	public event Action<string, int, MouseButton>? OnSlotPressed;
	public event Action<string, int, MouseButton>? OnSlotReleased;
	public event Action<string, int>? OnSlotHovered;
	public event Action<ItemSlot>? OnSlotSelected;
	public event Action<ItemSlot>? OnSlotDeselected;

	public Hotbar() {
	}

	public Hotbar(Inventory inventory, Player player) {
		Initialize(inventory, player);
	}

	public void Initialize(Inventory inventory, Player player) {
		if(inventory == null) {
			Log.Error("Inventory is null.");
			return;
		}
		if(player == null) {
			Log.Error("Player is null.");
			return;
		}
		Inventory = inventory;
		Player = player;
		IsInitialized = true;
		if(IsReady) {
			SetUpInventoryUI();
		}
	}

	public override void _EnterTree() {
		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
		IsReady = true;
		if(IsInitialized) {
			SetUpInventoryUI();
		}
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		if(IsRegisteredToInventory && Inventory != null) {
			Inventory.OnInventoryChanged -= UpdateInventoryUI;
			IsRegisteredToInventory = false;
		}
		if(IsInitialized && Player != null && Player.InventoryManager != null && Inventory != null) {
			Player.InventoryManager.UnregisterInventory(Inventory.Name);
		}
	}

	public void SetUpInventoryUI() {
		if(!IsInitialized || !IsReady) {
			return;
		}
		if(Player == null || Inventory == null) {
			Log.Error("Missing required Player or Inventory.");
			return;
		}
		if(!IsRegisteredToInventory) {
			Player.InventoryManager.RegisterInventory(Inventory, this);
			Inventory.OnInventoryChanged += UpdateInventoryUI;
			IsRegisteredToInventory = true;
		}
		if(HotbarSlotUIs.Count > 0) {
			UpdateInventoryUI();
			return;
		}
		GridContainer = GetNode<Control>("Background/GridBackground/HotbarSlots");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		NumHotbarSlots = Inventory.MaxRows * Inventory.MaxColumns;
		for(int i = 0; i < NumHotbarSlots; i++) {
			InvSlotUI slotInstance = InvSlotTemplate.Instantiate<InvSlotUI>();
			slotInstance.SlotIndex = i;
			slotInstance.OnSlotPressed += HandleOnSlotPressed;
			slotInstance.OnSlotReleased += HandleOnSlotReleased;
			slotInstance.OnSlotHovered += HandleOnSlotHovered;
			HotbarSlotUIs.Add(slotInstance);
			HotbarSlots.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
		if(NumHotbarSlots > 0) {
			SelectSlot(HotbarSlots[0]);
		}
	}

	public void HandleOnSlotPressed(int slotIndex, MouseButton button) {
		Log.Info($"Hotbar: Slot {slotIndex} pressed.");
		OnSlotPressed?.Invoke(Inventory.Name, slotIndex, button);
	}

	public void HandleOnSlotReleased(int slotIndex, MouseButton button) {
		Log.Info($"Hotbar: Slot {slotIndex} released.");
		OnSlotReleased?.Invoke(Inventory.Name, slotIndex, button);
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.Hotbar1.WhenPressed(() => SelectedSlot = 0);
		OnExit += ActionEvent.Hotbar2.WhenPressed(() => SelectedSlot = 1);
		OnExit += ActionEvent.Hotbar3.WhenPressed(() => SelectedSlot = 2);
		OnExit += ActionEvent.Hotbar4.WhenPressed(() => SelectedSlot = 3);
		OnExit += ActionEvent.Hotbar5.WhenPressed(() => SelectedSlot = 4);

		OnExit += ActionEvent.HotbarNext.WhenPressed(() => SelectedSlot++);
		OnExit += ActionEvent.HotbarPrev.WhenPressed(() => SelectedSlot--);
	}

	private void SelectSlot(Panel slot) {
		if(Debug) {
			Log.Info($"Hotbar: Selecting slot {HotbarSlots.IndexOf(slot)}");
			Log.Info($"Hotbar: Selected item: {GetSelectedItem()!.Name}");
		}
		ItemSlot selectedItemSlot = GetSelectedItemSlot();
		if(!selectedItemSlot.IsEmpty()) {
			OnSlotSelected?.Invoke(selectedItemSlot);
		}

		foreach(var other in HotbarSlots) {
			bool selected = other == slot;

			other.SelfModulate = selected ? SelectColor : NormalColor;
			other.Scale = selected ? SelectedScale : Vector2.One;
		}
	}

	public ItemSlot GetSelectedItemSlot() {
		int index = SelectedSlot;
		return Inventory.GetItemSlot(Inventory.GetRow(index), Inventory.GetColumn(index));
	}

	public Item? GetSelectedItem() {
		int index = SelectedSlot;
		if(Inventory.IsEmptySlot(Inventory.GetRow(index), Inventory.GetColumn(index))) { return null; }

		return Inventory.GetItem(Inventory.GetRow(index), Inventory.GetColumn(index));
	}

	private ItemSlot? GetItemSlotAtSlotIndex(int index) {
		if(index < 0 || index >= HotbarSlots.Count) {
			return null;
		}
		int row = Inventory.GetRow(index);
		int column = Inventory.GetColumn(index);
		return Inventory.GetItemSlot(row, column);
	}

	public void UpdateInventoryUI() {
		if(!IsInitialized || Inventory == null) {
			return;
		}
		int slotsToUpdate = Math.Min(NumHotbarSlots, Inventory.ItemSlots.Length);
		for(int i = 0; i < slotsToUpdate; i++) {
			HotbarSlotUIs[i].UpdateSlotUI(Inventory.ItemSlots[i]);
		}
	}

	public void HandleOnSlotHovered(int slotIndex) {
		Log.Info($"Hotbar: Slot {slotIndex} hovered.");
		OnSlotHovered?.Invoke(Inventory.Name, slotIndex);
	}
}
