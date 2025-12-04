using System;
using System.Collections.Generic;
using Godot;
using InputSystem;

public sealed partial class Hotbar : Control, IInventoryUI {
	public static readonly bool Debug = false;

	public int SelectedSlot {
		get;
		set {
			value += HotbarSlots.Count;
			value %= HotbarSlots.Count;
			field = value;
			SelectSlot(HotbarSlots[value]);
		}
	}

	private readonly List<Panel> HotbarSlots = [];

	private static readonly Color NormalColor = Colors.White;
	private static readonly Color SelectColor = new Color(1f, 1f, 0.3f);
	private static readonly Vector2 SelectedScale = new Vector2(1.05f, 1.05f);

	private const string HOTBAR = "Background/GridBackground/HotbarSlots";

	private event Action? OnExit;

	private Player Player = null!;
	public Inventory Inventory { get; set; } = null!;
	private List<InvSlotUI> HotbarSlotUIs = new List<InvSlotUI>();
	private int NumHotbarSlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;
	public event Action<string, int>? OnSlotClicked;
	
	public override void _EnterTree() {
		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
		SetUpInventoryUI();

		UpdateInventoryUI();
	}

	private void GetPlayer() {
		Player = GetParent<HUD>().Player;
		Inventory = Player.Hotbar;
		if(Player == null) {
			GD.PrintErr("Hotbar could not find Player node in parent HUD.");
			return;
		}
		GD.Print("Hotbar successfully found Player node in parent HUD.");

		Inventory.OnInventoryChanged += UpdateInventoryUI;
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	public void SetUpInventoryUI() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		Inventory = Player.Hotbar;
		GridContainer = GetNode<Control>("Background/GridBackground/HotbarSlots");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		NumHotbarSlots = Inventory.MaxSlotsRows * Inventory.MaxSlotsColumns;
		for(int i = 0; i < NumHotbarSlots; i++) {
			InvSlotUI slotInstance = InvSlotTemplate.Instantiate<InvSlotUI>();
			slotInstance.SlotIndex = i;
			slotInstance.OnSlotClicked += HandleOnSlotClicked;
			HotbarSlotUIs.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
	}

	public void HandleOnSlotClicked(int slotIndex) {
		OnSlotClicked?.Invoke(Inventory.Name, slotIndex);
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
			GD.Print($"Hotbar: Selecting slot {HotbarSlots.IndexOf(slot)}");
			GD.Print($"Hotbar: Selected item: {GetSelectedItem().Name}");
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

	public void UpdateInventoryUI(){
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		Inventory = Player.Hotbar;
		for(int i = 0; i < NumHotbarSlots; i++) {
			HotbarSlotUIs[i].UpdateSlotUI(Inventory.ItemSlots[i]);
		}
	}
}
