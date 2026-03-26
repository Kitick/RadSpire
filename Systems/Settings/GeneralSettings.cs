namespace Services.Settings;

public static class GeneralSettings {
	public static readonly Setting<string> Language = new(
		name: nameof(Language),
		getActual: () => default!,
		setActual: v => { },
		defaultValue: "English"
	);

	public static readonly Setting<float> UIScale = new(
		name: nameof(UIScale),
		getActual: () => default,
		setActual: v => { },
		defaultValue: 1.0f
	);

	public static readonly Setting<string> Theme = new(
		name: nameof(Theme),
		getActual: () => default!,
		setActual: v => { },
		defaultValue: "Default"
	);

	private static readonly ISetting[] All = [Language, UIScale, Theme];

	public static void Apply() => All.Apply();
	public static void Reset() => All.Reset();

	public static GeneralData Export() => new GeneralData {
		Language = Language.Target,
		UIScale = UIScale.Target,
		Theme = Theme.Target,
	};

	public static void Import(GeneralData data) {
		Language.Target = data.Language ?? Language.Default;
		UIScale.Target = data.UIScale;
		Theme.Target = data.Theme ?? Theme.Default;
	}
}

public readonly record struct GeneralData : ISaveData {
	public string Language { get; init; }
	public float UIScale { get; init; }
	public string Theme { get; init; }
}
