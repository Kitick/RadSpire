using System;
using Godot;
using Services;
using Components;

namespace Character {

	public sealed partial class NPC : CharacterBody3D {

		private static readonly LogService Log = new(nameof(NPC), enabled: true);

		[Export] private string NPCName = "Villager";
		[Export(PropertyHint.MultilineText)] private string Dialogue = "Hello there.";

		private bool PlayerInRange;
		private Action? UnsubscribeInteract;

		public override void _Ready() {
			SetupInteraction();
		}

		public override void _ExitTree() {
			UnsubscribeInteract?.Invoke();
		}

		private void SetupInteraction() {

			var interactionArea = GetNodeOrNull<InteractionArea>("InteractionArea");

			if (interactionArea == null) {
				Log.Error("NPC InteractionArea not found.");
				return;
			}

			interactionArea.OnBodyEnteredArea += HandleBodyEntered;
			interactionArea.OnBodyExitedArea += HandleBodyExited;

			UnsubscribeInteract = ActionEvent.Interact.WhenPressed(() => {

				if (!PlayerInRange) {
					return;
				}

				Interact();
			});
		}

		private void HandleBodyEntered(Node3D body) {

			if (body.IsInGroup("player")) {
				PlayerInRange = true;
				Log.Info("Player entered NPC interaction range");
			}
		}

		private void HandleBodyExited(Node3D body) {

			if (body.IsInGroup("player")) {
				PlayerInRange = false;
				Log.Info("Player left NPC interaction range");
			}
		}

		private void Interact() {
			GD.Print("Interaction worked");
			GD.Print($"{NPCName}: {Dialogue}");
		}
	}
}
