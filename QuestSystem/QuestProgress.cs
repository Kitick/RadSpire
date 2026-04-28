namespace QuestSystem;

using Root;
using Services;

public enum QuestStatus { Pending, Active, Completed }

public enum QuestType { Main, Side }

public enum QuestOfferMode { AutoByStage, OfferedByNpc }

public readonly record struct QuestObjectiveProgress : ISaveData {
	public int CurrentCount { get; init; }
	public bool IsCompleted { get; init; }
}

public readonly record struct QuestProgress : ISaveData {
	public QuestStatus Status { get; init; }
	public QuestObjectiveProgress[] Objectives { get; init; }
	public bool InitialDialogueDelivered { get; init; }
	public bool ReturnObjectivePending { get; init; }

	public static QuestProgress Pending(QuestDefinition def) => new() {
		Status = QuestStatus.Pending,
		Objectives = new QuestObjectiveProgress[def.Objectives.Length],
	};

	public static QuestProgress Active(QuestDefinition def) => new() {
		Status = QuestStatus.Active,
		Objectives = new QuestObjectiveProgress[def.Objectives.Length],
	};
}

// A fixed item reward — always gives this item at this quantity.
public readonly record struct QuestItemReward(string ItemId, int Quantity = 1);

// A random pool — picks Count items from the Choices array (with replacement).
public readonly record struct QuestRewardPool(QuestItemReward[] Choices, int Count = 1);

public readonly record struct QuestProgressionData : ISaveData {
	public int CurrentStage { get; init; }
	public QuestID[] QuestTitles { get; init; }
	public QuestProgress[] QuestProgresses { get; init; }
}
