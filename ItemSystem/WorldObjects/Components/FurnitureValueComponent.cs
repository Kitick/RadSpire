namespace ItemSystem.WorldObjects;

using System;
using Godot;
using InventorySystem;
using ItemSystem;
using ItemSystem.WorldObjects.House;
using Root;
using Services;
using GameWorld;
using Character;
using System.ComponentModel;

public interface IFurnitureValueComponent { FurnitureValueComponent FurnitureValue { get; set; } }

public sealed class FurnitureValueComponent : IObjectComponent {
	private static readonly LogService Log = new(nameof(FurnitureValueComponent), enabled: true);
	public Object ComponentOwner { get; init; } = null!;
    public int Value { get; set; }

	public FurnitureValueComponent(int value, Object owner) {
        Value = value;
        ComponentOwner = owner;
	}

    public int GetValue() => Value;
}
