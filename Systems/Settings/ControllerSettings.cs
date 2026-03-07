namespace Services.Settings {
	public static class ControllerSettings {
		public static readonly Setting<bool> Vibration = new(
			name: nameof(Vibration),
			getActual: () => default,
			setActual: v => { },
			defaultValue: true
		);

		public static readonly Setting<float> Deadzone = new(
			name: nameof(Deadzone),
			getActual: () => default,
			setActual: v => { },
			defaultValue: 0.2f
		);

		public static readonly Setting<float> ControllerSensitivity = new(
			name: nameof(ControllerSensitivity),
			getActual: () => ControllerSensitivity!.Target,
			setActual: v => { },
			defaultValue: 150f
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
			Vibration.Target = data.Vibration;
			Deadzone.Target = data.Deadzone;
			ControllerSensitivity.Target = data.ControllerSensitivity;
		}
	}

	public readonly record struct ControllerData : ISaveData {
		public bool Vibration { get; init; }
		public float Deadzone { get; init; }
		public float ControllerSensitivity { get; init; }
	}
}
