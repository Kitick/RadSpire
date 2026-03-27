namespace InventorySystem;

using System;
using Character;
using Godot;

public interface IInventoryUI {
	Inventory Inventory { get; set; }
	Rect2 GetGlobalRect();
	void Initialize(Inventory inventory, Player player);
	void SetUpInventoryUI();
	public event Action<string, int, MouseButton>? OnSlotPressed;
	public event Action<string, int, MouseButton>? OnSlotReleased;
	public event Action<string, int>? OnSlotHovered;
	void UpdateInventoryUI();
}
