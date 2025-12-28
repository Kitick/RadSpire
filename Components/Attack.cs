using System;
using Services;

namespace Components {
	public interface IOffense { Offense Offense { get; } }

	public sealed class Offense : ISaveable<OffenseData> {
		public int PhysicalDamage;
		public int MagicDamage;

		public Offense(int phys, int mag) {
			PhysicalDamage = phys;
			MagicDamage = mag;
		}

		public OffenseData Export() => new OffenseData {
			PhysicalDamage = PhysicalDamage,
			MagicDamage = MagicDamage,
		};

		public void Import(OffenseData data) {
			PhysicalDamage = data.PhysicalDamage;
			MagicDamage = data.MagicDamage;
		}
	}

	public readonly record struct OffenseData : ISaveData {
		public int PhysicalDamage { get; init; }
		public int MagicDamage { get; init; }
	}
}