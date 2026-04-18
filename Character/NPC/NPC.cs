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

	public event Action<NPCID>? Talked;
	public event Action<string?>? InteractionPromptChanged;

	private bool PlayerInRange;
	private event Action? OnExit;
	private Node3D? Player;
	private QuestManager? QuestManager;

	private string[] CurrentLines = [];
	private int CurrentLineIndex = 0;
	private bool InDialogue = false;

	public void Init(QuestManager questManager) => QuestManager = questManager;

	public override void _Ready() {
		if(Identity == NPCID.None) {
			Log.Error($"{Name}: Identity not assigned.");
			return;
		}
		SetupInteraction();
	}

	public override void _PhysicsProcess(double delta) {
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
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		ClearEvents();
	}

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
		InteractionPromptChanged?.Invoke("Press F to talk");
		Log.Info("Player entered NPC interaction range");
	}

	private void HandleBodyExited(Node3D body) {
		if(!body.IsInGroup(Group.Player.ToString())) { return; }
		PlayerInRange = false;
		Player = null;
		InDialogue = false;
		InteractionPromptChanged?.Invoke(null);
		Log.Info("Player left NPC interaction range");
	}

	private void Interact() {
		if(!InDialogue) {
			CurrentLines = QuestManager?.GetDialogueFor(Identity) ?? [];
			CurrentLineIndex = 0;
			InDialogue = CurrentLines.Length > 0;
			Talked?.Invoke(Identity);
			return;
		}

		if(CurrentLineIndex < CurrentLines.Length) {
			InteractionPromptChanged?.Invoke($"{Identity}: {CurrentLines[CurrentLineIndex]}");
			CurrentLineIndex++;
			return;
		}

		InteractionPromptChanged?.Invoke(null);
		InDialogue = false;
		QuestManager?.NotifyDialogueFinished(Identity);
	}

	public NPCData Export() => new NPCData {
		Id = Id,
		NPCName = NPCName,
		Dialogue = Dialogue,
		GlobalPosition = GlobalPosition,
		GlobalRotation = GlobalRotation,
	};

	public void Import(NPCData data) {
		if(!string.IsNullOrEmpty(data.Id)) {
			Id = data.Id;
		}
		NPCName = data.NPCName;
		Dialogue = data.Dialogue;
		GlobalPosition = data.GlobalPosition;
		GlobalRotation = data.GlobalRotation;
	}
}

public readonly record struct NPCData : ISaveData {
	public string Id { get; init; }
	public string NPCName { get; init; }
	public string Dialogue { get; init; }
	public Vector3 GlobalPosition { get; init; }
	public Vector3 GlobalRotation { get; init; }
}
