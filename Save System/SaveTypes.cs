using Godot;

namespace SaveSystem {
	public interface ISaveData;

	public interface ISaveable<T> where T : ISaveData {
		T Serialize();
		void Deserialize(in T data);
	}

	public readonly record struct PlayerData : ISaveData {
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }
		public Vector3 Velocity { get; init; }

		public float Health { get; init; }
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

	public enum FullscreenMode { Windowed, Borderless, Fullscreen }

	public readonly record struct GameSettings : ISaveData {
		public int ResolutionWidth { get; init; }
		public int ResolutionHeight { get; init; }
		public FullscreenMode Fullscreen { get; init; }

		public float MusicVolume { get; init; }
		public float SFXVolume { get; init; }
	}
}