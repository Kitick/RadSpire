namespace Character.Recruitment;

using System.Collections.Generic;
using Root;

public sealed record RecruitableNPCProfile(
	NPCID NpcId,
	string DisplayName,
	QuestID[] PrereqQuests,
	QuestID CollectionQuest,
	QuestID[] PostCollectionQuests,
	string ModelScenePath,
	string[] FollowingDialogue,
	string[] AssignedDialogue
);

public static class RecruitableNPCProfiles {
	public static readonly RecruitableNPCProfile Mara = new(
		NpcId: NPCID.Mara,
		DisplayName: "Mara",
		PrereqQuests: [QuestID.FoundationsThatLast],
		CollectionQuest: QuestID.MaraJoinsTheCamp,
		PostCollectionQuests: [QuestID.HearthAgainstTheAsh],
		ModelScenePath: string.Empty,
		FollowingDialogue: [
			"I will come with you.",
			"Not because I trust you.",
			"But because standing still will get us both killed.",
			"My work does not leave me much choice.",
			"Assign me somewhere with a view of the land.",
			"I need to see threats before they reach us."
		],
		AssignedDialogue: [
			"The spires do not move.",
			"But people do.",
			"And people are more dangerous."
		]
	);

	public static readonly RecruitableNPCProfile Colin = new(
		NpcId: NPCID.Colin,
		DisplayName: "Colin",
		PrereqQuests: [QuestID.ColinRadiationCamp],
		CollectionQuest: QuestID.ColinFollowToCamp,
		PostCollectionQuests: [QuestID.ColinCraftBed],
		ModelScenePath: string.Empty,
		FollowingDialogue: [
			"Good. You understand priorities.",
			"Place the tent. Assign me there.",
			"I will not wander until there is somewhere safe to wander back to."
		],
		AssignedDialogue: [
			"Camp looks solid.",
			"Next step is a bed.",
			"Sleeping is the only reliable way to purge radiation buildup."
		]
	);

	public static readonly RecruitableNPCProfile Rowan = new(
		NpcId: NPCID.Rowan,
		DisplayName: "Rowan",
		PrereqQuests: [],
		CollectionQuest: QuestID.RowanJoinsCamp,
		PostCollectionQuests: [QuestID.RowanStockTheShelter],
		ModelScenePath: string.Empty,
		FollowingDialogue: [
			"Alright. I am with you.",
			"Point me to the camp and I will make myself useful."
		],
		AssignedDialogue: [
			"Give me materials and time. I can turn chaos into something stable."
		]
	);

	public static readonly RecruitableNPCProfile Dag = new(
		NpcId: NPCID.Dag,
		DisplayName: "Dag",
		PrereqQuests: [],
		CollectionQuest: QuestID.ADealIsADeal,
		PostCollectionQuests: [],
		ModelScenePath: string.Empty,
		FollowingDialogue: [
			"I am right behind you.",
			"Let's find somewhere safe."
		],
		AssignedDialogue: [
			"I can keep watch from here.",
			"If you need anything, just ask."
		]
	);

	public static readonly IReadOnlyDictionary<NPCID, RecruitableNPCProfile> All =
		new Dictionary<NPCID, RecruitableNPCProfile> {
			[Mara.NpcId] = Mara,
			[Colin.NpcId] = Colin,
			[Rowan.NpcId] = Rowan,
			[Dag.NpcId] = Dag,
		};
}
