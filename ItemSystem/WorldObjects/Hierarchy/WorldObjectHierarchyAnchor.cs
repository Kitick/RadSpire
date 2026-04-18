namespace ItemSystem.WorldObjects.Hierarchy;

using System;
using Godot;

[Tool]
[GlobalClass]
public partial class WorldObjectHierarchyAnchor : Node {
	[Export] public string AnchorId { get; set; } = string.Empty;

	public override void _EnterTree() {
		if(Engine.IsEditorHint() && string.IsNullOrWhiteSpace(AnchorId)) {
			AnchorId = Guid.NewGuid().ToString();
			NotifyPropertyListChanged();
		}
	}

	public override void _Ready() {
		if(!Engine.IsEditorHint() && string.IsNullOrWhiteSpace(AnchorId)) {
			GD.PushWarning($"WorldObjectHierarchyAnchor '{GetPath()}' has an empty AnchorId and may fall back to root parenting.");
		}
	}
}
