namespace Tests;

using Components;
using NUnit.Framework;

public sealed class CombatTests {
	[SetUp]
	public void SetUp() { }
	private sealed class Attacker : IOffense {
		public Offense Offense { get; } = new Offense(10);
	}

	private sealed class Defender : IHealth {
		public Health Health { get; } = new Health(50);
	}

	[Test]
	public void Attack_ReducesHealth() {
		Attacker attacker = new();
		Defender defender = new();

		attacker.Attack(defender);

		Assert.AreEqual(40, defender.Health.Current);
	}
}
