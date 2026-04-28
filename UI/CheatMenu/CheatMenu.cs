namespace UI.CheatMenu;

using System;
using System.Collections.Generic;
using System.Linq;
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
	[Export] public Button SuperStrengthButton = null!;

	[Export] public Button ClearInventoryButton = null!;

	[Export] public Button SuperSpeedButton = null!;

	[ExportCategory("Give Item")]
	[Export] public OptionButton GiveItemDropdown = null!;
	[Export] public SpinBox GiveItemQuantity = null!;
	[Export] public Button GiveItemButton = null!;

	protected override Control? DefaultFocus => CloseButton;

	private const int HealthStep = 10;
	private const float RadStep = 0.1f;
	private const float SuperSpeedMultiplier = 5f;
	private const int SuperStrengthMultiplier = 10;

	private float OriginalSpeed;
	private bool IsSuperSpeedActive;
	private int OriginalDamage;
	private bool IsSuperStrengthActive;

	private Player? Player;
	private bool Bound;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.Always;
	}

	public void OpenMenu(Player player) {
		Player = player;
		Visible = true;
		OnOpen();
		PopulateItemDropdown();
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
		SuperStrengthButton.Pressed += CheatToggleSuperStrength;
		ClearInventoryButton.Pressed += CheatClearInventory;
		SuperSpeedButton.Pressed += CheatToggleSuperSpeed;
		GiveItemButton.Pressed += CheatGiveItemFromInput;
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
		SuperStrengthButton.Pressed -= CheatToggleSuperStrength;
		ClearInventoryButton.Pressed -= CheatClearInventory;
		SuperSpeedButton.Pressed -= CheatToggleSuperSpeed;
		GiveItemButton.Pressed -= CheatGiveItemFromInput;
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

	private void CheatToggleSuperStrength() {
		if(Player == null) { return; }
		if(IsSuperStrengthActive) {
			Player.Offense.Damage = OriginalDamage;
			IsSuperStrengthActive = false;
		}
		else {
			OriginalDamage = Player.Offense.Damage;
			Player.Offense.Damage = OriginalDamage * SuperStrengthMultiplier;
			IsSuperStrengthActive = true;
		}
	}

	private void CheatClearInventory() {
		if(Player == null) { return; }
		foreach(ItemSlot slot in Player.Inventory.ItemSlots) { slot.ClearSlot(); }
		foreach(ItemSlot slot in Player.Hotbar.ItemSlots) { slot.ClearSlot(); }
		Player.Inventory.NotifyChanged();
		Player.Hotbar.NotifyChanged();
	}

	private void PopulateItemDropdown() {
		GiveItemDropdown.Clear();
		List<string> ids = [];
		foreach(ItemDefinition def in DatabaseManager.Instance.ItemsDefinitions.Values) {
			bool hasDoor = false;
			bool hasStructure = false;
			foreach(ItemComponentDefinition comp in def.ComponentsResources) {
				if(comp is DoorDefinition) { hasDoor = true; }
				if(comp is StructureDefinition) { hasStructure = true; }
			}
			if(hasDoor && !hasStructure) { continue; }
			ids.Add(def.Id);
		}
		ids.Sort(StringComparer.OrdinalIgnoreCase);
		foreach(string id in ids) {
			GiveItemDropdown.AddItem(id);
		}
	}

	private void CheatGiveItemFromInput() {
		string id = GiveItemDropdown.GetItemText(GiveItemDropdown.Selected);
		if(string.IsNullOrEmpty(id)) { return; }
		int quantity = (int) GiveItemQuantity.Value;
		GiveItem(new StringName(id), quantity);
	}

	private void CheatToggleSuperSpeed() {
		if(Player == null) { return; }
		if(IsSuperSpeedActive) {
			Player.Movement.BaseSpeed = OriginalSpeed;
			IsSuperSpeedActive = false;
		}
		else {
			OriginalSpeed = Player.Movement.BaseSpeed;
			Player.Movement.BaseSpeed = OriginalSpeed * SuperSpeedMultiplier;
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
