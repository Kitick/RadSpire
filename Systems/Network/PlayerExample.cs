using System;
using Core;
using Godot;

namespace Network {
	/// <summary>
	/// Player controller - implements INetworkable for syncing position.
	/// This class doesn't know anything about the network model, only Serialize/Deserialize.
	/// </summary>
	public sealed class PlayerController : INetworkable<PlayerPositionData> {
		private readonly ColorRect Visual;
		private const float Speed = 200f;

		public event Action? OnStateChanged;

		public PlayerController(ColorRect visual) {
			Visual = visual;
		}

		public void Update(float delta) {
			Vector2 input = Input.GetVector(Actions.MoveLeft, Actions.MoveRight, Actions.MoveForward, Actions.MoveBack);

			if(input != Vector2.Zero) {
				var velocity = input.Normalized() * Speed;
				Visual.Position += velocity * delta;
				OnStateChanged?.Invoke();
			}
		}

		public PlayerPositionData Serialize() => new PlayerPositionData {
			X = Visual.Position.X,
			Y = Visual.Position.Y
		};

		public void Deserialize(in PlayerPositionData data) {
			Visual.Position = new Vector2(data.X, data.Y);
		}
	}

	public readonly record struct PlayerPositionData : INetworkData {
		public float X { get; init; }
		public float Y { get; init; }
	}
}