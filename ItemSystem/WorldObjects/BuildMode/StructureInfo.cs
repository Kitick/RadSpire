namespace UI.HUD;

using Godot;
using ItemSystem.WorldObjects.House;
using Root;

public sealed partial class StructureInfo : Control {
	[Export] private Label NameLabel = null!;
	[Export] private Label NpcLabel = null!;
	[Export] private Label ValueLabel = null!;

	private GameWorldManager? BoundGameWorldManager;
	private bool IsBound = false;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
	}

	public void Bind(GameWorldManager gameWorldManager) {
		if(IsBound) { return; }
		BoundGameWorldManager = gameWorldManager;
		BoundGameWorldManager.StructureInfoRefreshRequested += OnStructureInfoRefreshRequested;
		BoundGameWorldManager.RequestStructureInfoRefresh();
		IsBound = true;
	}

	public override void _ExitTree() {
		if(BoundGameWorldManager != null) {
			BoundGameWorldManager.StructureInfoRefreshRequested -= OnStructureInfoRefreshRequested;
			BoundGameWorldManager = null;
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
