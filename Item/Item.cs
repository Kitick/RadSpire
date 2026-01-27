namespace ItemSystem {
	using System.Collections.Generic;
	using Components;
	using Godot;
	using Services;

	public partial class Item : ISaveable<ItemData> {
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

		public bool SameItem(Item other) {
			if(other == null) {
				return false;
			}
			return Id == other.Id;
		}

		public Item() {

		}
		
		public Item(Item other) {
			Id = other.Id;
			Name = other.Name;
			Description = other.Description;
			MaxStackSize = other.MaxStackSize;
			IconTexture = other.IconTexture;
		}

		public ItemData Export() {
			return new ItemData {
				Id = Id
			};
		}

		public void Import(ItemData data) {
			Item item = ItemDataBaseManager.Instance.CreateBaseItemInstanceById(data.Id);
			Id = item.Id;
			Name = item.Name;
			Description = item.Description;
			MaxStackSize = item.MaxStackSize;
			IconTexture = item.IconTexture;	
		}
	}

	public readonly record struct ItemData : ISaveData {
		public string Id { get; init; }
	}
}
