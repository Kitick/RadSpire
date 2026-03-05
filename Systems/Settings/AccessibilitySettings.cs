namespace Services.Settings {
	public static class AccessibilitySettings {
		public static readonly Setting<bool> Subtitles = new(
			name: nameof(Subtitles),
			getActual: () => default,
			setActual: v => { },
			defaultValue: false
		);

		public static readonly Setting<float> SubtitleSize = new(
			name: nameof(SubtitleSize),
			getActual: () => default,
			setActual: v => { },
			defaultValue: 1.0f
		);

		public static readonly Setting<string> ColorblindMode = new(
			name: nameof(ColorblindMode),
			getActual: () => default!,
			setActual: v => { },
			defaultValue: "None"
		);

		public static readonly Setting<bool> TextToSpeech = new(
			name: nameof(TextToSpeech),
			getActual: () => default,
			setActual: v => { },
			defaultValue: false
		);

		public static readonly Setting<bool> HighContrastUI = new(
			name: nameof(HighContrastUI),
			getActual: () => default,
			setActual: v => { },
			defaultValue: false
		);

		private static readonly ISetting[] All = [Subtitles, SubtitleSize, ColorblindMode, TextToSpeech, HighContrastUI];

		public static void Apply() => All.Apply();
		public static void Reset() => All.Reset();

		public static AccessibilityData Export() => new AccessibilityData {
			Subtitles = Subtitles.Target,
			SubtitleSize = SubtitleSize.Target,
			ColorblindMode = ColorblindMode.Target,
			TextToSpeech = TextToSpeech.Target,
			HighContrastUI = HighContrastUI.Target,
		};

		public static void Import(AccessibilityData data) {
			Subtitles.Target = data.Subtitles;
			SubtitleSize.Target = data.SubtitleSize;
			ColorblindMode.Target = data.ColorblindMode ?? ColorblindMode.Default;
			TextToSpeech.Target = data.TextToSpeech;
			HighContrastUI.Target = data.HighContrastUI;
		}
	}

	public readonly record struct AccessibilityData : ISaveData {
		public bool Subtitles { get; init; }
		public float SubtitleSize { get; init; }
		public string ColorblindMode { get; init; }
		public bool TextToSpeech { get; init; }
		public bool HighContrastUI { get; init; }
	}
}
