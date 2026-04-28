namespace Character.Recruitment;

using QuestSystem;

public sealed class NPCDialogueRouter {
	private readonly QuestManager QuestManager;
	private readonly RecruitableNPCProfile Profile;

	public NPCDialogueRouter(QuestManager questManager, RecruitableNPCProfile profile) {
		QuestManager = questManager;
		Profile = profile;
	}

	public string[] GetDialogue(RecruitableNPCState state) {
		return state switch {
			RecruitableNPCState.Locked => QuestManager.GetDialogueFor(Profile.NpcId),
			RecruitableNPCState.Following => Profile.FollowingDialogue,
			RecruitableNPCState.AssignedToStructure => GetAssignedDialogue(),
			RecruitableNPCState.IdleAtStructure => GetAssignedDialogue(),
			_ => QuestManager.GetDialogueFor(Profile.NpcId),
		};
	}

	public string[] OnDialogueFinished(RecruitableNPCState state) {
		if(state == RecruitableNPCState.IdleAtStructure || state == RecruitableNPCState.AssignedToStructure) {
			return OfferPostCollectionQuests();
		}

		return QuestManager.NotifyDialogueFinished(Profile.NpcId);
	}

	private string[] OfferPostCollectionQuests() {
		System.Collections.Generic.List<string> notifications = [];
		bool offeredAnyQuest = false;

		foreach(Root.QuestID questId in Profile.PostCollectionQuests) {
			if(QuestManager.OfferQuest(questId)) {
				QuestDefinition? definition = QuestManager.GetDefinition(questId);
				if(definition != null) {
					notifications.Add($"Quest Available: {definition.Title}");
				}
				offeredAnyQuest = true;
			}
		}

		if(offeredAnyQuest) {
			return [.. notifications];
		}

		return QuestManager.NotifyDialogueFinished(Profile.NpcId);
	}

	private string[] GetAssignedDialogue() {
		foreach(Root.QuestID questId in Profile.PostCollectionQuests) {
			string[] dialogue = QuestManager.GetDialogueForQuest(questId);
			if(dialogue.Length > 0) {
				return dialogue;
			}
		}

		return Profile.AssignedDialogue;
	}
}
