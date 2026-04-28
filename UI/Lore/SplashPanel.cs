namespace UI;

using System;
using System.Collections.Generic;
using Godot;
using Services;

public partial class SplashPanel : Control {
	private static readonly LogService Log = new(nameof(SplashPanel), enabled: true);

	[ExportCategory("Nodes")]
	[Export] private Button SkipButton = null!;
	[Export] private ColorRect LabelContainer = null!;

	[ExportCategory("Config")]
	[Export] public float InitialDelaySeconds = 0.0f;
	[Export] public float DelayBetweenLabelsSeconds = 3.0f;
	[Export] public bool UseDynamicDelayByTextLength = true;
	[Export] public float ReadingCharactersPerSecond = 18.0f;
	[Export] public float MaxDelayBetweenLabelsSeconds = 20.0f;
	[Export] public float FadeDurationSeconds = 0.7f;
	[Export] public float SkipUnlockDelaySeconds = 8.0f;

	private readonly List<Label> Labels = [];
	private int NextLabelIndex;

	public event Action? Finished;

	public override void _Ready() {
		SkipButton.Visible = false;
		SkipButton.Disabled = true;
		SkipButton.Pressed += OnSkipButtonPressed;

		StartTimer(SkipUnlockDelaySeconds, () => {
			SkipButton.Visible = true;
			SkipButton.Disabled = false;
		});

		foreach(Node child in LabelContainer.GetChildren()) {
			if(child is Label label) {
				Labels.Add(label);
				label.Visible = false;
				label.Modulate = new Color(1f, 1f, 1f, 0f);
				label.Scale = Vector2.One;
			}
		}

		if(Labels.Count == 0) {
			Log.Warn("No Label children were found under 'ColorRect'.");
			return;
		}

		StartTimer(InitialDelaySeconds, RevealNextLabel);
	}

	public override void _ExitTree() => SkipButton.Pressed -= OnSkipButtonPressed;

	private float GetDelayForLabel(Label label) {
		if(!UseDynamicDelayByTextLength || ReadingCharactersPerSecond <= 0f) {
			return DelayBetweenLabelsSeconds;
		}

		int characterCount = label.Text?.Length ?? 0;
		float readingDelaySeconds = characterCount / ReadingCharactersPerSecond;
		float targetDelaySeconds = Mathf.Max(DelayBetweenLabelsSeconds, readingDelaySeconds);
		return Mathf.Clamp(targetDelaySeconds, DelayBetweenLabelsSeconds, MaxDelayBetweenLabelsSeconds);
	}

	private void ShowLabelWithReveal(Label label) {
		label.Visible = true;

		Tween tween = CreateTween();
		tween.TweenProperty(label, "modulate:a", 1.0f, FadeDurationSeconds).From(0.0f);
	}

	private void RevealNextLabel() {
		if(!IsInstanceValid(this)) {
			return;
		}

		if(NextLabelIndex >= Labels.Count) {
			Finished?.Invoke();
			return;
		}

		Label currentLabel = Labels[NextLabelIndex];
		ShowLabelWithReveal(currentLabel);

		float waitSeconds = NextLabelIndex < Labels.Count - 1
			? GetDelayForLabel(currentLabel)
			: Mathf.Max(FadeDurationSeconds, GetDelayForLabel(currentLabel));

		NextLabelIndex += 1;
		StartTimer(waitSeconds, RevealNextLabel);
	}

	private void StartTimer(float seconds, Action callback) {
		if(!IsInstanceValid(this)) {
			return;
		}

		if(seconds <= 0f) {
			callback();
			return;
		}

		SceneTreeTimer timer = GetTree().CreateTimer(seconds);
		timer.Timeout += () => {
			if(!IsInstanceValid(this)) {
				return;
			}

			callback();
		};
	}

	private void OnSkipButtonPressed() => Finished?.Invoke();
}
