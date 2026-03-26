namespace Components;

public interface IDefense { Defense Defense { get; } }

public sealed class Defense : Component<DefenseData> {
	public int PhysicalDefense {
		get => Data.PhysicalDefense;
		set => SetData(Data with { PhysicalDefense = value });
	}

	public int MagicDefense {
		get => Data.MagicDefense;
		set => SetData(Data with { MagicDefense = value });
	}

	public Defense(DefenseData data) : base(data) { }

	public Defense(int phys, int mag) : base(new DefenseData { PhysicalDefense = phys, MagicDefense = mag }) { }
}

public readonly record struct DefenseData : Services.ISaveData {
	public int PhysicalDefense { get; init; }
	public int MagicDefense { get; init; }
}
