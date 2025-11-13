using Godot;

namespace SaveSystem {
	public interface ISaveData;

	public interface ISaveable<T> where T : ISaveData {
		T Serialize();
		void Deserialize(in T data);
	}

	public readonly record struct PlayerData : ISaveData {
		public CharacterData Character { get; init; }
		public float DefaultSprintMultiplier { get; init; }
		public float DefaultCrouchMultiplier { get; init; }
		public float DefaultFriction { get; init; }
		public float PlayerMaxHealth { get; init; }
		public bool IsCrouching { get; init; }
		public bool IsSprinting { get; init; }
	}

	public readonly record struct CameraPivotData : ISaveData {
		public int TiltIndex { get; init; }
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }
	}

	public readonly record struct CameraRigData : ISaveData {
		public Vector3 Position { get; init; }
		public Vector3 CenterOffset { get; init; }
	}

	public readonly record struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraPivotData CameraPivot { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}