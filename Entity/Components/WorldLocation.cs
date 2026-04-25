namespace Components;

using Godot;

public interface IWorldLocation { WorldLocation WorldLocation { get; } }

public sealed class WorldLocation : Component<WorldLocationData> {
	public Vector3 Position {
		get => Data.Position;
		set => SetData(Data with { Position = value });
	}

	public Vector3 Rotation {
		get => Data.Rotation;
		set => SetData(Data with { Rotation = value });
	}

	public WorldLocation(Vector3 position, Vector3 rotation) : base(new WorldLocationData { Position = position, Rotation = rotation }) { }
}

public readonly record struct WorldLocationData : Services.ISaveData {
	public Vector3 Position { get; init; }
	public Vector3 Rotation { get; init; }
}
