namespace QuestSystem;

using Godot;
using Root;

public sealed partial class QuestLog : Control {
	[Export] private ItemList QuestList = null!;

	private QuestManager? QuestManagerRef;

	public void Init(QuestManager questManager) {
		QuestManagerRef = questManager;
		questManager.QuestActivated += _ => Refresh();
		questManager.QuestCompleted += _ => Refresh();
		questManager.ObjectiveUpdated += (_, _) => Refresh();
		questManager.StageAdvanced += _ => Refresh();
		Refresh();
	}

	public override void _Ready() => this.ValidateExports();

	private void Refresh() {
		if(QuestManagerRef == null) { return; }

		QuestList.Clear();

		bool first = true;
		foreach((QuestID id, QuestProgress progress) in QuestManagerRef.GetAllProgresses()) {
			if(progress.Status != QuestStatus.Active) { continue; }
			QuestDefinition? def = QuestManagerRef.GetDefinition(id);
			if(def == null) { continue; }

			if(!first) { QuestList.AddItem("─────────────────"); }
			first = false;

			QuestList.AddItem(def.Title.ToUpper());

			for(int i = 0; i < def.Objectives.Length; i++) {
				QuestObjective objDef = def.Objectives[i];
				QuestObjectiveProgress objProg = (progress.Objectives != null && i < progress.Objectives.Length)
					? progress.Objectives[i]
					: default;

				string check = objProg.IsCompleted ? "[x]" : "[ ]";
				QuestList.AddItem($"  {check} {objDef.Description}");
			}
		}
	}
}
