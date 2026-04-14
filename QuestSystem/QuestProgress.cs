namespace QuestSystem;

using Services;

public enum QuestStatus { NotStarted, Active, Completed, Failed }

public readonly record struct QuestObjectiveProgress : ISaveData {
	public int CurrentCount { get; init; }
	public bool IsCompleted { get; init; }
}

public readonly record struct QuestProgress : ISaveData {
	public QuestStatus Status { get; init; }
	public QuestObjectiveProgress[] Objectives { get; init; }

	public static QuestProgress For(QuestDefinition def) => new() {
		Status = QuestStatus.Active,
		Objectives = new QuestObjectiveProgress[def.Objectives.Length],
	};
}

public readonly record struct QuestProgressionData : ISaveData {
	public int CurrentStage { get; init; }
	public string[] QuestIds { get; init; }
	public QuestProgress[] QuestProgresses { get; init; }
}
