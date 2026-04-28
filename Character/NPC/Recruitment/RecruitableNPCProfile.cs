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
	public static readonly RecruitableNPCProfile Rowan = new(
		NpcId: NPCID.Rowan,
		DisplayName: "Rowan",
		PrereqQuests: [],
		CollectionQuest: QuestID.RowanJoinsCamp,
		PostCollectionQuests: [QuestID.RowanStockTheShelter],
		ModelScenePath: string.Empty,
		FollowingDialogue: [
			"Keep moving. I'll stay close.",
			"Once we reach a structure, assign me there and I'll settle in."
		],
		AssignedDialogue: [
			"This place will do. Talk to me if you want work done around camp.",
			"I can help keep the shelter running once we have enough supplies."
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
			[Rowan.NpcId] = Rowan,
			[Dag.NpcId] = Dag,
		};
}
