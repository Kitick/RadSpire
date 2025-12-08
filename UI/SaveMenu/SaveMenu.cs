using System;
using Godot;
using InputSystem;
using SaveSystem;

public enum SaveMenuMode { Save, Load }

public sealed partial class SaveMenu : Control {
	private static readonly Logger Log = new(nameof(SaveMenu), enabled: true);

	// Intent events
	public event Action<string>? OnSave;
	public event Action<string>? OnLoad;

	private event Action? OnExit;

	private const string BACK_BUTTON = "BackButton";
	private const string CONTAINER = "Panel/SaveSlots";
	private const string TITLE_LABEL = "Panel/Title";

	public static string SlotFile(int slot) => $"slot{slot}";

	private const int SLOTS = 5;

	private Button[] Buttons = new Button[SLOTS];
	private Label? TitleLabel;

	public SaveMenuMode Mode { get; set; } = SaveMenuMode.Load;

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;

		SetInputCallbacks();
		GetComponents();
		SetCallbacks();
		UpdateTitle();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
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
			TitleLabel.Text = Mode == SaveMenuMode.Save ? "Save Game" : "Load Game";
		}
	}

	private void SetCallbacks() {
		GetNode<Button>(BACK_BUTTON).Pressed += CloseMenu;
	}

	public void OpenMenu(Action? onClose = null) {
		OnExit += onClose;
		UpdateTitle();
		RefreshSlotDisplay();
	}

	private void CloseMenu() {
		QueueFree();
	}

	private void OnSlotPressed(int slot) {
		string fileName = SlotFile(slot);

		if(Mode == SaveMenuMode.Save) {
			if(!SaveService.Exists(fileName)) {
				// TODO: Could add confirmation dialog for overwriting
			}
			Log.Info($"Save requested: {fileName}");
			OnSave?.Invoke(fileName);
			CloseMenu();
		}
		else {
			if(!SaveService.Exists(fileName)) {
				Log.Warn($"No save file exists at {fileName}");
				return;
			}
			Log.Info($"Load requested: {fileName}");
			OnLoad?.Invoke(fileName);
			CloseMenu();
		}
	}
}