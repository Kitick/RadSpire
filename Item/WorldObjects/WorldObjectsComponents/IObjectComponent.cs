namespace Objects {
    using System;
    using Godot;
    using Services;
    using ItemSystem;

    public interface IObjectComponent {
        Object ComponentOwner { get; init; } 
    }
}