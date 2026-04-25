namespace ItemSystem;

using System.Collections.Generic;
using Godot;
using GodotResourceGroups;
using ItemSystem.WorldObjects;
using Services;

public sealed partial class DatabaseManager : Node {
	private static readonly LogService Log = new(nameof(DatabaseManager), enabled: true);

	public static DatabaseManager Instance { get; private set; } = null!;

	public ResourceGroup AllItemDefinitions { get; set; } = ResourceGroup.Of("res://ItemSystem/Database/AllItemDefinitions.tres");

	public Dictionary<string, ItemDefinition> ItemsDefinitions { get; private set; } = [];

	public override void _Ready() {
		Instance = this;

		List<ItemDefinition> resources = [];
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
		item.Can_Object_Stack = itemDef.Can_Object_Stack;
		item.IsConsumable = itemDef.IsConsumable;
		item.IsPlaceable = itemDef.IsPlaceable;
		item.Pickupable = itemDef.Pickupable;
		item.IsWallObject = itemDef.IsWallObject;
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
				else if(comp is WeaponBaseDefinition weaponDef) {
					WeaponBase weaponComp = new WeaponBase(weaponDef.BaseAttack, weaponDef.AttackSpeed);
					item.AddComponent(weaponComp);
				}
			}
		}
	}

	public void BuildObjectComponents(Object obj, ItemDefinition itemDef) {
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
				else if(comp is ChestRarityDefinition chestRarityDef) {
					ChestRarityComponent rarityComp = new ChestRarityComponent(chestRarityDef.RarityLevel, obj);
					obj.ComponentDictionary.Add(rarityComp);
				}
				else if(comp is DoorDefinition doorDef) {
					DoorComponent doorComp = new DoorComponent(doorDef.BaseScene, doorDef.SpawnPositionMarker, obj);
					obj.ComponentDictionary.Add(doorComp);
				}
			}
		}
	}
}
