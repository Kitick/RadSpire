namespace ItemSystem {
    using Godot;
    using System;
    using Services;
    using System.Collections.Generic;
    using GodotResourceGroups;
    using Components;

    public partial class ItemDataBaseManager : Node {
		private static readonly LogService Log = new(nameof(Item3DIconPickup), enabled: true);

		public ResourceGroup AllItemDefinitions { get; set; } = ResourceGroup.Of("res://Item/ItemDataBase/AllItemDefinitions.tres");
		public Dictionary<string, ItemDefinition> ItemsDefinitions { get; private set; } = new();

        public override void _Ready() {
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
            if(itemDef.ComponentsResources.Count == 0) {
                Log.Info($"Base ItemDefinition with ID {id} has no components.");
            }
            else {
                foreach(ItemComponentDefinition compDef in itemDef.ComponentsResources) {
                    if(compDef == null) {
                        Log.Error($"Failed to create component from definition in ItemDefinition ID {id}.");
                        continue;
                    }
                    switch(compDef) {
                        case HealItemDefinition healDef:
                            HealItem healComp = new HealItem(healDef.HealAmount);
                            item = new ItemHeal(item, healComp);
                            break;
                        case DurabilityDefinition durabilityDef:
                            Durability durabilityComp = new Durability(durabilityDef.MaxDurability);
                            item = new ItemDurability(item, durabilityComp);
                            break;
                        default:
                            Log.Error($"Unknown component type in ItemDefinition ID {id}.");
                            break;
                    }
                }
            }

            item.Id = itemDef.Id;
            item.Name = itemDef.Name;
            item.Description = itemDef.Description;
            item.MaxStackSize = itemDef.MaxStackSize;
            item.IconTexture = itemDef.IconTexture;
            
            return item;
        }
	}
}
