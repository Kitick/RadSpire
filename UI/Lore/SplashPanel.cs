using System.Collections.Generic;
using Godot;

public partial class SplashPanel : Control
{
    [Export] public float InitialDelaySeconds = 0.0f;
    [Export] public float DelayBetweenLabelsSeconds = 3.0f;
    [Export] public bool UseDynamicDelayByTextLength = true;
    [Export] public float ReadingCharactersPerSecond = 18.0f;
    [Export] public float MaxDelayBetweenLabelsSeconds = 20.0f;
    [Export] public float FadeDurationSeconds = 0.7f;

    public override async void _Ready()
    {
        ColorRect labelContainer = GetNodeOrNull<ColorRect>("ColorRect");

        if (labelContainer == null)
        {
            GD.PushError("SplashPanel: 'ColorRect' node was not found.");
            return;
        }

        List<Label> labels = new();

        foreach (Node child in labelContainer.GetChildren())
        {
            if (child is Label label)
            {
                labels.Add(label);
                label.Visible = false;
                label.Modulate = new Color(1f, 1f, 1f, 0f);
                label.Scale = Vector2.One;
            }
        }

        if (labels.Count == 0)
        {
            GD.PushWarning("SplashPanel: No Label children were found under 'ColorRect'.");
            return;
        }

        if (InitialDelaySeconds > 0f)
        {
            await ToSignal(GetTree().CreateTimer(InitialDelaySeconds), SceneTreeTimer.SignalName.Timeout);
        }

        for (int i = 0; i < labels.Count; i++)
        {
            ShowLabelWithReveal(labels[i]);

            if (i < labels.Count - 1)
            {
                float waitSeconds = GetDelayForLabel(labels[i]);

                if (waitSeconds > 0f)
                {
                    await ToSignal(GetTree().CreateTimer(waitSeconds), SceneTreeTimer.SignalName.Timeout);
                }
            }
        }
    }

    private float GetDelayForLabel(Label label)
    {
        if (!UseDynamicDelayByTextLength || ReadingCharactersPerSecond <= 0f)
        {
            return DelayBetweenLabelsSeconds;
        }

        int characterCount = label.Text?.Length ?? 0;
        float readingDelaySeconds = characterCount / ReadingCharactersPerSecond;
        float targetDelaySeconds = Mathf.Max(DelayBetweenLabelsSeconds, readingDelaySeconds);
        return Mathf.Clamp(targetDelaySeconds, DelayBetweenLabelsSeconds, MaxDelayBetweenLabelsSeconds);
    }

    private void ShowLabelWithReveal(Label label)
    {
        label.Visible = true;

        Tween tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 1.0f, FadeDurationSeconds).From(0.0f);
    }
}
