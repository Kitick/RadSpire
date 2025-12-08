using Godot;
using Services;
using ItemSystem;

namespace Components {
	public partial class Crafting : Resource, IItemComponent, ISaveable<CraftingData> {
		public Inventory CraftingRecipe {
			get;
			set;
		}

		public Crafting() {
			CraftingRecipe = new Inventory(3, 3);
		}

		public CraftingData Serialize() => new CraftingData {
			CraftingRecipe = CraftingRecipe.Serialize(),
		};

		public void Deserialize(in CraftingData data) {
			CraftingRecipe = new Inventory();
			CraftingRecipe.Deserialize(data.CraftingRecipe);
		}
	}

	public readonly struct CraftingData : ISaveData {
		public InventoryData CraftingRecipe { get; init; }
	}
}