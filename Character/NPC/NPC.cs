namespace Character;

using System;
using Components;
using Godot;
using QuestSystem;
using Root;
using Services;

public sealed partial class NPC : CharacterBody3D {
	private static readonly LogService Log = new(nameof(NPC), enabled: true);

	[Export] private NPCID Identity = NPCID.None;

	public event Action<NPCID>? Talked;
	public event Action<string?>? InteractionPromptChanged;

	private bool PlayerInRange;
	private Action? UnsubscribeInteract;
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
		UnsubscribeInteract?.Invoke();
	}

	private void SetupInteraction() {
		InteractionArea interactionArea = GetNodeOrNull<InteractionArea>("InteractionArea");

		if(interactionArea == null) {
			Log.Error("NPC InteractionArea not found.");
			return;
		}

		interactionArea.OnBodyEnteredArea += HandleBodyEntered;
		interactionArea.OnBodyExitedArea += HandleBodyExited;

		UnsubscribeInteract = ActionEvent.Interact.WhenPressed(() => {
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
}
