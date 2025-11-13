using Godot;
using System;
using SaveSystem;

public partial class Monster : Character, ISaveable<MonsterData> {

	// ISaveable implementation
	public MonsterData Serialize() {
		return new MonsterData {

		};
	}

	public void Deserialize(in MonsterData data) {

	}
}