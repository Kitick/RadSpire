using System;
using Godot;
using SaveSystem;

public abstract partial class Character : CharacterBody3D, ISaveable<CharacterData> {
	public abstract string CharacterName { get; protected set; }

	public abstract HealthComponent Health { get; }

	public override void _Ready() {
		Health.CurrentHealth = Health.MaxHealth;
	}

	// ISaveable implementation
	public CharacterData Serialize() {
		return new CharacterData {
			CharacterName = CharacterName,
			Position = GlobalPosition,
			Rotation = GlobalRotation
		};
	}

	public void Deserialize(in CharacterData data) {
		CharacterName = data.CharacterName;
		GlobalPosition = data.Position;
		GlobalRotation = data.Rotation;
	}
}