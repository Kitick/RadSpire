using Godot;
using System;
using SaveSystem;

public partial class NPC : Character, ISaveable<NPCData> {

	// ISaveable implementation
	public NPCData Serialize() {
		return new NPCData {

		};
	}

	public void Deserialize(in NPCData data) {

	}
}