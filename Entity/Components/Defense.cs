namespace Components;

public interface IDefense { Defense Defense { get; } }

public sealed class Defense : Component<DefenseData> {
	public int Armor {
		get => Data.Armor;
		set => SetData(Data with { Armor = value });
	}

	public Defense(DefenseData data) : base(data) { }

	public Defense(int armor) : base(new DefenseData { Armor = armor }) { }
}

public readonly record struct DefenseData : Services.ISaveData {
	public int Armor { get; init; }
}
