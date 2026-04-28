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
		base._ExitTree();
	}

	public void BindCurrentWorld(NPCManager? npcManager) {
		ControllersByNpcId.Clear();
		CurrentFollowingController = null;
		if(npcManager == null) {
			return;
		}

		foreach(NPC npc in npcManager.NPCs.Values) {
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
