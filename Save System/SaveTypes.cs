using Godot;

namespace SaveSystem {
	record PlayerData {
		public required Vector3 Position { get; init; }
		public required Vector3 Rotation { get; init; }
	}
}