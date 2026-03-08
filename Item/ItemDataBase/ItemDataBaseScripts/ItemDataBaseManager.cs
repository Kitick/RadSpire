namespace ItemSystem {
	using Godot;
	using System;
	using Services;
	using System.Collections.Generic;
	using GodotResourceGroups;
	using Components;
	using Objects;

	public partial class ItemDataBaseManager : Node {
		private static readonly LogService Log = new(nameof(ItemDataBaseManager), enabled: true);
		public static ItemDataBaseManager Instance { get; private set; } = null!;

		public ResourceGroup AllItemDefinitions { get; set; } = ResourceGroup.Of("res://Item/ItemDataBase/AllItemDefinitions.tres");
		public Dictionary<string, ItemDefinition> ItemsDefinitions { get; private set; } = new();

		public override void _Ready() {
			Instance = this;
			List<ItemDefinition> resources = new List<ItemDefinition>();
			AllItemDefinitions.LoadAllInto(resources);
			foreach(ItemDefinition itemDef in resources) {
				if(ItemsDefinitions.ContainsKey(itemDef.Id)) {
					Log.Error($"Duplicate ItemDefinition ID detected: {itemDef.Id}. Skipping duplicate.");
					continue;
				}
				else {
					ItemsDefinitions.Add(itemDef.Id, itemDef);
				}
			}
			Log.Info($"Loaded {ItemsDefinitions.Count} ItemDefinitions into ItemDataBaseManager.");
		}

		public ItemDefinition? GetItemDefinitionById(string id) {
			if(ItemsDefinitions.TryGetValue(id, out ItemDefinition? itemDef)) {
				return itemDef;
			}
			Log.Info($"ItemDefinition with ID {id} not found.");
			return null;
		}

		public Item CreateItemInstanceById(string id) {
			Item item = new Item();
			ItemDefinition? itemDef = GetItemDefinitionById(id);
			if(itemDef == null) {
				Log.Error($"Cannot create Item instance. ItemDefinition with ID {id} not found.");
				return null!;
			}

			item.Id = itemDef.Id;
			item.Name = itemDef.Name;
			item.Description = itemDef.Description;
			item.MaxStackSize = itemDef.MaxStackSize;
			item.IsConsumable = itemDef.IsConsumable;
			item.IsPlaceable = itemDef.IsPlaceable;
			item.IconTexture = itemDef.IconTexture;

			BuildComponents(item, itemDef);

			return item;
		}

		public void BuildComponents(Item item, ItemDefinition itemDef) {
			item.ClearComponents();
			if(itemDef.ComponentsResources.Count == 0) {
				return;
			}
			foreach(var resource in itemDef.ComponentsResources) {
				if(resource is ItemComponentDefinition comp) {
					if(comp is HealItemDefinition healDef) {
						HealItem healComp = new HealItem(healDef.HealAmount);
						item.AddComponent(healComp);
					}
					else if(comp is DurabilityDefinition durabilityDef) {
						Durability durabilityComp = new Durability(durabilityDef.MaxDurability);
						item.AddComponent(durabilityComp);
					}
				}
			}
		}
		
		public void BuildObjectComponents(Objects.Object obj, ItemDefinition itemDef) {
			obj.ComponentDictionary.Clear();
			if(itemDef.ComponentsResources.Count == 0) {
				return;
			}
			foreach(var resource in itemDef.ComponentsResources) {
				if(resource is ItemComponentDefinition comp) {
					if(comp is InventoryDefinition invDef) {
						InventoryComponent objComp = new InventoryComponent(invDef.Rows, invDef.Columns, obj);
						obj.ComponentDictionary.Add(objComp);
					}
				}
			}
		 }
	}
}
