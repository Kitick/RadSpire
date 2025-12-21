using System;
using Services;

namespace Components {
	public interface IAttack { Attack Attack { get; } }

	public sealed class Attack : ISaveable<AttackData> {
		public readonly int Damage;

		public Attack(int damage) {
			Damage = damage;
		}

		public AttackData Export() => new AttackData {

		};

		public void Import(AttackData data) {

		}
	}

	public static class AttackExtensions {

	}

	public readonly record struct AttackData : ISaveData {

	}
}