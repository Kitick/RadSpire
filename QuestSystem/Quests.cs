namespace QuestSystem;

using Root;

public abstract record QuestObjective(string Description, int RequiredCount = 1);

public sealed record KillObjective(string Description, int RequiredCount, string EnemyGroup = "")
	: QuestObjective(Description, RequiredCount);

public sealed record CollectObjective(string Description, int RequiredCount, string ItemId)
	: QuestObjective(Description, RequiredCount);

public sealed record LocationObjective(string Description, string LocationId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record TalkObjective(string Description, string NpcId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record QuestDefinition(
	string Id,
	string Title,
	string Description,
	bool IsMainQuest,
	int StageRequirement,
	QuestObjective[] Objectives
);

public static class Quests {
	public static readonly QuestDefinition KillPatrol = new(
		Id: QuestID.Stage0_KillPatrol,
		Title: "Clear the Patrol",
		Description: "Defeat the enemies guarding the area.",
		IsMainQuest: true,
		StageRequirement: 0,
		Objectives: [new KillObjective("Kill 3 enemies", RequiredCount: 3)]
	);

	public static readonly QuestDefinition[] All = [KillPatrol];
}
