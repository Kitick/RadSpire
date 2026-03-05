using Godot;

public abstract partial class BaseUIControl : Control {
    
    public override void _Ready() {
        ApplyHoverFocus(this);
    }

    public override void _Input(InputEvent @event) {
        if(@event.IsActionPressed("ui_cancel")) {
            OnCancel();
            GetViewport().SetInputAsHandled();
        }
    }

    protected virtual void OnCancel() {
        
    }

    protected void ApplyHoverFocus(Control root) {
        foreach (Node child in root.GetChildren()) {
            if (child is Control control) {
                if(control.FocusMode != FocusModeEnum.None) {
                    control.MouseEntered += () => control.GrabFocus();
                }

                ApplyHoverFocus(control);
            }
        }
    }
}