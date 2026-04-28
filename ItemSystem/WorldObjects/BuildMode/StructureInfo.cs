namespace UI.HUD;

using Character;
using GameWorld;
using Godot;
using ItemSystem.WorldObjects;
using Root;

public sealed partial class StructureInfo : Control {
	private Label? NameLabel;
	private Label? NpcLabel;
	private Label? ValueLabel;
	private BuildModeController? BoundBuildMode;
	private bool IsBound = false;

	public override void _Ready() {
		base._Ready();
		NameLabel = GetNodeOrNull<Label>("NameLabel");
		ValueLabel = GetNodeOrNull<Label>("ValueLabel");
		NpcLabel = GetNodeOrNull<Label>("NpcLabel");
	}

	public void Bind(BuildModeController buildModeController) {
		if(IsBound) {
			return;
		}
		BoundBuildMode = buildModeController;
		//buildModeController.StructureInfoRefreshRequested += OnStructureInfoRefreshRequested;
		IsBound = true;
	}


	public override void _ExitTree() {
		if(BoundBuildMode != null) {
			//BoundBuildMode.StructureInfoRefreshRequested -= OnStructureInfoRefreshRequested;
			BoundBuildMode = null;
			IsBound = false;
		}
		base._ExitTree();
	}	

	private void OnStructureInfoRefreshRequested(string name, string npc, int value) {
		if(NameLabel == null || NpcLabel == null || ValueLabel == null) {
			return;
		}
		NameLabel.Text = $"Name: {name}";
		NpcLabel.Text = $"NPC attached: {npc}";
		ValueLabel.Text = $"Total value: {value}";
	}
}
