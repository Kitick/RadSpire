namespace Components;

public interface IOffense { Offense Offense { get; } }

public sealed class Offense : Component<OffenseData> {
	public int PhysicalDamage {
		get => Data.PhysicalDamage;
		set => SetData(Data with { PhysicalDamage = value });
	}

	public int MagicDamage {
		get => Data.MagicDamage;
		set => SetData(Data with { MagicDamage = value });
	}

	// 0..1 chance to crit
	public float CritChance {
		get => Data.CritChance;
		set => SetData(Data with { CritChance = value });
	}

	// Multiplier applied on crit (e.g. 1.5 = +50%)
	public float CritMultiplier {
		get => Data.CritMultiplier;
		set => SetData(Data with { CritMultiplier = value });
	}

	// 0..1 variance applied to final damage (e.g. 0.1 = +/-10%)
	public float DamageVariance {
		get => Data.DamageVariance;
		set => SetData(Data with { DamageVariance = value });
	}

	// 0..1 percent of defense ignored
	public float ArmorPenetrationPhysical {
		get => Data.ArmorPenetrationPhysical;
		set => SetData(Data with { ArmorPenetrationPhysical = value });
	}

	public float ArmorPenetrationMagic {
		get => Data.ArmorPenetrationMagic;
		set => SetData(Data with { ArmorPenetrationMagic = value });
	}

	public Offense(OffenseData data) : base(data) { }
	public Offense(int phys, int mag) : base(new OffenseData {
		PhysicalDamage = phys,
		MagicDamage = mag,
		CritChance = 0.05f,
		CritMultiplier = 1.2f,
		DamageVariance = 0f,
		ArmorPenetrationPhysical = 0f,
		ArmorPenetrationMagic = 0f,
	}) { }
}

public readonly record struct OffenseData : Services.ISaveData {
	public int PhysicalDamage { get; init; }
	public int MagicDamage { get; init; }
	public float CritChance { get; init; }
	public float CritMultiplier { get; init; }
	public float DamageVariance { get; init; }
	public float ArmorPenetrationPhysical { get; init; }
	public float ArmorPenetrationMagic { get; init; }
}
