namespace UI.HUD;

using Godot;
using ItemSystem.WorldObjects;
using Root;

public sealed partial class StructureInfo : Control {
	[Export] private Label NameLabel = null!;
	[Export] private Label NpcLabel = null!;
	[Export] private Label ValueLabel = null!;

	private BuildModeController? BoundBuildMode;
	private bool IsBound = false;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
	}

	public void Bind(BuildModeController buildModeController) {
		if(IsBound) { return; }
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
		NameLabel.Text = $"Name: {name}";
		NpcLabel.Text = $"NPC attached: {npc}";
		ValueLabel.Text = $"Total value: {value}";
	}
}
