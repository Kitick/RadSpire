namespace QuestSystem;

using Godot;
using Root;

public abstract record QuestObjective(string Description, int RequiredCount = 1);

public sealed record KillObjective(string Description, int RequiredCount, StringName EnemyGroup = default!)
	: QuestObjective(Description, RequiredCount);

public sealed record CollectObjective(string Description, int RequiredCount, string ItemId)
	: QuestObjective(Description, RequiredCount);

public sealed record LocationObjective(string Description, StringName LocationId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record TalkObjective(string Description, StringName NpcId)
	: QuestObjective(Description, RequiredCount: 1);

public sealed record QuestDefinition(
	string Title,
	string Description,
	QuestType Type,
	int StageRequirement,
	QuestObjective[] Objectives,
	StringName? NpcId = null,
	string[] InitialDialogue = null!,
	string[] ActiveDialogue = null!,
	string[] CompletionDialogue = null!
);

public static class Quests {
	public static readonly QuestDefinition KillPatrol = new(
		Title: "Clear the Patrol",
		Description: "Defeat the enemies guarding the area.",
		Type: QuestType.Main,
		StageRequirement: 0,
		Objectives: [new KillObjective("Kill 3 enemies", RequiredCount: 3)]
	);

	public static readonly QuestDefinition LeftForDead = new(
		Title: "Left for Dead",
		Description: "Ambushed and stripped of your weapons, you need to find shelter and help.",
		Type: QuestType.Main,
		StageRequirement: 0,
		Objectives: [new LocationObjective("Find shelter", LocationID.OfficeBuilding.ToString())]
	);

	public static readonly QuestDefinition AFairTrade = new(
		Title: "A Fair Trade",
		Description: "Rescue Sera's friend Dag from the Warriors of Meldoran at the ruined gas station.",
		Type: QuestType.Main,
		StageRequirement: 1,
		Objectives: [
			new KillObjective("Defeat the Warriors of Meldoran at the gas station", RequiredCount: 5, EnemyGroup: Group.MeldoranWarrior.ToString()),
		],
		NpcId: NPCID.Sera.ToString(),
		InitialDialogue: [
			"Hey. Hey, over here. You look like you're about to fall over.",
			"Get inside. Those were Meldoran men that did this to you, weren't they? I can tell by the way they left you. They don't finish what they start.",
			"I'm not going to hurt you. My name is Sera. I'm a Wanderer with the League of Krones, posted out here for weeks mapping the area. Sit down before you fall down.",
			"I know this land better than anyone within ten miles. I know where to find what you'll need. But first — an Astra knight, out here alone? You're not on patrol. What are you looking for?",
			"The neutralizer. I've heard the rumor. The League has heard it too — something pre-flare, something that could pull the radiation back. Nobody knows if it's real or where it is, but people are moving because of it. The Meldoran included, which means you don't have much time to waste out here bleeding.",
			"But I have to ask something of you first, and I need you to hear me out.",
			"They took my friend. Dag. Same men who did this to you. They've had him for days now and I can't go there myself. I'm not built for it.",
			"But you are. Or you were, before they stripped you. Help me get him back, and I'll get you back on your feet. Materials, tools, everything I know. We can talk terms once you've stopped bleeding.",
		],
		ActiveDialogue: [
			"The gas station down the road. Four or five of them, maybe more. Get yourself armed first.",
			"Head into the woods for wood, pick up stones along the road, and check the craters for iron.",
			"Dag is stubborn. He would have done something stupid trying to get himself out by now. I just hope he's still in one piece.",
			"They're thugs, not soldiers — but they'll kill you just the same if you let them. Keep your guard up.",
		],
		CompletionDialogue: [
			"You actually did it. Both of you. I didn't let myself think too hard about what I'd do if you didn't come back.",
			"Dag — are you alright? Don't answer that, just sit down.",
			"You did well, Roland. Better than I had any right to hope when I found you bleeding in the road.",
			"Your order sent you out here looking for something. I hope you find it. You're not going to find it standing here.",
			"Go. And if our paths cross again, the debt goes the other way.",
		]
	);

	public static readonly QuestDefinition ArmYourself = new(
		Title: "Arm Yourself",
		Description: "Craft equipment with Sera's guidance before heading to the gas station.",
		Type: QuestType.Side,
		StageRequirement: 1,
		Objectives: [
			new CollectObjective("Craft a sword", RequiredCount: 1, ItemId: ItemID.SwordIron),
			new CollectObjective("Craft a helmet", RequiredCount: 1, ItemId: ItemID.HeadpieceIron),
			new CollectObjective("Craft a shield", RequiredCount: 1, ItemId: ItemID.ShieldIron),
		],
		NpcId: NPCID.Sera.ToString(),
		InitialDialogue: [],
		ActiveDialogue: [
			"Sword needs two iron bars, two sticks, and a stone. Iron ore comes from the craters — process it into chunks, smelt the chunks into bars, then you can work with it.",
			"Helmet is three iron bars. Same chain — ore from the craters, process to chunks, smelt to bars.",
			"Shield is easier: two wood and one iron bar. Get the wood from the tree line, you only need one bar for that.",
		],
		CompletionDialogue: [
			"That's better. You look less like a corpse now.",
			"I can't go with you. I wish I could. Just please be careful.",
		]
	);

	public static readonly QuestDefinition[] All = [KillPatrol, LeftForDead, AFairTrade, ArmYourself];
}
