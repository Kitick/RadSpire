namespace ItemSystem.WorldObjects.House;

using System;
using Components;
using Godot;
using ItemSystem;
using Services;

public partial class Door : Area3D {
    [Export] public string TargetGameWorldId = null!;
    
}

