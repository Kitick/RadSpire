namespace Character.Recruitment;

using System.Collections.Generic;
using GameWorld;
using Godot;
using ItemSystem.WorldObjects;
using QuestSystem;
using ItemSystem.WorldObjects.House;
public sealed partial class NPCRecruitmentManager : Node {
	private readonly Dictionary<string, RecruitableNPCController> ControllersByNpcId = [];

	private Player Player = null!;
	private QuestManager QuestManager = null!;
	private GameWorldManager GameWorldManager = null!;
	private RecruitableNPCController? CurrentFollowingController;
	private NPCManager? BoundNpcManager;

	public void Initialize(Player player, QuestManager questManager, GameWorldManager gameWorldManager) {
		Player = player;
		QuestManager = questManager;
		GameWorldManager = gameWorldManager;
		QuestManager.QuestCompleted += HandleQuestCompleted;
	}

	public override void _ExitTree() {
		if(QuestManager != null) {
			QuestManager.QuestCompleted -= HandleQuestCompleted;
		}
		UnbindNpcManager();
		base._ExitTree();
	}

	public void BindCurrentWorld(NPCManager? npcManager) {
		BindNpcManager(npcManager);

		ControllersByNpcId.Clear();
		CurrentFollowingController = null;
		if(BoundNpcManager == null) {
			return;
		}

		foreach(NPC npc in BoundNpcManager.NPCs.Values) {
			if(!RecruitableNPCProfiles.All.TryGetValue(npc.NpcIdentity, out RecruitableNPCProfile? profile)) {
				continue;
			}

			RecruitableNPCController controller = npc.GetNodeOrNull<RecruitableNPCController>("RecruitableNPCController") ?? new RecruitableNPCController {
				Name = "RecruitableNPCController",
			};

			if(controller.GetParent() == null) {
				npc.AddChild(controller);
			}

			controller.Initialize(npc, Player, QuestManager, profile);
			ControllersByNpcId[npc.Id] = controller;
			if(controller.State == RecruitableNPCState.Following) {
				CurrentFollowingController = controller;
			}
		}
	}

	private void BindNpcManager(NPCManager? npcManager) {
		if(ReferenceEquals(BoundNpcManager, npcManager)) {
			return;
		}
		UnbindNpcManager();
		BoundNpcManager = npcManager;
		if(BoundNpcManager != null) {
			BoundNpcManager.NPCRegistryChanged += HandleNpcRegistryChanged;
		}
	}

	private void UnbindNpcManager() {
		if(BoundNpcManager != null) {
			BoundNpcManager.NPCRegistryChanged -= HandleNpcRegistryChanged;
		}
		BoundNpcManager = null;
	}

	private void HandleNpcRegistryChanged() {
		BindCurrentWorld(BoundNpcManager);
	}

	public bool TryAssignFollowingNpc(Object structureObject) {
		if(CurrentFollowingController == null) {
			return false;
		}
		if(!structureObject.ComponentDictionary.Has<StructureComponent>()) {
			return false;
		}

		CurrentFollowingController.AssignToStructure(structureObject);
		StructureComponent structureComponent = structureObject.ComponentDictionary.Get<StructureComponent>();
		structureComponent.AddAttachedNPC(CurrentFollowingController.ControlledNpc);
		GameWorldManager.RequestStructureInfoRefresh();
		CurrentFollowingController = null;
		return true;
	}

	private void HandleQuestCompleted(Root.QuestID questId) {
		foreach(RecruitableNPCController controller in ControllersByNpcId.Values) {
			if(!controller.MatchesCollectionQuest(questId)) {
				continue;
			}

			controller.BeginFollowing();
			CurrentFollowingController = controller;
			return;
		}
	}
}
