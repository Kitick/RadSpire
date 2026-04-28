namespace Character.Recruitment;

using Godot;
using ItemSystem.WorldObjects;
using QuestSystem;

public enum RecruitableNPCState {
	Locked,
	Recruitable,
	Following,
	AssignedToStructure,
	IdleAtStructure,
}

public sealed partial class RecruitableNPCController : Node {
	private NPC Npc = null!;
	private Player Player = null!;
	private QuestManager QuestManager = null!;
	private RecruitableNPCProfile Profile = null!;
	private NPCFollowMovement FollowMovement = null!;
	private NPCIdleWander IdleWander = null!;
	private NPCDialogueRouter DialogueRouter = null!;

	public RecruitableNPCState State { get; private set; }
	public string AssignedStructureObjectId { get; private set; } = string.Empty;
	public Vector3 HomePosition { get; private set; }
	public Vector3 HomeRotation { get; private set; }
	public NPC ControlledNpc => Npc;

	public void Initialize(NPC npc, Player player, QuestManager questManager, RecruitableNPCProfile profile) {
		Npc = npc;
		Player = player;
		QuestManager = questManager;
		Profile = profile;
		FollowMovement = new NPCFollowMovement(npc);
		IdleWander = new NPCIdleWander(npc);
		DialogueRouter = new NPCDialogueRouter(questManager, profile);

		Npc.SetDisplayName(profile.DisplayName);
		NPCVisuals.Apply(npc, profile);
		RestoreSavedState();
		RefreshStateFromQuestProgress();
	}

	public override void _PhysicsProcess(double delta) {
		switch(State) {
			case RecruitableNPCState.Following:
				FollowMovement.Update(delta, Player);
				break;
			case RecruitableNPCState.AssignedToStructure:
				State = RecruitableNPCState.IdleAtStructure;
				goto case RecruitableNPCState.IdleAtStructure;
			case RecruitableNPCState.IdleAtStructure:
				IdleWander.Update(delta);
				break;
			default:
				break;
		}
	}

	public bool MatchesCollectionQuest(Root.QuestID questId) => Profile.CollectionQuest == questId;

	public void RefreshStateFromQuestProgress() {
		if(State == RecruitableNPCState.AssignedToStructure || State == RecruitableNPCState.IdleAtStructure) {
			ApplyDialogueOverrides();
			return;
		}

		if(QuestManager.IsQuestCompleted(Profile.CollectionQuest)) {
			BeginFollowing();
			return;
		}

		bool prereqsMet = true;
		foreach(Root.QuestID prerequisite in Profile.PrereqQuests) {
			if(!QuestManager.IsQuestCompleted(prerequisite)) {
				prereqsMet = false;
				break;
			}
		}

		State = prereqsMet ? RecruitableNPCState.Recruitable : RecruitableNPCState.Locked;
		Npc.SetDefaultFacingEnabled(true);
		ApplyDialogueOverrides();
		SaveState();
	}

	public void BeginFollowing() {
		State = RecruitableNPCState.Following;
		AssignedStructureObjectId = string.Empty;
		HomePosition = Vector3.Zero;
		HomeRotation = Vector3.Zero;
		Npc.SetDefaultFacingEnabled(false);
		ApplyDialogueOverrides();
		SaveState();
	}

	public void AssignToStructure(Object structureObject) {
		AssignedStructureObjectId = structureObject.Id;
		HomePosition = structureObject.WorldLocation.Position + new Vector3(3.0f, 0f, 0f);
		HomeRotation = structureObject.WorldLocation.Rotation;
		Npc.GlobalPosition = HomePosition;
		Npc.GlobalRotation = HomeRotation;
		IdleWander.SetHome(HomePosition, HomeRotation);
		State = RecruitableNPCState.AssignedToStructure;
		Npc.SetDefaultFacingEnabled(false);
		ApplyDialogueOverrides();
		SaveState();
	}

	private void ApplyDialogueOverrides() {
		Npc.ConfigureDialogueOverrides(
			() => DialogueRouter.GetDialogue(State),
			() => DialogueRouter.OnDialogueFinished(State),
			GetPromptText
		);
	}

	private string? GetPromptText() {
		return State switch {
			RecruitableNPCState.Locked => "Press F to talk",
			RecruitableNPCState.Following => "Press F to talk",
			RecruitableNPCState.AssignedToStructure => "Press F to talk",
			RecruitableNPCState.IdleAtStructure => "Press F to talk",
			_ => "Press F to talk",
		};
	}

	private void RestoreSavedState() {
		(string stateId, string structureObjectId, Vector3 homePosition, Vector3 homeRotation) = Npc.GetRecruitmentSaveState();
		if(!System.Enum.TryParse(stateId, out RecruitableNPCState restoredState)) {
			return;
		}

		State = restoredState;
		AssignedStructureObjectId = structureObjectId;
		HomePosition = homePosition;
		HomeRotation = homeRotation;
		if(State == RecruitableNPCState.AssignedToStructure || State == RecruitableNPCState.IdleAtStructure) {
			if(HomePosition != Vector3.Zero) {
				Npc.GlobalPosition = HomePosition;
				Npc.GlobalRotation = HomeRotation;
			}
			IdleWander.SetHome(HomePosition == Vector3.Zero ? Npc.GlobalPosition : HomePosition, HomeRotation);
		}
	}

	private void SaveState() {
		Npc.SetRecruitmentSaveState(State.ToString(), AssignedStructureObjectId, HomePosition, HomeRotation);
	}
}
