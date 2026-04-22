namespace Settings;

using Godot;
using Services;

public static class MouseKeyboardSettings {
	public static readonly SliderSetting<float> MouseSensitivity = new(
		name: nameof(MouseSensitivity),
		getActual: () => MouseSensitivity!.Target,
		setActual: v => { },
		defaultValue: 0.5f,
		min: 0.1f,
		max: 2f,
		step: 0.1f
	);

	public static readonly Setting<bool> InvertedYAxis = new(
		name: nameof(InvertedYAxis),
		getActual: () => default,
		setActual: v => { },
		defaultValue: false
	);

	public static readonly Setting<bool> RawInput = new(
		name: nameof(RawInput),
		getActual: () => !Input.UseAccumulatedInput,
		setActual: v => Input.UseAccumulatedInput = !v,
		defaultValue: false
	);

	private static readonly ISetting[] All = [MouseSensitivity, InvertedYAxis, RawInput];

	public static void Apply() => All.Apply();
	public static void Reset() => All.Reset();

	public static MouseKeyboardData Export() => new MouseKeyboardData {
		MouseSensitivity = MouseSensitivity.Target,
		InvertedYAxis = InvertedYAxis.Target,
		RawInput = RawInput.Target,
	};

	public static void Import(MouseKeyboardData data) {
		MouseSensitivity.Target = data.MouseSensitivity ?? MouseSensitivity.Default;
		InvertedYAxis.Target = data.InvertedYAxis ?? InvertedYAxis.Default;
		RawInput.Target = data.RawInput ?? RawInput.Default;
	}
}

public readonly record struct MouseKeyboardData : ISaveData {
	public float? MouseSensitivity { get; init; }
	public bool? InvertedYAxis { get; init; }
	public bool? RawInput { get; init; }
}
