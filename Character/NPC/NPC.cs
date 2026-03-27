namespace Character;

using System;
using Components;
using Godot;
using Services;
using UI.HUD;

public sealed partial class NPC : CharacterBody3D {
	private static readonly LogService Log = new(nameof(NPC), enabled: true);

	[Export] private string NPCName = "Villager";
	[Export(PropertyHint.MultilineText)] private string Dialogue = "Craft a sword and defeat the guys at the gas station";

	private bool PlayerInRange;
	private Action? UnsubscribeInteract;
	private Node3D? Player;
	private HUD? Hud;

	public override void _Ready() {
		Hud = GetTree().Root.GetNodeOrNull<HUD>("SceneDirector/GameManager/HUD");
		SetupInteraction();
	}

	public override void _PhysicsProcess(double delta) {
		if(PlayerInRange && Player != null) {
			Vector3 direction = Player.GlobalPosition - GlobalPosition;
			direction.Y = 0;

			if(direction.LengthSquared() < 0.0001f)
				return;

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
		var interactionArea = GetNodeOrNull<InteractionArea>("InteractionArea");

		if(interactionArea == null) {
			Log.Error("NPC InteractionArea not found.");
			return;
		}

		interactionArea.OnBodyEnteredArea += HandleBodyEntered;
		interactionArea.OnBodyExitedArea += HandleBodyExited;

		UnsubscribeInteract = ActionEvent.Interact.WhenPressed(() => {
			if(!PlayerInRange) {
				return;
			}

			Interact();
		});
	}

	private void HandleBodyEntered(Node3D body) {
		if(body.IsInGroup("player")) {
			PlayerInRange = true;
			Player = body;

			Hud?.ShowInteractionPrompt("Press F to talk");

			Log.Info("Player entered NPC interaction range");
		}
	}

	private void HandleBodyExited(Node3D body) {
		if(body.IsInGroup("player")) {
			PlayerInRange = false;
			Player = null;

			Hud?.HideInteractionPrompt();

			Log.Info("Player left NPC interaction range");
		}
	}

	private void Interact() {
		Hud?.ShowInteractionPrompt($"{NPCName}: {Dialogue}");
	}
}
