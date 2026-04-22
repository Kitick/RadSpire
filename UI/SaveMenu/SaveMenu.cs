namespace UI.SaveMenu;

using System;
using Godot;
using Root;
using Services;

public sealed partial class SaveMenu : Control {
	private static readonly LogService Log = new(nameof(SaveMenu), enabled: true);

	[ExportCategory("UI Elements")]
	[Export] private Button BackButton = null!;
	[Export] private Label TitleLabel = null!;
	[Export] private Container SlotContainer = null!;

	[ExportCategory("Slot Style")]
	[Export] private Texture2D? SlotIcon;
	[Export] private int SlotFontSize = 48;

	private const int SlotCount = 5;

	public enum SaveMode { Save, Load }

	public event Action<string>? OnSave;
	public event Action<string>? OnLoad;

	private event Action? OnExit;

	private readonly Button[] SlotButtons = new Button[SlotCount];

	public SaveMode Mode { get; private set; }

	public static string SlotFile(int slot) => $"slot{slot}";

	public override void _Ready() {
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.Always;

		GenerateSlots();
		SetInputCallbacks();
		SetCallbacks();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
	}

	private void GenerateSlots() {
		for(int i = 0; i < SlotCount; i++) {
			int slot = i + 1;

			var button = new Button {
				Name = $"Slot{slot}",
				Text = $"Slot {slot}",
				Icon = SlotIcon,
			};

			button.AddThemeFontSizeOverride("font_size", SlotFontSize);
			button.Pressed += () => OnSlotPressed(slot);

			SlotButtons[i] = button;
			SlotContainer.AddChild(button);
		}
	}

	private void SetCallbacks() {
		BackButton.Pressed += CloseMenu;
	}

	private void RefreshSlotDisplay() {
		TitleLabel.Text = Mode == SaveMode.Save ? "Save Game" : "Load Game";

		for(int i = 0; i < SlotCount; i++) {
			int slot = i + 1;
			bool exists = SaveService.Exists(SlotFile(slot));
			SlotButtons[i].Text = exists ? $"Slot {slot}" : "Empty";
		}
	}

	public void OpenMenu(SaveMode mode, Action? onClose = null) {
		OnExit += onClose;
		Mode = mode;

		RefreshSlotDisplay();
	}

	private void CloseMenu() {
		QueueFree();
	}

	private void OnSlotPressed(int slot) {
		string fileName = SlotFile(slot);

		if(Mode == SaveMode.Save) {
			Log.Info($"Save requested: {fileName}");
			OnSave?.Invoke(fileName);
		}
		else if(Mode == SaveMode.Load) {
			Log.Info($"Load requested: {fileName}");
			OnLoad?.Invoke(fileName);
		}

		CloseMenu();
	}
}
