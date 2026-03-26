namespace Components;

using System;
using Character;
using Godot;
using ItemSystem;
using Services;
using UI;

public static class ItemInteractions {
	public static bool EquipItem<TEntity, TItem>(this TEntity user, TItem item)
	where TEntity : CharacterBase
	where TItem : Item {
		return item.Equip(user);
	}

	public static bool UnequipItem<TEntity, TItem>(this TEntity user, TItem item)
	where TEntity : CharacterBase
	where TItem : Item {
		return item.Unequip(user);
	}

	public static bool UseItem<TEntity, TItem>(this TEntity user, TItem item)
	where TEntity : CharacterBase
	where TItem : Item {
		return item.Use(user);
	}

	public static bool UseItemOnTarget<TEntity, TItem, TTarget>(this TEntity user, TItem item, TTarget target)
	where TEntity : CharacterBase
	where TItem : Item
	where TTarget : CharacterBase {
		return item.UseOnTarget(user, target);
	}
}

public partial class UseItem : Node {
	private static readonly LogService Log = new(nameof(UseItem), enabled: true);
	public Player? User;
	public Item? ItemToUse;
	private Action? UnsubscribeUse;
	public Hotbar UserHotbar = null!;

	public override void _Ready() {
		SetInputCallbacks();
	}

	void SetInputCallbacks() {
		UnsubscribeUse = ActionEvent.Consume.WhenPressed(() => {
			Log.Info("UseItem action pressed.");
			if(User == null || UserHotbar == null) {
				Log.Info("User or UserHotbar is null");
				return;
			}
			else {
				Log.Info("Attempting to use item.");
				ItemToUse = UserHotbar.GetSelectedItem();
				if(ItemToUse == null) {
					Log.Info("No item selected in hotbar.");
					return;
				}
				bool success = User.UseItem(ItemToUse);
				if(success) {
					Log.Info("Item used successfully.");
					if(ItemToUse.IsConsumable) {
						Log.Info("Item is consumable, consuming from inventory.");
						User.InventoryManager.ConsumeSelectedHotbar(UserHotbar, 1);
					}
				}
				else {
					Log.Info("Failed to use item.");
				}
			}
		});
	}

	public override void _ExitTree() {
		UnsubscribeUse?.Invoke();
	}
}

public partial class EquipItem : Node {
	private static readonly LogService Log = new(nameof(EquipItem), enabled: true);
	public Player? User;
	public Item? ItemToUse;
	public Hotbar UserHotbar = null!;
	private bool IsInitalized = false;

	public void Initalize(Player user, Hotbar hotbar) {
		User = user;
		UserHotbar = hotbar;
		hotbar.OnSlotSelected += OnHotbarSlotSelected;
		hotbar.OnSlotDeselected += OnHotbarSlotDeselected;
		IsInitalized = true;
	}

	public override void _Ready() {

	}

	public void OnHotbarSlotSelected(ItemSlot selectedSlot) {
		if(!IsInitalized || User == null || UserHotbar == null) {
			return;
		}
		Item? selectedItem = selectedSlot.Item;
		if(selectedItem == null) {
			Log.Info("No item selected in hotbar.");
			return;
		}
		User!.EquipItem(selectedItem);
	}

	public void OnHotbarSlotDeselected(ItemSlot deselectedSlot) {
		if(!IsInitalized || User == null || UserHotbar == null) {
			return;
		}
		Item? deselectedItem = deselectedSlot.Item;
		if(deselectedItem == null) {
			Log.Info("No item deselected in hotbar.");
			return;
		}
		User!.UnequipItem(deselectedItem);
	}

	public override void _ExitTree() {
		if(IsInitalized && UserHotbar != null) {
			UserHotbar.OnSlotSelected -= OnHotbarSlotSelected;
			UserHotbar.OnSlotDeselected -= OnHotbarSlotDeselected;
		}
	}
}
