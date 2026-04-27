namespace UI;

using Godot;
using Services;

public abstract partial class BaseUIControl : Control {

	protected virtual Button? DefaultFocus => null;

	public override void _Ready() {
		ApplyHoverFocus(this);
		InputSystem.Instance.OnInputModeChanged += OnInputModeChanged;
	}

	public override void _ExitTree() {
		InputSystem.Instance.OnInputModeChanged -= OnInputModeChanged;
	}

	public override void _Input(InputEvent @event) {
		if(@event.IsActionPressed("ui_cancel") && OnCancel()) {
			GetViewport().SetInputAsHandled();
		}
	}

	// Return true to consume the event, false to let it propagate.
	protected virtual bool OnCancel() => false;

	protected void OnOpen() {
		if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
			DefaultFocus?.GrabFocus();
		}
	}

	private void OnInputModeChanged(InputSystem.InputMode mode) {
		if(!IsInsideTree()) { return; }
		if(mode == InputSystem.InputMode.MouseKeyboard) {
			GetViewport().GuiReleaseFocus();
		}
		else {
			if(GetViewport().GuiGetFocusOwner() == null) {
				DefaultFocus?.GrabFocus();
			}
		}
	}

	protected void ApplyHoverFocus(Control root) {
		foreach(Node child in root.GetChildren()) {
			if(child is Control control) {
				if(control.FocusMode != FocusModeEnum.None) {
					control.MouseEntered += () => {
						if(InputSystem.Instance.CurrentInputMode == InputSystem.InputMode.Controller) {
							control.GrabFocus();
						}
					};
				}

				ApplyHoverFocus(control);
			}
		}
	}
}
