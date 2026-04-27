namespace Components;

public interface IRadiation { Radiation Radiation { get; } }

public sealed class Radiation : Component<RadiationData> {
	public float Level {
		get => Data.Level;
		set => SetData(Data with { Level = System.Math.Clamp(value, 0f, 1f) });
	}

	public float RatePerSecond {
		get => Data.RatePerSecond;
		set => SetData(Data with { RatePerSecond = value });
	}

	public void Accumulate(float dt) => Level += RatePerSecond * dt;

	public Radiation(float secondsToFatalDose) : base(new RadiationData { Level = 0f, RatePerSecond = 1f / secondsToFatalDose }) { }
	public Radiation(RadiationData data) : base(data) { }
}

public readonly record struct RadiationData : Services.ISaveData {
	public float Level { get; init; }
	public float RatePerSecond { get; init; }
}
