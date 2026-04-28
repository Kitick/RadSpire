namespace Character;

using System;
using Components;
using Godot;
using QuestSystem;
using Root;
using Services;

public sealed partial class NPC : CharacterBody3D, ISaveable<NPCData> {
	private static readonly LogService Log = new(nameof(NPC), enabled: true);
	public string Id { get; set; } = Guid.NewGuid().ToString();

	[Export] private NPCID Identity = NPCID.None;
	[Export] private string NPCName = string.Empty;
	[Export(PropertyHint.MultilineText)] private string Dialogue = string.Empty;

	public event Action<NPCID>? Talked;
	public event Action<string?>? InteractionPromptChanged;

	[Export] private Label3D DialogueLabel = null!;

	private bool PlayerInRange;
	private event Action? OnExit;
	private Node3D? Player;
	private QuestManager? QuestManager;

	private string[] CurrentLines = [];
	private int CurrentLineIndex = 0;
	private bool InDialogue = false;
	private bool UseDefaultFacing = true;
	private Func<string[]>? DialogueProviderOverride;
	private Func<string[]>? DialogueFinishedOverride;
	private Func<string?>? PromptProviderOverride;
	private string RecruitmentStateId = string.Empty;
	private string AssignedStructureObjectId = string.Empty;
	private Vector3 AssignedHomePosition = Vector3.Zero;
	private Vector3 AssignedHomeRotation = Vector3.Zero;

	public NPCID NpcIdentity => Identity;
	public string DisplayName => string.IsNullOrWhiteSpace(NPCName) ? Identity.ToString() : NPCName;
	public Player? NearbyPlayer => Player as Player;

	public void Init(QuestManager questManager) => QuestManager = questManager;

	public void ConfigureIdentity(NPCID identity, string displayName = "") {
		Identity = identity;
		if(!string.IsNullOrWhiteSpace(displayName)) {
			NPCName = displayName;
		}
	}

	public override void _Ready() {
		this.ValidateExports();
		if(Identity == NPCID.None) {
			Log.Error($"{Name}: Identity not assigned.");
			return;
		}
		SetupInteraction();
	}

	private float IdleLookTarget;
	private float IdleLookTimer;
	private float IdleLookInterval;

	private const float IdleLookIntervalMin = 3f;
	private const float IdleLookIntervalMax = 8f;

	public override void _PhysicsProcess(double delta) {
		if(!UseDefaultFacing) {
			return;
		}

		if(PlayerInRange && Player != null) {
			Vector3 direction = Player.GlobalPosition - GlobalPosition;
			direction.Y = 0;

			if(direction.LengthSquared() < 0.0001f) { return; }

			float targetRotation = Mathf.Atan2(direction.X, direction.Z);
			Rotation = new Vector3(
				0,
				Mathf.LerpAngle(Rotation.Y, targetRotation, (float) delta * 5f),
				0
			);
		}
		else {
			IdleLookTimer -= (float) delta;
			if(IdleLookTimer <= 0f) {
				IdleLookTarget = (float) GD.RandRange(0, Mathf.Tau);
				IdleLookInterval = (float) GD.RandRange(IdleLookIntervalMin, IdleLookIntervalMax);
				IdleLookTimer = IdleLookInterval;
			}

			Rotation = new Vector3(
				0,
				Mathf.LerpAngle(Rotation.Y, IdleLookTarget, (float) delta * 2f),
				0
			);
		}
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		ClearEvents();
	}

	private void SetDialogue(string? text) {
		InteractionPromptChanged?.Invoke(text);
		DialogueLabel.Text = text ?? "";
		DialogueLabel.Visible = text != null;
	}

	public void SetDefaultFacingEnabled(bool enabled) {
		UseDefaultFacing = enabled;
		if(enabled) {
			Velocity = Vector3.Zero;
		}
	}

	public void ConfigureDialogueOverrides(
		Func<string[]>? dialogueProvider,
		Func<string[]>? dialogueFinished,
		Func<string?>? promptProvider = null
	) {
		DialogueProviderOverride = dialogueProvider;
		DialogueFinishedOverride = dialogueFinished;
		PromptProviderOverride = promptProvider;
		RefreshInteractionPrompt();
	}

	public void ClearDialogueOverrides() {
		DialogueProviderOverride = null;
		DialogueFinishedOverride = null;
		PromptProviderOverride = null;
		RefreshInteractionPrompt();
	}

	public void SetDisplayName(string name) {
		NPCName = name ?? string.Empty;
	}

	public Node3D? GetVisualRoot() => GetNodeOrNull<Node3D>("Mage");

