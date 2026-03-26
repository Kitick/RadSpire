namespace Components;

	using System;
	using Character;
	using Godot;
	using ItemSystem;
	using Objects;
	using Root;
	using Services;
	using UI;

	public interface IInventoryComponent { InventoryComponent InventoryComponent { get; set; } }

	public sealed class InventoryComponent : ISaveable<InventoryComponentData>, IObjectComponent, IInteract {
		private static readonly LogService Log = new(nameof(InventoryComponent), enabled: true);
		public Objects.Object ComponentOwner { get; init; }
		public Inventory Inventory { get; private set; }

		public InventoryComponent(int rows, int columns, Objects.Object owner) {
			Inventory = new Inventory(rows, columns);
			Inventory.Name = "Chest";
			ComponentOwner = owner;
		}

		public bool Interact<TEntity>(TEntity interactor) {
			if(interactor is Player player) {
				Node? gameManager = player.GetParent();
				HUD? hud = gameManager?.GetNodeOrNull<HUD>("HUD");
				if(hud == null) {
					Log.Error("Interact failed: HUD not found.");
					return false;
				}

				hud.OpenChest(Inventory, player);
				return true;
			}
			else {
				return false;
			}
		}

		public InventoryComponentData Export() => new InventoryComponentData {
			InventoryData = Inventory.Export(),
		};

		public void Import(InventoryComponentData data) {
			if(Inventory == null) {
				Inventory = new Inventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
			}
			if(Inventory.MaxRows != data.InventoryData.MaxSlotsRows || Inventory.MaxColumns != data.InventoryData.MaxSlotsColumns) {
				Inventory = new Inventory(data.InventoryData.MaxSlotsRows, data.InventoryData.MaxSlotsColumns);
			}
			Inventory.Import(data.InventoryData);
			Inventory.Name = "Chest";
		}
	}

	public readonly record struct InventoryComponentData : ISaveData {
		public InventoryData InventoryData { get; init; }
	}

