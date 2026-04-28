namespace Components;

public interface IOffense { Offense Offense { get; } }

public sealed class Offense : Component<OffenseData> {
	public int Damage {
		get => Data.Damage;
		set => SetData(Data with { Damage = value });
	}

	public float CritChance {
		get => Data.CritChance;
		set => SetData(Data with { CritChance = value });
	}

	public float CritMultiplier {
		get => Data.CritMultiplier;
		set => SetData(Data with { CritMultiplier = value });
	}

	public float DamageVariance {
		get => Data.DamageVariance;
		set => SetData(Data with { DamageVariance = value });
	}

	public float ArmorPenetration {
		get => Data.ArmorPenetration;
		set => SetData(Data with { ArmorPenetration = value });
	}

	public Offense(OffenseData data) : base(data) { }
	public Offense(int damage) : base(new OffenseData {
		Damage = damage,
		CritChance = 0.05f,
		CritMultiplier = 1.2f,
		DamageVariance = 0.2f,
		ArmorPenetration = 0f,
	}) { }
}

public readonly record struct OffenseData : Services.ISaveData {
	public int Damage { get; init; }
	public float CritChance { get; init; }
	public float CritMultiplier { get; init; }
	public float DamageVariance { get; init; }
	public float ArmorPenetration { get; init; }
}
