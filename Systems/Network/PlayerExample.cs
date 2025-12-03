using System;

namespace Network {
	public sealed class PlayerStats : INetworkable<PlayerStatData> {
		public event Action? OnStateChanged;

		public int Health {
			get;
			set {
				if(field != value) {
					field = value;
					OnStateChanged?.Invoke();
				}
			}
		} = 100;

		public int Armor {
			get;
			set {
				if(field != value) {
					field = value;
					OnStateChanged?.Invoke();
				}
			}
		} = 50;

		public string Name {
			get;
			set {
				if(field != value) {
					field = value;
					OnStateChanged?.Invoke();
				}
			}
		} = "";

		public PlayerStatData Serialize() => new PlayerStatData {
			Health = Health,
			Armor = Armor,
			Experience = Name,
		};

		public void Deserialize(in PlayerStatData data) {
			Health = data.Health;
			Armor = data.Armor;
			Name = data.Experience;
		}
	}

	public readonly record struct PlayerStatData : INetworkData {
		public int Health { get; init; }
		public int Armor { get; init; }
		public string Experience { get; init; }
	}
}
