using System.Collections.Generic;
using Components;
using Godot;
using Services;

namespace ItemSystem {
	public partial class Item : ISaveable<ItemData> {
		//Basic properties of all items

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

		public ItemData Serialize() => new ItemData {

		};

		public void Deserialize(in ItemData data) {

		}
	}

	public readonly record struct ItemData : ISaveData {

	}
}