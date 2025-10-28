using Godot;

namespace SaveSystem {
	interface ISaveData;

	record PlayerData : ISaveData {
		public required Vector3 Position;
		public required Vector3 Rotation;
	}
}