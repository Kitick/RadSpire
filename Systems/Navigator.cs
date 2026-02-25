namespace Services {
	using System;
	using Godot;

	public interface INavigatable {
		Control[] Order { get; }
	}

	public sealed partial class Navigator : Node {
		public static readonly LogService Log = new(nameof(InputSystem), enabled: true);

		public static Navigator Instance { get; private set; } = null!;

		public INavigatable? UIPanel { get; private set; } = null;

		public Control Selected => UIPanel!.Order[SelectedIndex];

		public int SelectedIndex {
			get;
			private set {
				Selected.EmitSignal("mouse_exited");
				ClearStyles(Selected);

				field = WrapIndex(value);

				StyleSelected(Selected);
				Selected.EmitSignal("mouse_entered");
			}
		} = 0;

		private int WrapIndex(int index) {
			int count = UIPanel!.Order.Length;
			return (index % count + count) % count;
		}

		public override void _Ready() {
			Instance = this;
			SetActions();
		}

		public void SetPanel(INavigatable panel) {
			if(UIPanel != null){
				ClearStyles(UIPanel);
			}

			UIPanel = panel;
			SelectedIndex = 0;

			StyleSelected(Selected);
		}

		private void SetActions() {
			ActionEvent.MenuSelect.WhenPressed(() => {
				if(Selected is Button button) {
					button.EmitSignal("pressed");
				}
			});

			ActionEvent.MenuUp.WhenPressed(() => {
				SelectedIndex--;
			});

			ActionEvent.MenuDown.WhenPressed(() => {
				SelectedIndex++;
			});

			ActionEvent.MenuLeft.WhenPressed(() => {
				if(Selected is HSlider slider) {
					slider.Value -= slider.Step;
				}
			});

			ActionEvent.MenuRight.WhenPressed(() => {
				if(Selected is HSlider slider) {
					slider.Value += slider.Step;
				}
			});

			// other actions
		}

		// Reset visual style for all controls
		private static void ClearStyles(INavigatable panel) {
			foreach(var control in panel.Order) {
				ClearStyles(control);
			}
		}

		private static void ClearStyles(Control control) {
			control.SelfModulate = Colors.White;
		}

		// Highlight the selected control
		private static void StyleSelected(Control node) {
			node.SelfModulate = new Color(1f, 1f, 0.3f); // yellow highlight
		}
	}
}