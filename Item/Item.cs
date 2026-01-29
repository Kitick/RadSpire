namespace ItemSystem {
	using System.Collections.Generic;
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
		[Export] public Texture2D IconTexture { get; set; } = null!;

		[Export] public List<IItemComponent> Components { get; set; } = new();

		public bool SameItem(Item other) {
			if(other == null) {
				return false;
			}
			return Id == other.Id;
		}

		public Item() {
			Components = new List<IItemComponent>();
		}

		public Item(Item other) {
			Id = other.Id;
			Name = other.Name;
			Description = other.Description;
			MaxStackSize = other.MaxStackSize;
			IconTexture = other.IconTexture;

			Components = new List<IItemComponent>(other.Components);
		}
		
		public bool AddComponent(IItemComponent component) {
			if(component == null) {
				return false;
			}
			if(!(component is IItemComponent comp)) {
				return false;
			}
			foreach(IItemComponent c in Components) {
				if(component.GetType() == c.GetType()) {
					Log.Info($"Item already has a component of type {component.GetType().Name}.");
					return false;
				}
			}
			Components.Add(component);
			return true;
		}

		public bool RemoveComponent(IItemComponent component) {
			if(component == null) {
				return false;
			}
			foreach(IItemComponent comp in Components) {
				if(component.GetType() == comp.GetType()) {
					Components.Remove(component);
					return true;
				}
			}
			return false;
		}

		public ItemData Export() {
			return new ItemData {
				Id = Id
			};
		}

		public void Import(ItemData data) {
			Item item = ItemDataBaseManager.Instance.CreateItemInstanceById(data.Id);
			Id = item.Id;
			Name = item.Name;
			Description = item.Description;
			MaxStackSize = item.MaxStackSize;
			IconTexture = item.IconTexture;
			Components = new List<IItemComponent>(data.ComponentsData);
		}
	}

	public readonly record struct ItemData : ISaveData {
		public string Id { get; init; }
		public List<IItemComponent> ComponentsData { get; init; }
	}
}