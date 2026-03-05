namespace Radspire.Tests {
	using Components;
	using NUnit.Framework;

	public class CombatTests {

		[SetUp]
		public void SetUp() { }
		private sealed class Attacker : IOffense {
			public Offense Offense { get; } = new Offense(default);

			public Attacker() {
				Offense.PhysicalDamage = 10;
			}
		}

		private sealed class Defender : IHealth {
			public Health Health { get; } = new Health(50);
		}

		[Test]
		public void Attack_ReducesHealth() {
			var attacker = new Attacker();
			var defender = new Defender();

			attacker.Attack(defender);

			Assert.AreEqual(40, defender.Health.Current);
		}
	}
}