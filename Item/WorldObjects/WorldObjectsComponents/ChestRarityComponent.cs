namespace Components {
    using System;
    using Core;
    using ItemSystem;
    using Objects;
    using Services;
    using System.Collections.Generic;

    public interface IChestRarityComponent { ChestRarityComponent ChestRarityComponent { get; set; } }

    public sealed class ChestRarityComponent : IObjectComponent {
        private static readonly LogService Log = new(nameof(ChestRarityComponent), enabled: true);
        public Objects.Object ComponentOwner { get; init; }
        public Rarity RarityLevel { get; private set; }
        public ChestRarityComponent(Rarity rarity, Objects.Object owner) {
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
                new(ItemID.StrawberryGreen, 2)
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
                new(ItemID.Barrel, 1)
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
                new(ItemID.Barrel, 1)
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
}