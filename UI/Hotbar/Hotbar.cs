using System;
using System.Collections.Generic;
using Godot;
using InputSystem;

public sealed partial class Hotbar : Control {
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
	private Inventory PlayerHotbar = null!;
	private List<InvSlotUI> HotbarSlotUIs = new List<InvSlotUI>();
	private int NumHotbarSlots = 0;
	private PackedScene? InvSlotTemplate = null!;
	private Control? GridContainer = null!;
	
	public override void _EnterTree() {
		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
		SetUpHotbarUI();

		UpdateHotbarUI();
	}

	private void GetPlayer() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("Hotbar could not find Player node in parent HUD.");
			return;
		}
		GD.Print("Hotbar successfully found Player node in parent HUD.");

		Player.Inventory.OnInventoryChanged += UpdateHotbarUI;
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	private void SetUpHotbarUI() {
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerHotbar = Player.Inventory;
		GridContainer = GetNode<Control>("Background/GridBackground/HotbarSlots");
		if(InvSlotTemplate == null) {
			InvSlotTemplate = GD.Load<PackedScene>("res://UI/Inventory/InvSlotUITemplate.tscn");
		}
		NumHotbarSlots = PlayerHotbar.MaxSlotsRows * PlayerHotbar.MaxSlotsColumns;
		for(int i = 0; i < NumHotbarSlots; i++) {
			InvSlotUI slotInstance = InvSlotTemplate.Instantiate<InvSlotUI>();
			slotInstance.SlotIndex = i;
			slotInstance.OnSlotClicked += HandleOnSlotClicked;
			HotbarSlotUIs.Add(slotInstance);
			GridContainer.AddChild(slotInstance);
		}
	}

	public void HandleOnSlotClicked(int slotIndex) {
		
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
		return PlayerHotbar.GetItemSlot(PlayerHotbar.GetRow(index), PlayerHotbar.GetColumn(index));
	}

	public Item? GetSelectedItem() {
		int index = SelectedSlot;
		if(PlayerHotbar.IsEmptySlot(PlayerHotbar.GetRow(index), PlayerHotbar.GetColumn(index))) { return null; }

		return PlayerHotbar.GetItem(PlayerHotbar.GetRow(index), PlayerHotbar.GetColumn(index));
	}

	public void UpdateHotbarUI(){
		Player = GetParent<HUD>().Player;
		if(Player == null) {
			GD.PrintErr("InventoryUI SetUpInventoryUI: Player is null.");
			return;
		}
		PlayerHotbar = Player.Hotbar;
		for(int i = 0; i < NumHotbarSlots; i++) {
			HotbarSlotUIs[i].UpdateSlotUI(PlayerHotbar.ItemSlots[i]);
		}
	}
}
