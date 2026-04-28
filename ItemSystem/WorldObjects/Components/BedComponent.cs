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

public interface IBedComponent { BedComponent BedComponent { get; set; } }

public sealed class BedComponent : IObjectComponent, IInteract {
	private static readonly LogService Log = new(nameof(BedComponent), enabled: true);
	public Object ComponentOwner { get; init; } = null!;
    public int RestoreAmount { get; set; }
    public Vector3 Location { get; set; }
    public bool IsSleeping = false;

	public BedComponent(int RestoreAmount, Vector3 location, Object owner) {
        this.RestoreAmount = RestoreAmount;
		this.Location = location;
		ComponentOwner = owner;
	}

    public bool Interact<TEntity>(TEntity interactor) {
        if(interactor is not Player player) {
            return false;
        }
        if(IsSleeping) {
            player.EndSleep();
            IsSleeping = false;
            return true;
        }
        player.Sleep(RestoreAmount, Location);
        IsSleeping = true;
        return true;
    }
}