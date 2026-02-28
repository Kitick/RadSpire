namespace Services {
	using System;
	using Godot;

	public sealed partial class Navigator : Node {
		public static readonly LogService Log = new(nameof(InputSystem), enabled: true);

		public static Navigator Instance { get; private set; } = null!;

		public Control[] Order {
			get;
			set {
				ClearStyles(Order);
				IsActive = false;
				field = value;
				SelectedIndex = 0;
			}
		} = [];

		public Control Selected => Order[SelectedIndex];

		public bool IsActive { get; private set; } = false;

		private int SelectedIndex {
			get;
			set {
				Selected.EmitSignal("mouse_exited");
				ClearStyles(Selected);

				field = WrapIndex(value);

				StyleSelected(Selected);
				Selected.EmitSignal("mouse_entered");
			}
		} = 0;

		private int WrapIndex(int index) {
			int count = Order.Length;
			return (index % count + count) % count;
		}

		public void Select(Control control) {
			int index = Array.IndexOf(Order, control);
			if(index >= 0) { SelectedIndex = index; }
		}

		public override void _Ready() {
			Instance = this;
			SetActions();
		}

		private void SetActions() {
			ActionEvent.MenuSelect.WhenPressed(() => {
				IsActive = true;
				if(Selected is Button button) {
					button.EmitSignal("pressed");
				}
			});

			ActionEvent.MenuUp.WhenPressed(() => {
				IsActive = true;
				SelectedIndex--;
			});

			ActionEvent.MenuDown.WhenPressed(() => {
				IsActive = true;
				SelectedIndex++;
			});

			ActionEvent.MenuLeft.WhenPressed(() => {
				IsActive = true;
				if(Selected is HSlider slider) {
					slider.Value -= slider.Step;
				}
			});

			ActionEvent.MenuRight.WhenPressed(() => {
				IsActive = true;
				if(Selected is HSlider slider) {
					slider.Value += slider.Step;
				}
			});

			// other actions
		}

		// Reset visual style for all controls
		private static void ClearStyles(Control[] panel) {
			foreach(var control in panel) {
				ClearStyles(control);
			}
		}

		private static void ClearStyles(Control control) {
			if(IsInstanceValid(control)) {
				control.SelfModulate = Colors.White;
			}
		}

		// Highlight the selected control
		private static void StyleSelected(Control node) {
			node.SelfModulate = new Color(1f, 1f, 0.3f); // yellow highlight
		}
	}
}