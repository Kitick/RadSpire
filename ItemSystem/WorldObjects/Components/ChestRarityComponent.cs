namespace ItemSystem.WorldObjects;

using System;
using System.Collections.Generic;
using InventorySystem;
using ItemSystem;
using Root;
using Services;

public interface IChestRarityComponent { ChestRarityComponent ChestRarityComponent { get; set; } }

public sealed class ChestRarityComponent : IObjectComponent {
	private static readonly LogService Log = new(nameof(ChestRarityComponent), enabled: true);
	public Object ComponentOwner { get; init; }
	public Rarity RarityLevel { get; private set; }
	public ChestRarityComponent(Rarity rarity, Object owner) {
		RarityLevel = rarity;
		ComponentOwner = owner;
	}
}

public readonly record struct ItemProbabilities(string ItemId, int ChanceWeight);

public record RarityDefinition(
	Rarity RarityLevel,
	int UpperBound,
	int LowerBound,
	ItemProbabilities[] PossibleContents
);

public static class RarityDefinitions {
	private static readonly LogService Log = new(nameof(RarityDefinitions), enabled: true);

	public static readonly RarityDefinition Common = new(
		RarityLevel: Rarity.Common,
		UpperBound: 12,
		LowerBound: 6,
		PossibleContents: [
			new(ItemID.AppleGreen, 4),
				new(ItemID.BananaYellow, 4),
				new(ItemID.BerryGreen, 3),
				new(ItemID.BlueberryGreen, 3),
				new(ItemID.CherryGreen, 2),
				new(ItemID.StrawberryGreen, 2),
				new(ItemID.Wood, 4),
				new(ItemID.Stick, 4),
				new(ItemID.StonePiece, 3),
				new(ItemID.Stone, 2)
		]
	);

	public static readonly RarityDefinition Rare = new(
		RarityLevel: Rarity.Rare,
		UpperBound: 18,
		LowerBound: 10,
		PossibleContents: [
			new(ItemID.AppleRed, 4),
				new(ItemID.AppleYellow, 4),
				new(ItemID.BananaGreen, 3),
				new(ItemID.BerryRed, 3),
				new(ItemID.CherryRed, 3),
				new(ItemID.BerryBlack, 2),
				new(ItemID.BlueberryBlue, 2),
				new(ItemID.StrawberryRed, 2),
				new(ItemID.BananaYellow, 1),
				new(ItemID.Barrel, 1),
				new(ItemID.IronOre, 3),
				new(ItemID.IronChunk, 2),
				new(ItemID.IronBar, 1),
				new(ItemID.GoldOre, 2),
				new(ItemID.GoldChunk, 1),
				new(ItemID.SwordWood, 1),
				new(ItemID.ShieldWood, 1)
		]
	);

	public static readonly RarityDefinition Precious = new(
		RarityLevel: Rarity.Precious,
		UpperBound: 24,
		LowerBound: 14,
		PossibleContents: [
			new(ItemID.CoconutGreenOpen, 4),
				new(ItemID.CoconutBrownOpen, 4),
				new(ItemID.StrawberryRed, 3),
				new(ItemID.BlueberryBlue, 3),
				new(ItemID.BerryBlack, 2),
				new(ItemID.CherryRed, 2),
				new(ItemID.Bonfire, 2),
				new(ItemID.Barrel, 1),
				new(ItemID.IronBar, 3),
				new(ItemID.GoldBar, 2),
				new(ItemID.GoldChunk, 2),
				new(ItemID.SwordIron, 2),
				new(ItemID.SwordGold, 1),
				new(ItemID.ShieldIron, 2),
				new(ItemID.HeadpieceIron, 1),
				new(ItemID.ChestpieceIron, 1),
				new(ItemID.PantpieceIron, 1)
		]
	);

	private static readonly Dictionary<Rarity, RarityDefinition> ByRarity = new() {
			{ Rarity.Common, Common },
			{ Rarity.Rare, Rare },
			{ Rarity.Precious, Precious },
		};

	public static RarityDefinition Get(Rarity rarity) {
		if(ByRarity.TryGetValue(rarity, out RarityDefinition? definition)) {
			return definition;
		}
		Log.Warn($"Unknown rarity '{rarity}', falling back to Common.");
		return Common;
	}
}

public static class ChestRarityComponentExtensions {
	private static readonly LogService Log = new(nameof(ChestRarityComponentExtensions), enabled: true);
	private static readonly Random Random = new Random();

	public static void TryFillInventoryFromRarity(this Object obj, string spawnPointName) {
		if(!obj.ComponentDictionary.Has<InventoryComponent>()) {
			return;
		}
		if(!obj.ComponentDictionary.Has<ChestRarityComponent>()) {
			return;
		}

		Inventory inventory = obj.ComponentDictionary.Get<InventoryComponent>().Inventory;
		ChestRarityComponent chestRarity = obj.ComponentDictionary.Get<ChestRarityComponent>();
		RarityDefinition rarityDefinition = RarityDefinitions.Get(chestRarity.RarityLevel);

		int lowerBound = Math.Max(0, Math.Min(rarityDefinition.LowerBound, rarityDefinition.UpperBound));
		int upperBound = Math.Max(lowerBound, Math.Max(rarityDefinition.LowerBound, rarityDefinition.UpperBound));
		int numberOfItemsInChest = Random.Next(lowerBound, upperBound + 1);

		for(int i = 0; i < numberOfItemsInChest; i++) {
			ItemProbabilities? selected = PickWeightedItem(rarityDefinition.PossibleContents);
			if(selected == null) {
				break;
			}

			if(DatabaseManager.Instance.GetItemDefinitionById(selected.Value.ItemId) == null) {
				Log.Warn($"Rarity loot on spawn point '{spawnPointName}' references unknown item '{selected.Value.ItemId}'. Skipping.");
				continue;
			}

			Item itemInstance = DatabaseManager.Instance.CreateItemInstanceById(selected.Value.ItemId);
			ItemSlot remaining = inventory.AddItem(new ItemSlot(itemInstance, 1));
			if(!remaining.IsEmpty()) {
				// Inventory is full
				break;
			}
		}
	}

	private static ItemProbabilities? PickWeightedItem(ItemProbabilities[] possibleContents) {
		if(possibleContents == null || possibleContents.Length == 0) {
			return null;
		}
		int totalWeight = 0;
		for(int i = 0; i < possibleContents.Length; i++) {
			totalWeight += Math.Max(0, possibleContents[i].ChanceWeight);
		}
		if(totalWeight <= 0) {
			return null;
		}
		int roll = Random.Next(0, totalWeight);
		int cumulative = 0;
		for(int i = 0; i < possibleContents.Length; i++) {
			int weight = Math.Max(0, possibleContents[i].ChanceWeight);
			cumulative += weight;
			if(roll < cumulative) {
				return possibleContents[i];
			}
		}
		return possibleContents[possibleContents.Length - 1];
	}
}

