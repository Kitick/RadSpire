using System;
using Godot;

namespace Settings {
	public sealed partial class GeneralPanel : VBoxContainer {
		public static readonly bool Debug = false;

		// Node Paths


		// Main
		public override void _Ready() {
			
			GetComponents();
			SetCallbacks();
		}

		// Get Components
		private void GetComponents() {
			
		}

		// Set Callbacks
		private void SetCallbacks() {
			
		}
	}
}

namespace SaveSystem {
	public readonly record struct GeneralSettings : ISaveData {
		
	}
}