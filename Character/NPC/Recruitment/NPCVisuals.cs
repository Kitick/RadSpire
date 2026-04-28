namespace Character.Recruitment;

using Godot;

public static class NPCVisuals {
	public static void Apply(NPC npc, RecruitableNPCProfile profile) {
		if(npc == null || string.IsNullOrWhiteSpace(profile.ModelScenePath)) {
			return;
		}

		Node3D? visualRoot = npc.GetVisualRoot();
		if(visualRoot == null) {
			return;
		}

		PackedScene? scene = ResourceLoader.Load<PackedScene>(profile.ModelScenePath);
		if(scene?.Instantiate() is not Node3D visualInstance) {
			return;
		}

		foreach(Node child in visualRoot.GetChildren()) {
			child.QueueFree();
		}

		visualRoot.AddChild(visualInstance);
	}
}
