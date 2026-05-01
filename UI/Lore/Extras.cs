namespace UI;

using Godot;

public sealed partial class Extras : BaseUIControl {
	private Button BackButton = null!;

	protected override Control? DefaultFocus => BackButton;

	public override void _Ready() {
		base._Ready();
		ProcessMode = ProcessModeEnum.Always;

		BackButton = GetNode<Button>("BackButton");
		BackButton.Pressed += CloseMenu;
	}

	public override void _ExitTree() {
		base._ExitTree();
		if(IsInstanceValid(BackButton)) {
			BackButton.Pressed -= CloseMenu;
		}
	}

	protected override bool OnCancel() {
		CloseMenu();
		return true;
	}

	public void OpenMenu() => OnOpen();

	private void CloseMenu() => QueueFree();
}
