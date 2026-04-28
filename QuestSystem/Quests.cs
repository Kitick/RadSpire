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
	QuestID[] Prerequisites = null!,
	QuestOfferMode OfferMode = QuestOfferMode.AutoByStage,
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
			"Get inside. Those were Meldoran men that did this to you, weren't they?",
			"I can tell by the way they left you. They don't finish what they start.",
			"I'm not going to hurt you. My name is Sera.",
			"I'm a Wanderer with the League of Krones, posted out here for weeks mapping the area. Sit down before you fall down.",
			"I know this land better than anyone within ten miles. I know where to find what you'll need.",
			"I'll help you get back on your feet. Materials, tools, everything I know.",
			"But I need something from you first.",
			"My friend Dag was taken by the same men who did this to you. Help me get him back, and I'll get you armed.",
			"First things first. Get yourself a sword.",
			"Iron ore from the craters. Process the ore into chunks, smelt the chunks into bars, then forge.",
			"Two iron bars, two sticks, a stone.",
		],
		ActiveDialogue: [
			"Sword needs two iron bars, two sticks, and a stone. Iron ore comes from the craters.",
			"Process ore into chunks, smelt the chunks into bars, then you can work with it.",
			"Watch the radiation out there. It creeps up on you. Find a bed and sleep it off before it gets bad.",
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
			"The gas station down the road. Four or five of them, maybe more. Dag is still there.",
			"Get him out. That's the deal.",
		],
		ActiveDialogue: [
			"The gas station down the road. Four or five of them, maybe more. Get yourself armed first.",
			"Dag is stubborn. He would have done something stupid trying to get himself out by now. I just hope he's still in one piece.",
		],
		CompletionDialogue: [
			"You actually did it. Both of you.",
			"Dag. Are you alright? Don't answer that, just sit down.",
			"You did well. Better than I had any right to hope.",
			"Go. And if our paths cross again, the debt goes the other way.",
		]
	);

	public static readonly QuestDefinition RowanJoinsCamp = new(
		Title: "A New Hand",
		Description: "Talk to Rowan and convince him to join you.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new TalkObjective("Talk to Rowan", NPCID.Rowan),
		],
		NpcId: NPCID.Rowan,
		InitialDialogue: [
			"You're alive. Good. I was starting to think I'd be the next body on this road.",
			"My name is Rowan. I was scavenging alone until the raiders started circling back through here.",
			"I don't want to stay exposed out here. If you've got room for one more, I'll come with you.",
			"Get me somewhere with walls and a roof, and I'll make myself useful."
		],
		ActiveDialogue: [
			"If you're taking me in, say the word. I am ready to move.",
		],
		CompletionDialogue: [
			"Right. I'm with you now. Lead on and I'll follow.",
		]
	);

	public static readonly QuestDefinition RowanStockTheShelter = new(
		Title: "Stock the Shelter",
		Description: "Bring Rowan enough wood to start making your shelter livable.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Bring Rowan 10 wood", RequiredCount: 10, ItemId: ItemID.Wood),
		],
		NpcId: NPCID.Rowan,
		Prerequisites: [QuestID.RowanJoinsCamp],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"If I'm staying here, this place needs supplies.",
			"Bring me ten pieces of wood and I'll get started shoring things up."
		],
		ActiveDialogue: [
			"Ten pieces of wood. That's enough to get a proper stockpile started.",
		],
		CompletionDialogue: [
			"This is exactly what I needed. Good haul.",
			"Give me a little time and I'll turn this pile into something useful."
		]
	);

	public static readonly QuestDefinition ColinRadiationCamp = new(
		Title: "Set Up a Base Camp",
		Description: "Colin warned you about radiation. Craft a tent so you can establish a safe base camp.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Craft a tent", RequiredCount: 1, ItemId: ItemID.Tent),
		],
		NpcId: NPCID.Colin,
		InitialDialogue: [
			"There is radiation all around here. You need to set up a base camp before it chews through you.",
			"Go craft a tent, then come back to me."
		],
		ActiveDialogue: [
			"Craft a tent first. You need a base camp to survive out here.",
		],
		CompletionDialogue: [
			"Good. Now place that tent down and I'll follow you there.",
		]
	);

	public static readonly QuestDefinition ColinFollowToCamp = new(
		Title: "Lead Colin to Camp",
		Description: "Talk to Colin so he can follow you, then place down your tent and assign him to your camp.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new TalkObjective("Talk to Colin", NPCID.Colin),
		],
		NpcId: NPCID.Colin,
		Prerequisites: [QuestID.ColinRadiationCamp],
		InitialDialogue: [
			"Alright, let's move. Place the tent down and assign me there once we're at camp.",
		],
		ActiveDialogue: [
			"Place that tent and assign me when you are ready.",
		],
		CompletionDialogue: [
			"Right behind you. Let's get this base camp running.",
		]
	);

	public static readonly QuestDefinition ColinCraftBed = new(
		Title: "Build a Bed",
		Description: "Craft a bed so you can heal radiation damage while resting at camp.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Craft a bed", RequiredCount: 1, ItemId: ItemID.BedSmall),
		],
		NpcId: NPCID.Colin,
		Prerequisites: [QuestID.ColinFollowToCamp],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"Looks like an amazing base camp. I'll stay here and help you along the way.",
			"Next step: craft a bed. That's how you can heal your radiation damage.",
		],
		ActiveDialogue: [
			"Get a bed crafted. Sleeping is your best way to clear radiation buildup.",
		],
		CompletionDialogue: [
			"Perfect. With a bed in camp you'll recover from radiation a lot faster.",
		]
	);

	public static readonly Dictionary<QuestID, QuestDefinition> All = new() {
		[QuestID.LeftForDead] = LeftForDead,
		[QuestID.ArmYourself] = ArmYourself,
		[QuestID.ArmYourselfSide] = ArmYourselfSide,
		[QuestID.ADealIsADeal] = ADealIsADeal,
		[QuestID.RowanJoinsCamp] = RowanJoinsCamp,
		[QuestID.RowanStockTheShelter] = RowanStockTheShelter,
		[QuestID.ColinRadiationCamp] = ColinRadiationCamp,
		[QuestID.ColinFollowToCamp] = ColinFollowToCamp,
		[QuestID.ColinCraftBed] = ColinCraftBed,
	};
}
