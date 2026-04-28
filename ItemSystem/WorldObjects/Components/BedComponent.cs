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
        // Convert the stored local bed location to a global position using the owner's world location
        Vector3 globalLocation = Location;
        if(ComponentOwner?.WorldLocation != null) {
            // Apply owner's rotation then translation
            Basis b = Basis.FromEuler(ComponentOwner.WorldLocation.Rotation);
            globalLocation = ComponentOwner.WorldLocation.Position + (b * Location);
        }

        if(IsSleeping) {
            player.EndSleep();
            IsSleeping = false;
            return true;
        }
        // Compute desired global rotation so player's forward (GlobalBasis.Z) aligns with the bed's local +Z
        Vector3 desiredRotation = Vector3.Zero;
        if(ComponentOwner?.WorldLocation != null) {
            Basis ownerBasis = Basis.FromEuler(ComponentOwner.WorldLocation.Rotation);
            Vector3 bedLocalPositiveZ = new Vector3(0, 0, 1);
            Vector3 bedZWorld = (ownerBasis * bedLocalPositiveZ).Normalized();
            float yaw = Mathf.Atan2(bedZWorld.X, bedZWorld.Z);
            desiredRotation = new Vector3(0, yaw, 0);
        }

        player.Sleep(RestoreAmount, globalLocation, desiredRotation);
        IsSleeping = true;
        return true;
    }
}