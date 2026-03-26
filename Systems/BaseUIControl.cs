namespace UI;

using Godot;

public interface IFocusable {
	void GrabFocus();
}

public class HoverService {
	public void HandleHover(IFocusable target) {
		target.GrabFocus();
	}
}

public abstract partial class BaseUIControl : Control, IFocusable {
	private HoverService _hoverService = new HoverService();

	public override void _Ready() {
		ApplyHoverFocus(this);
	}

	public override void _Input(InputEvent @event) {
		if(@event.IsActionPressed("ui_cancel")) {
			OnCancel();
			GetViewport().SetInputAsHandled();
		}
	}

	protected virtual void OnCancel() { }

	protected void ApplyHoverFocus(Control root) {
		foreach(Node child in root.GetChildren()) {
			if(child is Control control) {
				if(control.FocusMode != FocusModeEnum.None) {
					control.MouseEntered += () => _hoverService.HandleHover((IFocusable) control);
				}

				ApplyHoverFocus(control);
			}
		}
	}
}
