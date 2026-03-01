namespace ItemSystem {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Components;
	using Godot;
	using Services;

	public partial class Item : ISaveable<ItemData> {
		private static readonly LogService Log = new(nameof(Item), enabled: true);
		[Export]
		public string Id {
			get;
			set {
				if(string.IsNullOrWhiteSpace(value)) {
					return;
				}
				field = value;
			}
		} = "ItemDefault";

		[Export]
		public string Name {
			get;
			set {
				if(string.IsNullOrWhiteSpace(value)) {
					return;
				}
				field = value;
			}
		} = "Default Item";

		[Export]
		public string Description {
			get;
			set {
				if(string.IsNullOrWhiteSpace(value)) {
					return;
				}
				field = value;
			}
		} = "Default Description";

		[Export]
		public int MaxStackSize {
			get; set {
				if(value < 1) {
					field = 1;
					return;
				}
				field = value;
			}
		} = 1;

		public bool IsStackable => MaxStackSize > 1;

		[Export] public bool IsConsumable { get; set; } = false;

		[Export] public Texture2D IconTexture { get; set; } = null!;
		public ComponentDictionary<IItemComponent> ComponentDictionary { get; } = new();

		public IEnumerable<IItemComponent> GetComponentsOrdered() {
			return ComponentDictionary.All.Values.OrderBy(component => component.priority);
		}

		public void ClearComponents() {
			ComponentDictionary.Clear();
		}

		private void CopyComponentsFrom(Item other) {
			ClearComponents();
			foreach(IItemComponent component in other.ComponentDictionary.All.Values) {
				this.AddComponent(component);
			}
		}

		public Item() {
			ClearComponents();
		}

		public Item(Item other) {
			Id = other.Id;
			Name = other.Name;
			Description = other.Description;
			MaxStackSize = other.MaxStackSize;
			IsConsumable = other.IsConsumable;
			IconTexture = other.IconTexture;
			CopyComponentsFrom(other);
		}

		public ItemData Export() {
			DurabilityData? DurabilityData = null;
			// Additional component data can be declared here
			foreach(IItemComponent component in ComponentDictionary.All.Values) {
				if(component is Durability durabilityComp) {
					DurabilityData = durabilityComp.Export();
				}
				// Additional components that has run-time data can be exported here
			}
			return new ItemData {
				Id = Id,
				DurabilityData = DurabilityData,
				// Additional component data can be added here
			};
		}

		public void Import(ItemData data) {
			Item item = ItemDataBaseManager.Instance.CreateItemInstanceById(data.Id);
			Id = item.Id;
			Name = item.Name;
			Description = item.Description;
			MaxStackSize = item.MaxStackSize;
			IsConsumable = item.IsConsumable;
			IconTexture = item.IconTexture;
			CopyComponentsFrom(item);
			if(data.DurabilityData != null) {
				if(ComponentDictionary.Has<Durability>()) {
					this.RemoveComponent(ComponentDictionary.Get<Durability>());
				}
				Durability durabilityComp = new Durability(1);
				durabilityComp.Import(data.DurabilityData.Value);
				this.AddComponent(durabilityComp);
			}
			// Additional components with run time data can be imported here
		}
	}

	public static class ItemExtensions {
		public static readonly LogService Log = new(nameof(ItemExtensions), enabled: true);

		public static bool Use<TEntity>(this Item item, TEntity user) {
			bool sucess = false;
			foreach(IItemComponent component in item.GetComponentsOrdered()) {
				if(component is IItemUseable useable) {
					sucess |= useable.Use(user);
				}
			}
			return sucess;
		}

		public static bool Equip<TEntity>(this Item item, TEntity user) {
			bool sucess = false;
			foreach(IItemComponent component in item.GetComponentsOrdered()) {
				if(component is IItemEquipable equipable) {
					sucess |= equipable.Equip(user);
				}
			}
			return sucess;
		}

		public static bool Unequip<TEntity>(this Item item, TEntity user) {
			bool sucess = false;
			foreach(IItemComponent component in item.GetComponentsOrdered()) {
				if(component is IItemEquipable equipable) {
					sucess |= equipable.Unequip(user);
				}
			}
			return sucess;
		}

		public static bool UseOnTarget<TEntity, TTarget>(this Item item, TEntity user, TTarget target) {
			bool sucess = false;
			foreach(IItemComponent component in item.GetComponentsOrdered()) {
				if(component is IItemUseableOnTarget useableOnTarget) {
					sucess |= useableOnTarget.UseOnTarget(user, target);
				}
			}
			return sucess;
		}

		public static bool SameItem(this Item item, Item other) {
			if(other == null) {
				return false;
			}
			return item.Id == other.Id;
		}

		public static bool HasComponent(this Item item, Type componentType) {
			return item.ComponentDictionary.All.ContainsKey(componentType);
		}

		public static bool AddComponent(this Item item, IItemComponent component) {
			if(component == null) {
				return false;
			}
			Type componentType = component.GetType();
			bool success = false;
			success = item.ComponentDictionary.Add(componentType, component);
			return success;
		}

		public static bool RemoveComponent(this Item item, IItemComponent component) {
			if(component == null) {
				return false;
			}
			Type componentType = component.GetType();
			return item.ComponentDictionary.Remove(componentType);
		}
	}

	public readonly record struct ItemData : ISaveData {
		public string Id { get; init; }
		public DurabilityData? DurabilityData { get; init; }
		// Additional component data can be added here
	}
}
