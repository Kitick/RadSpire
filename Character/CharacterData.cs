using Godot;

namespace SaveSystem {
	public readonly record struct CharacterData : ISaveData {
		public string CharacterName { get; init; }
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }
	}

	public readonly record struct MonsterData : ISaveData {

	}

	public readonly record struct ZombieData : ISaveData {

	}

	public readonly record struct NPCData : ISaveData {

	}

	public readonly record struct CollectableNPCData : ISaveData {

	}
}
