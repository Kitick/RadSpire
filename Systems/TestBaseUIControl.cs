using Godot;
using System;

public partial class TestBaseUIControl : BaseUIControl {
    public bool CancelCalled = false;
    protected override void OnCancel() => CancelCalled = true;

    public void RunTests(SceneTree tree) {
        GD.Print("---- Start Unit Test: BaseUIControl ----");

        // Create a focusable button
        var button = new Button { FocusMode = Control.FocusModeEnum.All };
        AddChild(button);

        ApplyHoverFocus(this); // Manually call to set up hover focus

        // Simulate Mouse Hover
        button.EmitSignal(Control.SignalName.MouseEntered);
        bool focusGained = button.HasFocus();

        // Simulate Cancel Input
        var cancelEvent = new InputEventAction { Action = "ui_cancel", Pressed = true };
        _Input(cancelEvent);

        // Print results to Godot Ouput
        GD.Print($"Test Hover Focus: {(focusGained ? "Passed" : "Failed")}");
        GD.Print($"Test OnCancel: {(CancelCalled ? "Passed" : "Failed")}");
    }
}