namespace QuestSystem;

using Root;
using Services;

public enum QuestStatus { Pending, Active, Completed }

public enum QuestType { Main, Side }

public readonly record struct QuestObjectiveProgress : ISaveData {
	public int CurrentCount { get; init; }
	public bool IsCompleted { get; init; }
}

public readonly record struct QuestProgress : ISaveData {
	public QuestStatus Status { get; init; }
	public QuestObjectiveProgress[] Objectives { get; init; }

	public static QuestProgress Pending(QuestDefinition def) => new() {
		Status = QuestStatus.Pending,
		Objectives = new QuestObjectiveProgress[def.Objectives.Length],
	};

	public static QuestProgress Active(QuestDefinition def) => new() {
		Status = QuestStatus.Active,
		Objectives = new QuestObjectiveProgress[def.Objectives.Length],
	};
}

public readonly record struct QuestProgressionData : ISaveData {
	public int CurrentStage { get; init; }
	public QuestID[] QuestTitles { get; init; }
	public QuestProgress[] QuestProgresses { get; init; }
}
