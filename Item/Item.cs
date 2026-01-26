using System.Collections.Generic;
using Components;
using Godot;
using Services;

namespace ItemSystem {
	public partial class Item : ISaveable<ItemData>, IDurable, IHealItem {
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

		//Components
		public HealItem Heal { get; set; } = null!;
		public Durability Durability { get; set; } = null!;


		public bool SameItem(Item other) {
			if(other == null) {
				return false;
			}
			return Id == other.Id;
		}

		public ItemData Export() {
			if(Durability != null) {
				return new ItemData {
					Id = Id,
					DurabilityData = Durability.Export()
				};
			}
			return new ItemData {
				Id = Id,
				DurabilityData = new DurabilityData { Current = 0, Max = 0 }
			};
		}

		public void Import(ItemData data) {
			//Load Item from ItemDefinition Database
			//Setup Run-time Components Data
		}
	}

	public readonly record struct ItemData : ISaveData {
		public string Id { get; init; }
		public DurabilityData DurabilityData { get; init; }
	}
}
