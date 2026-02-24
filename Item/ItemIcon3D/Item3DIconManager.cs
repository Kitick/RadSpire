namespace ItemSystem {
	using Godot;
	using Services;
	using Objects;
	using System.Collections.Generic;

	public partial class Item3DIconManager : Node, ISaveable<Item3DIconManagerData> {
		private static readonly LogService Log = new(nameof(Item3DIconManager), enabled: true);

		public List<Item3DIcon> ActiveItem3DIcons { get; private set; } = new List<Item3DIcon>();

		private void AddItem3DIcon(Item3DIcon icon) {
			ActiveItem3DIcons.Add(icon);
		}

		private void RemoveItem3DIcon(Item3DIcon icon) {
			ActiveItem3DIcons.Remove(icon);

		}

		public void SpawnItem(string itemID, Vector3 position, float scaleFactor = 1.0f) {
			Item? item = ItemDataBaseManager.Instance.CreateItemInstanceById(itemID);
			if(item == null) {
				Log.Error($"Failed to load item with ID: {itemID}");
				return;
			}
			Item3DIcon item3DIcon = new Item3DIcon();
			item3DIcon.Item = item;
			item3DIcon.Name = item.Name + "3DIcon";
			AddChild(item3DIcon);
			item3DIcon.ScaleFactor = scaleFactor;
			item3DIcon.SpawnItem3D(position);
			AddItem3DIcon(item3DIcon);
		}
		
		public void DespawnItem(Item3DIcon icon) {
			RemoveItem3DIcon(icon);
			icon.QueueFree();
		}

		private void ClearActiveIcons() {
			for(int i = 0; i < ActiveItem3DIcons.Count; i++) {
				var icon = ActiveItem3DIcons[i];
				if(IsInstanceValid(icon)) {
					icon.QueueFree();
				}
			}

			foreach(var child in GetChildren()) {
				if(child is Item3DIcon icon && IsInstanceValid(icon)) {
					icon.QueueFree();
				}
			}

			ActiveItem3DIcons.Clear();
		}

		public Item3DIconManagerData Export() => new Item3DIconManagerData {
			Item3DIconsData = ActiveItem3DIcons
				.FindAll(icon => IsInstanceValid(icon))
				.ConvertAll(icon => icon.Export())
		};

		public void Import(Item3DIconManagerData data) {
			ClearActiveIcons();
			foreach (Item3DIconData Item3DIconData in data.Item3DIconsData) {
				var icon = new Item3DIcon();
				AddChild(icon);
				icon.Import(Item3DIconData);
				AddItem3DIcon(icon);
				
			}
		}
	}

	public readonly record struct Item3DIconManagerData : ISaveData {
		public List<Item3DIconData> Item3DIconsData { get; init; }
	}
}
