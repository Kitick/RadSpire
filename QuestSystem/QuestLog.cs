namespace QuestSystem;

using System.Collections.Generic;
using System.Text;
using Godot;
using Root;

public sealed partial class QuestLog : Control {
	[Export] private ItemList QuestList = null!;
	[Export] private TextEdit DescriptionText = null!;
	[Export] private Button ActiveButton = null!;
	[Export] private Button CompletedButton = null!;
	[Export] private Button PendingButton = null!;

	private QuestManager? QuestManagerRef;
	private QuestStatus CurrentFilter = QuestStatus.Active;
	private readonly List<QuestID> FilteredIds = [];

	public void Init(QuestManager questManager) {
		QuestManagerRef = questManager;
		questManager.QuestBecamePending += _ => RefreshList();
		questManager.QuestActivated += _ => RefreshList();
		questManager.QuestCompleted += _ => RefreshList();
		questManager.ObjectiveUpdated += (_, _) => RefreshList();
		RefreshList();
	}

	public override void _Ready() {
		this.ValidateExports();

		PendingButton.Pressed += () => SetFilter(QuestStatus.Pending);
		ActiveButton.Pressed += () => SetFilter(QuestStatus.Active);
		CompletedButton.Pressed += () => SetFilter(QuestStatus.Completed);
		QuestList.ItemSelected += OnQuestSelected;
	}

	private void SetFilter(QuestStatus status) {
		CurrentFilter = status;
		RefreshList();
	}

	private void RefreshList() {
		if(QuestManagerRef == null) { return; }

		QuestList.Clear();
		FilteredIds.Clear();
		DescriptionText.Text = "";

		foreach((QuestID id, QuestProgress progress) in QuestManagerRef.GetAllProgresses()) {
			if(progress.Status != CurrentFilter) { continue; }
			QuestDefinition? def = QuestManagerRef.GetDefinition(id);
			if(def == null) { continue; }
			FilteredIds.Add(id);
			string prefix = CurrentFilter == QuestStatus.Active ? "> " : "  ";
			QuestList.AddItem($"{prefix}{def.Title}");
		}
	}

	private void OnQuestSelected(long index) {
		if(index < 0 || index >= FilteredIds.Count) { return; }
		if(QuestManagerRef == null) { return; }

		QuestID id = FilteredIds[(int) index];
		QuestDefinition? def = QuestManagerRef.GetDefinition(id);
		if(def == null) { return; }

		QuestProgress progress = QuestManagerRef.GetProgress(id);
		DescriptionText.Text = BuildDescription(def, progress);
	}

	private static string BuildDescription(QuestDefinition def, QuestProgress progress) {
		StringBuilder sb = new();
		sb.AppendLine(def.Title);
		sb.AppendLine(def.Description);
		sb.AppendLine();

		for(int i = 0; i < def.Objectives.Length; i++) {
			QuestObjective objDef = def.Objectives[i];
			QuestObjectiveProgress objProg = (progress.Objectives != null && i < progress.Objectives.Length)
				? progress.Objectives[i]
				: default;

			string check = objProg.IsCompleted ? "[x]" : "[ ]";
			sb.AppendLine($"{check} {objDef.Description} ({objProg.CurrentCount}/{objDef.RequiredCount})");
		}

		return sb.ToString();
	}
}
