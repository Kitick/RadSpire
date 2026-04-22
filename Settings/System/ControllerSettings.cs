namespace Settings;

using Services;

public static class ControllerSettings {
	public static readonly Setting<bool> Vibration = new(
		name: nameof(Vibration),
		getActual: () => default,
		setActual: v => { },
		defaultValue: true
	);

	public static readonly SliderSetting<float> Deadzone = new(
		name: nameof(Deadzone),
		getActual: () => default,
		setActual: v => { },
		defaultValue: 0.2f,
		min: 0f,
		max: 1f,
		step: 0.1f
	);

	public static readonly SliderSetting<float> ControllerSensitivity = new(
		name: nameof(ControllerSensitivity),
		getActual: () => ControllerSensitivity!.Target,
		setActual: v => { },
		defaultValue: 150f,
		min: 1f,
		max: 500f,
		step: 1f
	);

	private static readonly ISetting[] All = [Vibration, Deadzone, ControllerSensitivity];

	public static void Apply() => All.Apply();
	public static void Reset() => All.Reset();

	public static ControllerData Export() => new ControllerData {
		Vibration = Vibration.Target,
		Deadzone = Deadzone.Target,
		ControllerSensitivity = ControllerSensitivity.Target,
	};

	public static void Import(ControllerData data) {
		Vibration.Target = data.Vibration ?? Vibration.Default;
		Deadzone.Target = data.Deadzone ?? Deadzone.Default;
		ControllerSensitivity.Target = data.ControllerSensitivity ?? ControllerSensitivity.Default;
	}
}

public readonly record struct ControllerData : ISaveData {
	public bool? Vibration { get; init; }
	public float? Deadzone { get; init; }
	public float? ControllerSensitivity { get; init; }
}
