namespace Services.Settings {
	using Godot;

	public static class MouseKeyboardSettings {
		public static readonly Setting<float> MouseSensitivity = new(
			name: nameof(MouseSensitivity),
			getActual: () => default,
			setActual: v => { },
			defaultValue: 1.0f
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
			MouseSensitivity.Target = data.MouseSensitivity;
			InvertedYAxis.Target = data.InvertedYAxis;
			RawInput.Target = data.RawInput;
		}
	}

	public readonly record struct MouseKeyboardData : ISaveData {
		public float MouseSensitivity { get; init; }
		public bool InvertedYAxis { get; init; }
		public bool RawInput { get; init; }
	}
}
