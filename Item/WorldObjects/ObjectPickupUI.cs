namespace Objects;

using System;
using Godot;
using ItemSystem;
using Services;

public sealed class ObjectPickupUI {
	private static readonly LogService Log = new(nameof(ObjectPickupUI), enabled: true);
	private readonly ObjectPickup ObjectPickup;
	private readonly PackedScene? ObjectPickupUINodeTemplate;
	private readonly Vector3 PromptOffset;
	private Node3D? CurrentPromptInstance;

	public ObjectPickupUI(ObjectPickup objectPickup, Vector3? promptOffset = null) {
		ObjectPickup = objectPickup;
		ObjectPickupUINodeTemplate = GD.Load<PackedScene>("res://Item/WorldObjects/ObjectPickupUINodeTemplate.tscn");
		if(promptOffset != null) {
			PromptOffset = promptOffset.Value;
		}
		else {
			PromptOffset = new Vector3(0f, 1.35f, 0f);
		}

		ObjectPickup.AddedTargetObjectNode += HandleAddedTargetObjectNode;
		ObjectPickup.RemovedTargetObjectNode += HandleRemovedTargetObjectNode;

		if(ObjectPickup.currentTargetObjectNode != null) {
			Show(ObjectPickup.currentTargetObjectNode);
		}
	}

	public void Dispose() {
		ObjectPickup.AddedTargetObjectNode -= HandleAddedTargetObjectNode;
		ObjectPickup.RemovedTargetObjectNode -= HandleRemovedTargetObjectNode;
		Hide();
	}

	private void HandleAddedTargetObjectNode(ObjectNode objectNode) => Show(objectNode);

	private void HandleRemovedTargetObjectNode(ObjectNode objectNode) => Hide();

	public void Show(ObjectNode objectNode) {
		Hide();

		if(ObjectPickupUINodeTemplate == null) {
			Log.Error("Show: ObjectPickupUINodeTemplate is null.");
			return;
		}

		ItemDefinition? itemDef = ItemDataBaseManager.Instance.GetItemDefinitionById(objectNode.Data.ItemId);
		if(itemDef == null) {
			Log.Error($"Show: ItemDefinition not found for ID {objectNode.Data.ItemId}.");
			return;
		}

		Node3D promptNode = ObjectPickupUINodeTemplate.Instantiate<Node3D>();
		Control promptControl = promptNode.GetNode<Control>("SubViewport/Item3DIconPickupPrompt");
		promptControl.GetNode<Label>("GlassPanel/Label").Text = itemDef.Name;
		promptControl.GetNode<TextureRect>("GlassPanel/TextureRect").Texture = itemDef.IconTexture;

		objectNode.AddChild(promptNode);
		promptNode.Position = PromptOffset;
		CurrentPromptInstance = promptNode;
	}

	public void Hide() {
		if(CurrentPromptInstance == null) {
			return;
		}
		if(GodotObject.IsInstanceValid(CurrentPromptInstance)) {
			CurrentPromptInstance.QueueFree();
		}
		CurrentPromptInstance = null;
	}
}
