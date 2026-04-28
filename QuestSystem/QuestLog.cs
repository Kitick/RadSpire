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
		foreach((QuestID id, QuestProgress progress) in QuestManagerRef.GetAllProgresses()) {
			if(progress.Status != QuestStatus.Active) { continue; }
			QuestDefinition? def = QuestManagerRef.GetDefinition(id);
			if(def == null) { continue; }

			QuestList.AddItem(def.Title.ToUpper());
			QuestList.SetItemTooltipEnabled(QuestList.ItemCount - 1, false);

			if(progress.ReturnObjectivePending) {
				QuestList.AddItem($"  - Return to {def.NpcId}");
				QuestList.SetItemTooltipEnabled(QuestList.ItemCount - 1, false);
			}
			else {
				for(int i = 0; i < def.Objectives.Length; i++) {
					QuestList.AddItem($"  - {def.Objectives[i].Description}");
					QuestList.SetItemTooltipEnabled(QuestList.ItemCount - 1, false);
				}
			}
		}
	}
}
