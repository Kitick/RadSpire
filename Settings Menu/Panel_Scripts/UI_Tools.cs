using System;
using Godot;

namespace SettingsPanels {
	public static class UITools {
		public static void Populate<T>(this OptionButton button, T[] values) where T : notnull {
			button.Clear();
			foreach(var value in values) {
				button.AddItem(value.ToString());
			}
		}

		public static bool Select<T>(this OptionButton button, T value) where T : notnull {
			string target = value.ToString()!;
			int count = button.GetItemCount();

			for(int i = 0; i < count; i++) {
				if(button.GetItemText(i) == target) {
					button.Select(i);
					return true;
				}
			}
			return false;
		}
	}
}