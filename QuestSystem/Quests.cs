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
			"You're alive. That alone puts you ahead of most people on this road.",
			"My name is Rowan.",
			"I was scavenging alone until the raiders started looping back through here.",
			"I have skills. Storage. Repairs. Inventory tracking. The unglamorous things that keep people breathing.",
			"I do not want to die out here proving a point.",
			"If you have room for one more, I will come with you.",
			"Somewhere with walls. Somewhere radiation does not sink into your bones."
		],
		ActiveDialogue: [
			"If you have room for one more, I will come with you.",
		],
		CompletionDialogue: [
			"Alright. I am with you.",
			"Point me to the camp and I will make myself useful.",
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
			"Before I move anywhere, I need to know we can survive there.",
			"Bring me ten pieces of wood.",
			"If you can manage that, I can manage the rest."
		],
		ActiveDialogue: [
			"Bring me ten pieces of wood.",
		],
		CompletionDialogue: [
			"Good haul.",
			"This is enough to start reinforcing, organize supplies, and stop sleeping on damp ground.",
			"I will move when you say the word."
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
			"Stop right there.",
			"You have radiation in your blood already. I can see it.",
			"If you do not put down roots soon, it will eat through you.",
			"Craft a tent. That comes first.",
			"Then come back to me."
		],
		ActiveDialogue: [
			"Craft a tent. That comes first.",
		],
		CompletionDialogue: [
			"Good. You understand priorities.",
			"Place the tent. Assign me there.",
			"I will not wander until there is somewhere safe to wander back to.",
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
			"Good. You understand priorities.",
			"Place the tent. Assign me there.",
			"I will not wander until there is somewhere safe to wander back to.",
		],
		ActiveDialogue: [
			"Place the tent. Assign me there.",
		],
		CompletionDialogue: [
			"Alright. I am with you.",
			"Point me to the camp and I will make myself useful.",
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
			"Camp looks solid.",
			"Next step is a bed.",
			"Sleeping is the only reliable way to purge radiation buildup.",
			"Craft one and place it. Then you might live long enough to regret things.",
		],
		ActiveDialogue: [
			"Craft a bed and place it. You need it to purge radiation buildup.",
		],
		CompletionDialogue: [
			"Perfect.",
			"With that, you can survive repeated exposure.",
			"Do not get reckless just because you can heal.",
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
