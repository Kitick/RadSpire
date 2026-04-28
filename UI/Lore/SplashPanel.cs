using System;
using System.Collections.Generic;
using Godot;

public partial class SplashPanel : Control {
	[Export] public float InitialDelaySeconds = 0.0f;
	[Export] public float DelayBetweenLabelsSeconds = 3.0f;
	[Export] public bool UseDynamicDelayByTextLength = true;
	[Export] public float ReadingCharactersPerSecond = 18.0f;
	[Export] public float MaxDelayBetweenLabelsSeconds = 20.0f;
	[Export] public float FadeDurationSeconds = 0.7f;
	[Export] public float SkipUnlockDelaySeconds = 8.0f;
	[Export] private Button SkipButton = null!;

	private bool IsSkipRequested;
	private readonly List<Label> Labels = [];
	private int NextLabelIndex;

	public override void _Ready() {
		ColorRect labelContainer = GetNodeOrNull<ColorRect>("ColorRect");

		if(labelContainer == null) {
			GD.PushError("SplashPanel: 'ColorRect' node was not found.");
			return;
		}

		if(!IsInstanceValid(SkipButton)) {
			GD.PushWarning("SplashPanel: exported 'SkipButton' is not assigned.");
		}
		else {
			SkipButton.Visible = false;
			SkipButton.Disabled = true;
			SkipButton.Pressed += OnSkipButtonPressed;
			StartTimer(SkipUnlockDelaySeconds, EnableSkipButton);
		}

		foreach(Node child in labelContainer.GetChildren()) {
			if(child is Label label) {
				Labels.Add(label);
				label.Visible = false;
				label.Modulate = new Color(1f, 1f, 1f, 0f);
				label.Scale = Vector2.One;
			}
		}

		if(Labels.Count == 0) {
			GD.PushWarning("SplashPanel: No Label children were found under 'ColorRect'.");
			return;
		}

		StartTimer(InitialDelaySeconds, RevealNextLabel);
	}

	public override void _ExitTree() {
		if(IsInstanceValid(SkipButton)) {
			SkipButton.Pressed -= OnSkipButtonPressed;
		}
	}

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
		if(!IsInstanceValid(label)) {
			return;
		}

		label.Visible = true;

		Tween tween = CreateTween();
		tween.TweenProperty(label, "modulate:a", 1.0f, FadeDurationSeconds).From(0.0f);
	}

	private void RevealNextLabel() {
		if(IsSkipRequested || !IsInsideTree()) {
			return;
		}

		if(NextLabelIndex >= Labels.Count) {
			QueueFree();
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

	private void EnableSkipButton() {
		if(IsSkipRequested || !IsInsideTree() || !IsInstanceValid(SkipButton)) {
			return;
		}

		SkipButton.Visible = true;
		SkipButton.Disabled = false;
	}

	private void StartTimer(float seconds, Action callback) {
		if(IsSkipRequested || !IsInsideTree()) {
			return;
		}

		if(seconds <= 0f) {
			callback();
			return;
		}

		SceneTreeTimer timer = GetTree().CreateTimer(seconds);
		timer.Timeout += () => {
			if(IsSkipRequested || !IsInsideTree()) {
				return;
			}

			callback();
		};
	}

	private void OnSkipButtonPressed() {
		if(IsSkipRequested) {
			return;
		}

		IsSkipRequested = true;
		QueueFree();
	}
}
