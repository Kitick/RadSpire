namespace Services.Settings {
	public static class ControllerSettings {
		public static readonly Setting<bool> EnableController = new(
			name: nameof(EnableController),
			getActual: () => default,
			setActual: v => { },
			defaultValue: true
		);

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

		private static readonly ISetting[] All = [EnableController, Vibration, Deadzone];

		public static void Apply() => All.Apply();
		public static void Reset() => All.Reset();

		public static ControllerData Export() => new ControllerData {
			EnableController = EnableController.Target,
			Vibration = Vibration.Target,
			Deadzone = Deadzone.Target,
		};

		public static void Import(ControllerData data) {
			EnableController.Target = data.EnableController;
			Vibration.Target = data.Vibration;
			Deadzone.Target = data.Deadzone;
		}
	}

	public readonly record struct ControllerData : ISaveData {
		public bool EnableController { get; init; }
		public bool Vibration { get; init; }
		public float Deadzone { get; init; }
	}
}