	public void SetRecruitmentSaveState(string stateId, string structureObjectId, Vector3 homePosition, Vector3 homeRotation) {
		RecruitmentStateId = stateId ?? string.Empty;
		AssignedStructureObjectId = structureObjectId ?? string.Empty;
		AssignedHomePosition = homePosition;
		AssignedHomeRotation = homeRotation;
	}

	public (string StateId, string StructureObjectId, Vector3 HomePosition, Vector3 HomeRotation) GetRecruitmentSaveState() =>
		(RecruitmentStateId, AssignedStructureObjectId, AssignedHomePosition, AssignedHomeRotation);

	public void RefreshInteractionPrompt() {
		if(!PlayerInRange) {
			return;
		}

		SetDialogue(GetInteractionPrompt());
	}

	private string? GetInteractionPrompt() => PromptProviderOverride?.Invoke() ?? "Press F to talk";

	private void ClearEvents() {
		Talked = null;
		InteractionPromptChanged = null;
	}

	private void SetupInteraction() {
		InteractionArea interactionArea = GetNodeOrNull<InteractionArea>("InteractionArea");

		if(interactionArea == null) {
			Log.Error("NPC InteractionArea not found.");
			return;
		}

		interactionArea.OnBodyEnteredArea += HandleBodyEntered;
		interactionArea.OnBodyExitedArea += HandleBodyExited;

		OnExit += ActionEvent.Interact.WhenPressed(() => {
			if(!PlayerInRange) { return; }
			Interact();
		});
	}

	private void HandleBodyEntered(Node3D body) {
		if(!body.IsInGroup(Group.Player.ToString())) { return; }
		PlayerInRange = true;
		Player = body;
		SetDialogue(GetInteractionPrompt());
		Log.Info("Player entered NPC interaction range");
	}

	private void HandleBodyExited(Node3D body) {
		if(!body.IsInGroup(Group.Player.ToString())) { return; }
		PlayerInRange = false;
		Player = null;
		InDialogue = false;
		SetDialogue(null);
		Log.Info("Player left NPC interaction range");
	}

	private void Interact() {
		if(!InDialogue) {
			CurrentLines = DialogueProviderOverride?.Invoke() ?? QuestManager?.GetDialogueFor(Identity) ?? [];
			CurrentLineIndex = 0;
			InDialogue = CurrentLines.Length > 0;
			if(!InDialogue && DialogueProviderOverride != null) {
				SetDialogue(PlayerInRange ? GetInteractionPrompt() : null);
				return;
			}
		}

		if(InDialogue && CurrentLineIndex < CurrentLines.Length) {
			SetDialogue($"{Identity}: {CurrentLines[CurrentLineIndex]}");
			CurrentLineIndex++;
			return;
		}

		Talked?.Invoke(Identity);
		string[] notifications = DialogueFinishedOverride?.Invoke() ?? QuestManager?.NotifyDialogueFinished(Identity) ?? [];

		if(notifications.Length > 0) {
			CurrentLines = notifications;
			CurrentLineIndex = 0;
			InDialogue = true;
			SetDialogue(CurrentLines[CurrentLineIndex]);
			CurrentLineIndex++;
		}
		else {
			SetDialogue(PlayerInRange ? GetInteractionPrompt() : null);
			InDialogue = false;
		}
	}

	public NPCData Export() => new() {
		Id = Id,
		Identity = Identity,
		NPCName = NPCName,
		GlobalPosition = GlobalPosition,
		GlobalRotation = GlobalRotation,
		RecruitmentStateId = RecruitmentStateId,
		AssignedStructureObjectId = AssignedStructureObjectId,
		AssignedHomePosition = AssignedHomePosition,
		AssignedHomeRotation = AssignedHomeRotation,
	};

	public void Import(NPCData data) {
		if(!string.IsNullOrEmpty(data.Id)) {
			Id = data.Id;
		}
		Identity = data.Identity;
		NPCName = data.NPCName ?? string.Empty;
		GlobalPosition = data.GlobalPosition;
		GlobalRotation = data.GlobalRotation;
		RecruitmentStateId = data.RecruitmentStateId ?? string.Empty;
		AssignedStructureObjectId = data.AssignedStructureObjectId ?? string.Empty;
		AssignedHomePosition = data.AssignedHomePosition;
		AssignedHomeRotation = data.AssignedHomeRotation;
	}
}

public readonly record struct NPCData : ISaveData {
	public string Id { get; init; }
	public NPCID Identity { get; init; }
	public string NPCName { get; init; }
	public Vector3 GlobalPosition { get; init; }
	public Vector3 GlobalRotation { get; init; }
	public string RecruitmentStateId { get; init; }
	public string AssignedStructureObjectId { get; init; }
	public Vector3 AssignedHomePosition { get; init; }
	public Vector3 AssignedHomeRotation { get; init; }
}
