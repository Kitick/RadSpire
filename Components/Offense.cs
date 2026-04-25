namespace Components;

public interface IOffense { Offense Offense { get; } }

public sealed class Offense : Component<OffenseData> {
	public int Damage {
		get => Data.Damage;
		set => SetData(Data with { Damage = value });
	}

	public Offense(OffenseData data) : base(data) { }
	public Offense(int damage) : base(new OffenseData { Damage = damage }) { }
}

public readonly record struct OffenseData : Services.ISaveData {
	public int Damage { get; init; }
}
