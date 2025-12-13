//This file was developed entirely by the RadSpire Development Team.

using System;
using Godot;
using InputSystem;
using SaveSystem;

public enum SaveLoadMode { Save, Load }

public sealed partial class SaveLoadMenu : Control {
	private static readonly Logger Log = new(nameof(SaveLoadMenu), enabled: true);

	public event Action? OnMenuClosed;
	private event Action? OnExit;

	private const string BACK_BUTTON = "BackButton";
	private const string CONTAINER = "Panel/SaveSlots";
	private const string TITLE_LABEL = "Panel/Title";

	public static string SlotFile(int slot) => $"slot{slot}";

	private const int SLOTS = 5;

	private Button[] Buttons = new Button[SLOTS];
	private Label? TitleLabel;

	public SaveLoadMode Mode { get; set; } = SaveLoadMode.Load;

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;

		SetInputCallbacks();
		GetComponents();
		SetCallbacks();
		UpdateTitle();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		OnMenuClosed?.Invoke();
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
		OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
	}

	private void GetComponents() {
		TitleLabel = GetNodeOrNull<Label>(TITLE_LABEL);

		for(int i = 0; i < SLOTS; i++) {
			int slot = i + 1;
			Buttons[i] = GetNode<Button>($"{CONTAINER}/Slot{slot}");
			Buttons[i].Pressed += () => OnSlotPressed(slot);
		}

		RefreshSlotDisplay();
	}

	private void RefreshSlotDisplay() {
		for(int i = 0; i < SLOTS; i++) {
			int slot = i + 1;
			bool exists = SaveService.Exists(SlotFile(slot));
			Buttons[i].Text = exists ? $"Slot {slot}" : "Empty";
		}
	}

	private void UpdateTitle() {
		if(TitleLabel != null) {
			TitleLabel.Text = Mode == SaveLoadMode.Save ? "Save Game" : "Load Game";
		}
	}

	private void SetCallbacks() {
		GetNode<Button>(BACK_BUTTON).Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed() {
		CloseMenu();
	}

	public void OpenMenu() {
		UpdateTitle();
		RefreshSlotDisplay();
	}

	private void CloseMenu() {
		QueueFree();
	}

	private void OnSlotPressed(int slot) {
		string fileName = SlotFile(slot);

		if(Mode == SaveLoadMode.Save) {
			Log.Info($"Saving game to {fileName}");
			if(GameManager.Instance.SaveGame(fileName)) {
				RefreshSlotDisplay();
				CloseMenu();
			}
			else {
				Log.Error($"Failed to save game to {fileName}");
			}
		}
		else { // Load mode
			if(!SaveService.Exists(fileName)) {
				Log.Warn($"No save file exists at {fileName}");
				return;
			}

			GameManager.Instance.LoadGame(fileName);
			CloseMenu();
		}
	}
}