namespace ItemSystem.WorldObjects;

    using System;
    using Godot;
    using Services;
    using ItemSystem;

    public interface IObjectComponent {
        Object ComponentOwner { get; init; }
    }

    public interface IInteract {
        public bool Interact<TEntity>(TEntity interactor);
    }
