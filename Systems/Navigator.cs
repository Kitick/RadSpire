namespace Services {
	using System;
	using Godot;

	public interface INavigatable {
		Control[] Order { get; }
	}

	public sealed class Navigator {
		public static readonly LogService Log = new(nameof(InputSystem), enabled: true);

		public static Navigator Instance { get; private set; } = null!;

		public INavigatable? UIPanel = null;

		public Control Selected => UIPanel!.Order[SelectedIndex];

		public int SelectedIndex {
			get;
			private set => field = value % UIPanel!.Order.Length;
		} = 0;

		public Navigator() {
			Instance = this;
			SetActions();
		}

		private void SetActions() {
			ActionEvent.MenuSelect.WhenPressed(() => {
				if(Selected is Button button) {
					button.EmitSignal("pressed");
				}
			});

			ActionEvent.MenuUp.WhenPressed(() => {
				SelectedIndex--;
				UpdateSelected();
			});

			ActionEvent.MenuDown.WhenPressed(() => {
				SelectedIndex++;
				UpdateSelected();
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

		private void ClearStyles() {
			foreach(var control in UIPanel!.Order) {
				   // Reset visual style for all controls
				   control.SelfModulate = Colors.White;
			}
		}

		public void UpdateSelected() {
			if(UIPanel is null) { return; }
			ClearStyles();

			   // Highlight the selected control
			   var selected = Selected;
			   selected.SelfModulate = new Color(1f, 1f, 0.3f); // yellow highlight
		}
	}
}