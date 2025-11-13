using Godot;
using System;
using SaveSystem;

public partial class CollectableNPC : NPC, ISaveable<NPCData> {

	// ISaveable implementation
	public CollectableNPCData Serialize() {
		return new CollectableNPCData {

		};
	}

	public void Deserialize(in CollectableNPC data) {

	}
}