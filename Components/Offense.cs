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

	public Offense(OffenseData data) : base(data) { }
	public Offense(int phys, int mag) : base(new OffenseData { PhysicalDamage = phys, MagicDamage = mag }) { }
}

public readonly record struct OffenseData : Services.ISaveData {
	public int PhysicalDamage { get; init; }
	public int MagicDamage { get; init; }
}
