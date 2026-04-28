namespace UI.CheatMenu;

using System;
using Character;
using Components;
using Godot;
using InventorySystem;
using ItemSystem;
using Root;
using UI;

public sealed partial class CheatMenu : BaseUIControl {
	[ExportCategory("Buttons")]
	[Export] public Button CloseButton = null!;

	[Export] public Button HealFullButton = null!;
	[Export] public Button HealButton = null!;
	[Export] public Button HurtButton = null!;
	[Export] public Button KillButton = null!;

	[Export] public Button ClearRadButton = null!;
	[Export] public Button AddRadButton = null!;
	[Export] public Button RemoveRadButton = null!;
	[Export] public Button MaxRadButton = null!;

	[Export] public Button ClearInventoryButton = null!;

	[Export] public Button SuperSpeedButton = null!;

	[ExportCategory("Give Item")]
	[Export] public LineEdit GiveItemInput = null!;
	[Export] public SpinBox GiveItemQuantity = null!;
	[Export] public Button GiveItemButton = null!;

	protected override Control? DefaultFocus => CloseButton;

	private const int HealthStep = 10;
	private const float RadStep = 0.1f;
	private const float SuperSpeed = 20.0f;

	private float OriginalSpeed;
	private bool IsSuperSpeedActive;

	private Player? Player;
	private bool Bound;
	private LineEdit.TextSubmittedEventHandler? GiveItemSubmitHandler;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.Always;
	}

	public void OpenMenu(Player player) {
		Player = player;
		Visible = true;
		OnOpen();
		BindButtons();
	}

	public void CloseMenu() {
		Visible = false;
		UnbindButtons();
		Player = null;
	}

	private void BindButtons() {
		if(Bound) { return; }
		Bound = true;
		CloseButton.Pressed += CloseMenu;
		HealFullButton.Pressed += CheatHealFull;
		HealButton.Pressed += CheatHeal;
		HurtButton.Pressed += CheatHurt;
		KillButton.Pressed += CheatKill;
		ClearRadButton.Pressed += CheatClearRad;
		AddRadButton.Pressed += CheatAddRad;
		RemoveRadButton.Pressed += CheatRemoveRad;
		MaxRadButton.Pressed += CheatMaxRad;
		ClearInventoryButton.Pressed += CheatClearInventory;
		SuperSpeedButton.Pressed += CheatToggleSuperSpeed;
		GiveItemButton.Pressed += CheatGiveItemFromInput;
		GiveItemSubmitHandler = _ => CheatGiveItemFromInput();
		GiveItemInput.TextSubmitted += GiveItemSubmitHandler;
	}

	private void UnbindButtons() {
		if(!Bound) { return; }
		Bound = false;
		CloseButton.Pressed -= CloseMenu;
		HealFullButton.Pressed -= CheatHealFull;
		HealButton.Pressed -= CheatHeal;
		HurtButton.Pressed -= CheatHurt;
		KillButton.Pressed -= CheatKill;
		ClearRadButton.Pressed -= CheatClearRad;
		AddRadButton.Pressed -= CheatAddRad;
		RemoveRadButton.Pressed -= CheatRemoveRad;
		MaxRadButton.Pressed -= CheatMaxRad;
		ClearInventoryButton.Pressed -= CheatClearInventory;
		SuperSpeedButton.Pressed -= CheatToggleSuperSpeed;
		GiveItemButton.Pressed -= CheatGiveItemFromInput;
		if(GiveItemSubmitHandler != null) { GiveItemInput.TextSubmitted -= GiveItemSubmitHandler; }
	}

	private void CheatHealFull() {
		if(Player == null) { return; }
		Player.Health.Current = Player.Health.Max;
	}

	private void CheatHeal() => Player?.Heal(HealthStep);
	private void CheatHurt() => Player?.Hurt(HealthStep);
	private void CheatKill() => Player?.Hurt();

	private void CheatClearRad() {
		if(Player == null) { return; }
		Player.Radiation.Level = 0f;
	}

	private void CheatAddRad() {
		if(Player == null) { return; }
		Player.Radiation.Level = Math.Clamp(Player.Radiation.Level + RadStep, 0f, 1f);
	}

	private void CheatRemoveRad() {
		if(Player == null) { return; }
		Player.Radiation.Level = Math.Clamp(Player.Radiation.Level - RadStep, 0f, 1f);
	}

	private void CheatMaxRad() {
		if(Player == null) { return; }
		Player.Radiation.Level = 1f;
	}

	private void CheatClearInventory() {
		if(Player == null) { return; }
		foreach(ItemSlot slot in Player.Inventory.ItemSlots) { slot.ClearSlot(); }
		foreach(ItemSlot slot in Player.Hotbar.ItemSlots) { slot.ClearSlot(); }
		Player.Inventory.NotifyChanged();
		Player.Hotbar.NotifyChanged();
	}

	private void CheatGiveItemFromInput() {
		string id = GiveItemInput.Text.Trim();
		if(string.IsNullOrEmpty(id)) { return; }
		int quantity = (int) GiveItemQuantity.Value;
		GiveItem(new StringName(id), quantity);
		GiveItemInput.Clear();
	}

	private void CheatToggleSuperSpeed() {
		if(Player == null) { return; }
		if(IsSuperSpeedActive) {
			Player.Movement.BaseSpeed = OriginalSpeed;
			IsSuperSpeedActive = false;
		}
		else {
			OriginalSpeed = Player.Movement.BaseSpeed;
			Player.Movement.BaseSpeed = SuperSpeed;
			IsSuperSpeedActive = true;
		}
	}

	private void GiveItem(StringName id, int quantity = 1) {
		if(Player == null) { return; }
		for(int i = 0; i < quantity; i++) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(id);
			if(item == null) { continue; }
			Player.Inventory.AddItem(item);
		}
	}
}
