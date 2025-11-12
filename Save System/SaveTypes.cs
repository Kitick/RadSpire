using Godot;

namespace SaveSystem {
	public interface ISaveData;

	public interface ISaveable<T> where T : ISaveData {
		T Serialize();
		void Deserialize(in T data);
	}

	public readonly record struct PlayerData : ISaveData {
		public float DefaultSprintMultiplier { get; init; }
		public float DefaultCrouchMultiplier { get; init; }
		public float DefaultFriction { get; init; }
		public float PlayerMaxHealth { get; init; }
		public Vector3 HorizontalInput { get; init; }
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

	public readonly record struct CharacterData : ISaveData {
		public float CurrentHealth { get; init; }
		public string CharacterName { get; init; }
		public float MaxHealth { get; init; }
		public bool IsInvincible { get; init; }
		public bool IsAlive { get; init; }
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }
		public Vector3 Velocity { get; init; }
		public float Speed { get; init; }
		public float SpeedModifier { get; init; }
		public float RotationSpeed { get; init; }
		public float FallAcceleration { get; init; }
		public float JumpForce { get; init; }
		public string Type { get; init; }
		public bool UseGravity { get; init; }
		public Vector3 MoveDirection { get; init; }
		public Vector3 FaceDirection { get; init; }
		public bool InAir { get; init; }
		public bool Moving { get; init; }
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