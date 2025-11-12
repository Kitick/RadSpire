using Godot;
using System;
using SaveSystem;

public partial class Zombie : Character, ISaveable<ZombieData> {

	// ISaveable implementation
	public ZombieData Serialize() {
		return new ZombieData {

		};
	}

	public void Deserialize(in ZombieData data) {

	}
}