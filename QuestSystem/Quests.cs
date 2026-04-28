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

public sealed record StructureValueObjective(string Description, int ValueRequired)
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
	private const int LowHouseValueRequirement = 180;
	private const int MediumHouseValueRequirement = 240;
	private const int HighHouseValueRequirement = 320;
	private const int VeryHighHouseValueRequirement = 420;

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

	public static readonly QuestDefinition RowanHouseLogistics = new(
		Title: "Logistics Under One Roof",
		Description: "Raise Rowan's assigned house value to improve organization and workflow.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new StructureValueObjective(
				"Raise Rowan's house value",
				ValueRequired: MediumHouseValueRequirement
			),
		],
		NpcId: NPCID.Rowan,
		Prerequisites: [QuestID.RowanJoinsCamp],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"We are outgrowing this.",
			"Storage is cramped. Work is slower.",
			"If we reinforce the structure and add proper furnishings, we waste less.",
			"Better tables. Better storage.",
			"Better house means fewer mistakes."
		],
		ActiveDialogue: [
			"Reinforce this place and furnish it properly.",
			"We need cleaner logistics."
		],
		CompletionDialogue: [
			"Much better.",
			"Less clutter, fewer mistakes."
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
		Prerequisites: [QuestID.RowanHouseLogistics],
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

	public static readonly QuestDefinition ColinRecoveryStandards = new(
		Title: "Recovery Standards",
		Description: "Raise Colin's assigned house value to improve functional recovery conditions.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new StructureValueObjective(
				"Raise Colin's house value",
				ValueRequired: MediumHouseValueRequirement
			),
		],
		NpcId: NPCID.Colin,
		Prerequisites: [QuestID.ColinFollowToCamp],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"This works. Barely.",
			"Recovery depends on quality rest.",
			"You add better furnishings, injuries heal faster.",
			"A proper bed. Stable surfaces.",
			"Comfort is not luxury. It is efficiency."
		],
		ActiveDialogue: [
			"Stability and rest speed recovery.",
			"Upgrade this place."
		],
		CompletionDialogue: [
			"Good.",
			"This is efficient enough to recover properly."
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
		Prerequisites: [QuestID.ColinRecoveryStandards],
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

	public static readonly QuestDefinition GildedProof = new(
		Title: "Gilded Proof",
		Description: "Bring Mara proof you can recover rare resources from the ashfields.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Collect 3 Gold Bars", RequiredCount: 3, ItemId: ItemID.GoldBar),
		],
		NpcId: NPCID.Mara,
		InitialDialogue: [
			"Do not come any closer.",
			"Gold is rare out here. Rarer still in hands that have not stolen it.",
			"You want answers. Everyone does.",
			"But answers are expensive, and lies are cheap.",
			"Bring me three gold bars.",
			"Not ore. Not scraps. Bars.",
			"Then we will speak."
		],
		ActiveDialogue: [
			"Three gold bars.",
			"If you found them easily, someone else died for it.",
			"Remember that."
		],
		CompletionDialogue: [
			"So you can gather what the flare buried.",
			"Good. That tells me you are persistent.",
			"Persistence is useful.",
			"But persistence alone breaks people.",
			"If you want to understand what is happening in this land, you will need more than gold."
		]
	);

	public static readonly QuestDefinition BladeOfLegacy = new(
		Title: "Blade of Legacy",
		Description: "Craft the Golden Sword to prove you understand what this world has lost.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Craft the Golden Sword", RequiredCount: 1, ItemId: ItemID.SwordGold),
		],
		NpcId: NPCID.Mara,
		Prerequisites: [QuestID.GildedProof],
		InitialDialogue: [
			"Before the flare, gold was luxury.",
			"After it, gold became memory.",
			"A Golden Sword is a foolish weapon.",
			"It bends. It dulls. It invites envy.",
			"Which is why it tells me exactly who you are when you make one.",
			"Forge it.",
			"Not to fight.",
			"To prove you understand what the world lost."
		],
		ActiveDialogue: [
			"Do not rush the blade.",
			"Gold does not forgive impatience.",
			"If you treat it like iron, it will fail you."
		],
		CompletionDialogue: [
			"You forged it.",
			"You did not have to.",
			"That matters.",
			"The device you seek was never meant to save the world.",
			"It was meant to preserve something specific.",
			"And now I need to know if you are capable of preserving anything at all."
		]
	);

	public static readonly QuestDefinition FoundationsThatLast = new(
		Title: "Foundations That Last",
		Description: "Build a medium house to prove you can create something made to endure.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Build a Medium House", RequiredCount: 1, ItemId: ItemID.HouseMedium),
		],
		NpcId: NPCID.Mara,
		Prerequisites: [QuestID.BladeOfLegacy],
		InitialDialogue: [
			"Power means nothing without permanence.",
			"I will not follow a knight who sleeps under canvas and calls it a future.",
			"Build a house.",
			"Not a shack. Not a tent.",
			"Stone. Timber. Something meant to last longer than a season.",
			"Show me you plan to stay."
		],
		ActiveDialogue: [
			"Medium house.",
			"Walls thick enough to slow radiation.",
			"A roof that does not leak fear."
		],
		CompletionDialogue: [
			"You built something that expects tomorrow.",
			"Very few people do that anymore.",
			"This land erodes hope faster than stone.",
			"Which means you might be worth following."
		]
	);

	public static readonly QuestDefinition MaraJoinsTheCamp = new(
		Title: "Mara Joins the Camp",
		Description: "Talk to Mara and bring her to your settlement so she can begin her work.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new TalkObjective("Talk to Mara", NPCID.Mara),
		],
		NpcId: NPCID.Mara,
		Prerequisites: [QuestID.FoundationsThatLast],
		InitialDialogue: [
			"I will come with you.",
			"Not because I trust you.",
			"But because standing still will get us both killed.",
			"My work does not leave me much choice.",
			"Assign me somewhere with a view of the land.",
			"I need to see threats before they reach us."
		],
		ActiveDialogue: [
			"Assign me somewhere with a view of the land.",
			"I need to see threats before they reach us."
		],
		CompletionDialogue: [
			"I will come with you.",
			"Assign me somewhere with a view of the land."
		]
	);

	public static readonly QuestDefinition MaraBuildForTomorrow = new(
		Title: "Build for Tomorrow",
		Description: "Raise Mara's assigned house value to establish permanence and long-term intent.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new StructureValueObjective(
				"Raise Mara's house value",
				ValueRequired: VeryHighHouseValueRequirement
			),
		],
		NpcId: NPCID.Mara,
		Prerequisites: [QuestID.MaraJoinsTheCamp],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"This is not enough.",
			"Temporary shelters breed temporary thinking.",
			"Better houses anchor people.",
			"Fireplaces. Stone. Furnishings that do not move when the wind shifts.",
			"If this place is to matter in the years ahead, it must look like it intends to stay.",
			"Build for tomorrow, not just for the night."
		],
		ActiveDialogue: [
			"Make this place look like it intends to stay.",
			"Build beyond tonight."
		],
		CompletionDialogue: [
			"Now this has weight.",
			"It looks like tomorrow matters here."
		]
	);

	public static readonly QuestDefinition HearthAgainstTheAsh = new(
		Title: "Hearth Against the Ash",
		Description: "Craft a fireplace so the camp can hold against the ash-laden nights.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Craft a Fireplace", RequiredCount: 1, ItemId: ItemID.FirePlace),
		],
		NpcId: NPCID.Mara,
		Prerequisites: [QuestID.MaraBuildForTomorrow],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"A house without fire is a tomb waiting to be claimed.",
			"The nights here kill softly.",
			"Build a fireplace.",
			"Stone-lined.",
			"Contained.",
			"Radiation weakens when heat remains constant."
		],
		ActiveDialogue: [
			"Fire drives more than cold away.",
			"It reminds people they are still alive."
		],
		CompletionDialogue: [
			"Good.",
			"This place will hold.",
			"With shelter, warmth, and preparation, we might last long enough for your device to matter.",
			"And if it does not...",
			"Then at least we will burn in defiance, not ignorance."
		]
	);

	public static readonly QuestDefinition ThePriceOfKnowing = new(
		Title: "The Price of Knowing",
		Description: "Bring David 3 gold bars to buy his first truth.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Bring David 3 Gold Bars", RequiredCount: 3, ItemId: ItemID.GoldBar),
		],
		NpcId: NPCID.David,
		InitialDialogue: [
			"You do not look lost.",
			"You look curious. Curiosity is expensive.",
			"I deal in information. Real information. The kind people kill for.",
			"If you want the first truth, bring me three gold bars.",
			"Not tomorrow. Not promises. Bars.",
			"Then I will tell you something useful."
		],
		ActiveDialogue: [
			"Three gold bars.",
			"I do not bargain."
		],
		CompletionDialogue: [
			"Good.",
			"Most people try to lie first.",
			"You did not.",
			"Here is your truth.",
			"There are radiation anomalies that are not spires.",
			"Tools. Weapons. Things the flare twisted instead of killing.",
			"If that interests you, it will cost more."
		]
	);

	public static readonly QuestDefinition MoreThanRumors = new(
		Title: "More Than Rumors",
		Description: "Bring David 3 more gold bars for the next layer of information.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Bring David 3 More Gold Bars", RequiredCount: 3, ItemId: ItemID.GoldBar),
		],
		NpcId: NPCID.David,
		Prerequisites: [QuestID.ThePriceOfKnowing],
		InitialDialogue: [
			"Information always comes in layers.",
			"If you want the second one, you already know the price.",
			"Three more gold bars.",
			"This truth is heavier."
		],
		ActiveDialogue: [
			"You cannot half pay for half a secret.",
			"Three gold bars."
		],
		CompletionDialogue: [
			"Good.",
			"Now listen carefully.",
			"Before the flare, some research bunkers were sealed automatically.",
			"One of them still exists.",
			"Inside is a weapon that feeds on radiation instead of flesh.",
			"A staff.",
			"If you are foolish or brave enough, I will tell you where."
		]
	);

	public static readonly QuestDefinition WhatTheFlameForgot = new(
		Title: "What the Flame Forgot",
		Description: "Retrieve the Radiation Staff from the buried bunker.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Retrieve the Radiation Staff from the bunker", RequiredCount: 1, ItemId: ItemID.RadiationStaff),
		],
		NpcId: NPCID.David,
		Prerequisites: [QuestID.MoreThanRumors],
		InitialDialogue: [
			"The bunker is buried east of here.",
			"Concrete shell. Manual lock. Radiation thick enough to sting.",
			"The staff is inside.",
			"It will not feel safe in your hands.",
			"If you bring it back, I will know you are worth trusting."
		],
		ActiveDialogue: [
			"The staff does not kill quickly.",
			"It erodes.",
			"Keep that in mind when you decide how to use it."
		],
		CompletionDialogue: [
			"You came back alive.",
			"And you brought it.",
			"That staff has killed more slowly than any blade ever could.",
			"You did not sell it.",
			"You did not run.",
			"That tells me more than gold ever could."
		]
	);

	public static readonly QuestDefinition AManWorthFollowing = new(
		Title: "A Man Worth Following",
		Description: "Talk to David and bring him to your camp.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new TalkObjective("Talk to David", NPCID.David),
		],
		NpcId: NPCID.David,
		Prerequisites: [QuestID.WhatTheFlameForgot],
		InitialDialogue: [
			"I do not follow people lightly.",
			"But you survive. And you listen.",
			"I will come with you.",
			"Put me somewhere quiet.",
			"I work best where knowledge can breathe."
		],
		ActiveDialogue: [
			"Put me somewhere quiet.",
			"I work best where knowledge can breathe."
		],
		CompletionDialogue: [
			"I will come with you.",
			"Put me somewhere quiet."
		]
	);

	public static readonly QuestDefinition DavidQuietFoundations = new(
		Title: "Quiet Foundations",
		Description: "Raise David's assigned house value to support focused planning and rest.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new StructureValueObjective(
				"Raise David's house value",
				ValueRequired: HighHouseValueRequirement
			),
		],
		NpcId: NPCID.David,
		Prerequisites: [QuestID.AManWorthFollowing],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"I can function like this.",
			"I would rather not.",
			"A better house changes how the mind rests.",
			"A big bed helped.",
			"But solid walls, real furnishings, quiet corners?",
			"That is where planning becomes possible instead of defensive."
		],
		ActiveDialogue: [
			"Give me quiet corners and solid walls.",
			"Planning needs more than survival."
		],
		CompletionDialogue: [
			"Now this is usable.",
			"I can think ahead here."
		]
	);

	public static readonly QuestDefinition RestWithoutFear = new(
		Title: "Rest Without Fear",
		Description: "Craft a big bed for David so camp feels like somewhere worth staying.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Craft a Big Bed", RequiredCount: 1, ItemId: ItemID.BedBig),
		],
		NpcId: NPCID.David,
		Prerequisites: [QuestID.DavidQuietFoundations],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"I have not slept properly in years.",
			"Every place I stayed expected me to run.",
			"Build a big bed.",
			"Something meant for staying.",
			"If I am going to help you, I need rest that does not feel temporary."
		],
		ActiveDialogue: [
			"A real bed changes how a man thinks."
		],
		CompletionDialogue: [
			"Good.",
			"This is the first time in a long while that I do not feel the need to leave before morning.",
			"Now we can plan properly."
		]
	);

	public static readonly QuestDefinition AWarmDrinkFirst = new(
		Title: "A Warm Drink First",
		Description: "Bring Chloe apple cider before she agrees to talk.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Bring Apple Cider", RequiredCount: 1, ItemId: ItemID.AppleCider),
		],
		NpcId: NPCID.Chloe,
		InitialDialogue: [
			"Wait. Hold up. You smell like road dust and bad decisions.",
			"If you want to talk, bring apple cider.",
			"Not because I am picky.",
			"Because hot, sweet things keep people honest out here."
		],
		ActiveDialogue: [
			"Apple cider.",
			"If it tastes burned, do not bother coming back."
		],
		CompletionDialogue: [
			"That is better.",
			"I almost forgot what this smelled like.",
			"Alright. You have my attention now."
		]
	);

	public static readonly QuestDefinition SomethingStronger = new(
		Title: "Something Stronger",
		Description: "Bring Chloe coffee so she can stay sharp while sharing what she knows.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new CollectObjective("Bring Coffee", RequiredCount: 1, ItemId: ItemID.Coffee),
		],
		NpcId: NPCID.Chloe,
		Prerequisites: [QuestID.AWarmDrinkFirst],
		InitialDialogue: [
			"Cider softens the edges.",
			"Coffee sharpens them.",
			"If we are going to talk about danger, I need to stay awake.",
			"Bring me coffee."
		],
		ActiveDialogue: [
			"Coffee. Real beans if you can manage it."
		],
		CompletionDialogue: [
			"Good.",
			"Now listen carefully."
		]
	);

	public static readonly QuestDefinition ThingsThatGlowBack = new(
		Title: "Things That Glow Back",
		Description: "Kill a Radiation Caster and learn how to fight what radiation has changed.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new KillObjective("Kill a Radiation Caster enemy", RequiredCount: 1, EnemyType: EnemyType.RadiationCaster),
		],
		NpcId: NPCID.Chloe,
		Prerequisites: [QuestID.SomethingStronger],
		InitialDialogue: [
			"You have probably noticed enemies that linger near the spires.",
			"The ones that do not flee.",
			"They are changed.",
			"We call them radiation casters.",
			"They do not just survive radiation.",
			"They throw it.",
			"If you plan to live long enough to finish your mission, you need to know how they die.",
			"Go kill one.",
			"Then come back alive."
		],
		ActiveDialogue: [
			"Do not get close.",
			"They want you slow."
		],
		CompletionDialogue: [
			"So they bleed just like everything else.",
			"Good.",
			"That means they are not inevitable."
		]
	);

	public static readonly QuestDefinition SomeoneWorthKeepingClose = new(
		Title: "Someone Worth Keeping Close",
		Description: "Talk to Chloe and bring her into your camp's warning line.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new TalkObjective("Talk to Chloe", NPCID.Chloe),
		],
		NpcId: NPCID.Chloe,
		Prerequisites: [QuestID.ThingsThatGlowBack],
		InitialDialogue: [
			"You listen.",
			"You prepare.",
			"And you come back when you say you will.",
			"I can work with that.",
			"I will come with you.",
			"Put me somewhere warm.",
			"I see threats better when I am not shivering."
		],
		ActiveDialogue: [
			"Put me somewhere warm.",
			"I see threats better when I am not shivering."
		],
		CompletionDialogue: [
			"I will come with you.",
			"Put me somewhere warm."
		]
	);

	public static readonly QuestDefinition ChloeWarmAndLivedIn = new(
		Title: "Warm and Lived In",
		Description: "Raise Chloe's assigned house value so camp feels warm and lived in.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new StructureValueObjective(
				"Raise Chloe's house value",
				ValueRequired: LowHouseValueRequirement
			),
		],
		NpcId: NPCID.Chloe,
		Prerequisites: [QuestID.SomeoneWorthKeepingClose],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"It gets cold in ways heat does not always fix.",
			"A chair that does not wobble would be nice.",
			"So would walls that keep the wind from sounding like footsteps.",
			"I do not need much.",
			"Just something that feels lived in, not abandoned."
		],
		ActiveDialogue: [
			"Warmth matters.",
			"Make this place feel lived in."
		],
		CompletionDialogue: [
			"This feels better.",
			"Still rough, but not abandoned."
		]
	);

	public static readonly QuestDefinition ProvingGround = new(
		Title: "Proving Ground",
		Description: "Defeat a Meldoran Warrior to prove your camp is not prey.",
		Type: QuestType.Side,
		StageRequirement: 0,
		Objectives: [
			new KillObjective("Defeat a Meldoran Warrior", RequiredCount: 1, EnemyType: EnemyType.MeldoranWarrior),
		],
		NpcId: NPCID.Chloe,
		Prerequisites: [QuestID.ChloeWarmAndLivedIn],
		OfferMode: QuestOfferMode.OfferedByNpc,
		InitialDialogue: [
			"Meldoran scouts have been getting bolder.",
			"They are watching camps now. Testing.",
			"Go take one of them out.",
			"Not because you have to.",
			"But because they need to know you are not prey."
		],
		ActiveDialogue: [
			"Do not let them posture.",
			"They fold when pressed hard."
		],
		CompletionDialogue: [
			"Good.",
			"They will remember that.",
			"And if they do not, the next one will."
		]
	);

	public static readonly Dictionary<QuestID, QuestDefinition> All = new() {
		[QuestID.LeftForDead] = LeftForDead,
		[QuestID.ArmYourself] = ArmYourself,
		[QuestID.ArmYourselfSide] = ArmYourselfSide,
		[QuestID.ADealIsADeal] = ADealIsADeal,
		[QuestID.RowanJoinsCamp] = RowanJoinsCamp,
		[QuestID.RowanHouseLogistics] = RowanHouseLogistics,
		[QuestID.RowanStockTheShelter] = RowanStockTheShelter,
		[QuestID.ColinRadiationCamp] = ColinRadiationCamp,
		[QuestID.ColinFollowToCamp] = ColinFollowToCamp,
		[QuestID.ColinRecoveryStandards] = ColinRecoveryStandards,
		[QuestID.ColinCraftBed] = ColinCraftBed,
		[QuestID.GildedProof] = GildedProof,
		[QuestID.BladeOfLegacy] = BladeOfLegacy,
		[QuestID.FoundationsThatLast] = FoundationsThatLast,
		[QuestID.MaraJoinsTheCamp] = MaraJoinsTheCamp,
		[QuestID.MaraBuildForTomorrow] = MaraBuildForTomorrow,
		[QuestID.HearthAgainstTheAsh] = HearthAgainstTheAsh,
		[QuestID.ThePriceOfKnowing] = ThePriceOfKnowing,
		[QuestID.MoreThanRumors] = MoreThanRumors,
		[QuestID.WhatTheFlameForgot] = WhatTheFlameForgot,
		[QuestID.AManWorthFollowing] = AManWorthFollowing,
		[QuestID.DavidQuietFoundations] = DavidQuietFoundations,
		[QuestID.RestWithoutFear] = RestWithoutFear,
		[QuestID.AWarmDrinkFirst] = AWarmDrinkFirst,
		[QuestID.SomethingStronger] = SomethingStronger,
		[QuestID.ThingsThatGlowBack] = ThingsThatGlowBack,
		[QuestID.SomeoneWorthKeepingClose] = SomeoneWorthKeepingClose,
		[QuestID.ChloeWarmAndLivedIn] = ChloeWarmAndLivedIn,
		[QuestID.ProvingGround] = ProvingGround,
	};
}
