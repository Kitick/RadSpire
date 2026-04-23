namespace QuestSystem;

using System.Collections.Generic;
using Root;

public abstract record QuestObjective(string Description, int RequiredCount = 1);

public sealed record KillObjective(string Description, int RequiredCount, EnemyType EnemyType)
	: QuestObjective(Description, RequiredCount);

public sealed record CollectObjective(string Description, int RequiredCount, string ItemId)
	: QuestObjective(Description, RequiredCount);

public sealed record LocationObjective(string Description, LocationID LocationId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record TalkObjective(string Description, NPCID NpcId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record QuestDefinition(
	string Title,
	string Description,
	QuestType Type,
	int StageRequirement,
	QuestObjective[] Objectives,
	NPCID NpcId = NPCID.None,
	string[] InitialDialogue = null!,
	string[] ActiveDialogue = null!,
	string[] CompletionDialogue = null!
);

public static class Quests {
	// Stage 0 — auto-activates on spawn, completes when player talks to Sera
	public static readonly QuestDefinition LeftForDead = new(
		Title: "Left for Dead",
		Description: "Ambushed and stripped of your weapons, you need to find help.",
		Type: QuestType.Main,
		StageRequirement: 0,
		Objectives: [new TalkObjective("Find help", NPCID.Sera)]
	);

	// Stage 1 — NPC-gated on Sera, main quest is crafting the sword
	public static readonly QuestDefinition ArmYourself = new(
		Title: "Arm Yourself",
		Description: "Craft a sword with Sera's guidance.",
		Type: QuestType.Main,
		StageRequirement: 1,
		Objectives: [
			new CollectObjective("Craft a sword", RequiredCount: 1, ItemId: ItemID.SwordIron),
		],
		NpcId: NPCID.Sera,
		InitialDialogue: [
			"Hey. Hey, over here. You look like you're about to fall over.",
			"Get inside. Those were Meldoran men that did this to you, weren't they? I can tell by the way they left you. They don't finish what they start.",
			"I'm not going to hurt you. My name is Sera. I'm a Wanderer with the League of Krones, posted out here for weeks mapping the area. Sit down before you fall down.",
			"I know this land better than anyone within ten miles. I know where to find what you'll need.",
			"I'll help you get back on your feet — materials, tools, everything I know. But I need something from you first.",
			"My friend Dag was taken by the same men who did this to you. Help me get him back, and I'll get you armed.",
			"First things first. get yourself a sword. Iron ore from the craters, process to chunks, smelt to bars, then forge. Two iron bars, two sticks, a stone.",
		],
		ActiveDialogue: [
			"Sword needs two iron bars, two sticks, and a stone. Iron ore comes from the craters.",
			"Process ore into chunks, smelt the chunks into bars, then you can work with it.",
		],
		CompletionDialogue: [
			"That's better. You look less like a corpse now.",
			"I can't go with you. I wish I could. Just please be careful.",
		]
	);

	// Stage 1 — side quest, same NPC gate as ArmYourself
	public static readonly QuestDefinition ArmYourselfSide = new(
		Title: "Better Protected",
		Description: "Craft a shield and helmet while you have the materials.",
		Type: QuestType.Side,
		StageRequirement: 1,
		Objectives: [
			new CollectObjective("Craft a shield", RequiredCount: 1, ItemId: ItemID.ShieldIron),
			new CollectObjective("Craft a helmet", RequiredCount: 1, ItemId: ItemID.HeadpieceIron),
		],
		NpcId: NPCID.Sera
	);

	// Stage 2 — activates when player talks to Sera again after ArmYourself completes
	public static readonly QuestDefinition ADealIsADeal = new(
		Title: "A Deal is a Deal",
		Description: "Rescue Dag from the Warriors of Meldoran at the ruined gas station.",
		Type: QuestType.Main,
		StageRequirement: 2,
		Objectives: [
			new KillObjective("Defeat the Warriors of Meldoran at the gas station", RequiredCount: 5, EnemyType: EnemyType.MeldoranWarrior),
			new TalkObjective("Talk to Dag", NPCID.Dag),
		],
		NpcId: NPCID.Sera,
		InitialDialogue: [
			"You actually did it. You got yourself armed.",
			"The gas station down the road — four or five of them, maybe more. Dag is still there.",
			"Get him out. That's the deal.",
		],
		ActiveDialogue: [
			"The gas station down the road. Four or five of them, maybe more. Get yourself armed first.",
			"Dag is stubborn — he would have done something stupid trying to get himself out by now. I just hope he's still in one piece.",
		],
		CompletionDialogue: [
			"You actually did it. Both of you.",
			"Dag — are you alright? Don't answer that, just sit down.",
			"You did well. Better than I had any right to hope.",
			"Go. And if our paths cross again, the debt goes the other way.",
		]
	);

	public static readonly Dictionary<QuestID, QuestDefinition> All = new() {
		[QuestID.LeftForDead] = LeftForDead,
		[QuestID.ArmYourself] = ArmYourself,
		[QuestID.ArmYourselfSide] = ArmYourselfSide,
		[QuestID.ADealIsADeal] = ADealIsADeal,
	};
}
