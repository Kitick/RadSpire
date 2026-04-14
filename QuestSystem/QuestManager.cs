namespace QuestSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Godot;
using InventorySystem;
using Services;

public sealed partial class QuestManager : Node, ISaveable<QuestProgressionData> {
	private static readonly LogService Log = new(nameof(QuestManager), enabled: true);

	public event Action<string>? QuestStarted;
	public event Action<string>? QuestCompleted;
	public event Action<int>? StageAdvanced;
	public event Action<string, int>? ObjectiveUpdated;

	private int CurrentStage = 0;
	private readonly Dictionary<string, QuestProgress> Progresses = [];

	public void Init(Player player) {
		player.Inventory.OnInventoryChanged += () => CheckCollectObjectives(player.Inventory);
		player.Hotbar.OnInventoryChanged += () => CheckCollectObjectives(player.Hotbar);
		TryAutoStartQuests();
	}

	public void NotifyEnemyKilled(string enemyGroup) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyKill(def, Progresses[def.Id], enemyGroup);
			UpdateProgress(def.Id, updated);
		}
	}

	public void NotifyPlayerTalkedToNPC(string npcId) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyTalk(def, Progresses[def.Id], npcId);
			UpdateProgress(def.Id, updated);
		}
	}

	public void NotifyLocationReached(string locationId) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyLocationReached(def, Progresses[def.Id], locationId);
			UpdateProgress(def.Id, updated);
		}
	}

	public IReadOnlyDictionary<string, QuestProgress> GetAllProgresses() => Progresses;

	public QuestProgress GetProgress(string id) =>
		Progresses.TryGetValue(id, out QuestProgress p) ? p : default;

	public QuestDefinition? GetDefinition(string id) =>
		Array.Find(Quests.All, q => q.Id == id);

	public QuestProgressionData Export() => new() {
		CurrentStage = CurrentStage,
		QuestIds = [.. Progresses.Keys],
		QuestProgresses = [.. Progresses.Values],
	};

	public void Import(QuestProgressionData data) {
		CurrentStage = data.CurrentStage;
		Progresses.Clear();
		if(data.QuestIds is null) { return; }
		if(data.QuestProgresses is null || data.QuestProgresses.Length != data.QuestIds.Length) {
			Log.Error("QuestProgressionData arrays length mismatch — skipping quest import");
			return;
		}
		for(int i = 0; i < data.QuestIds.Length; i++) {
			Progresses[data.QuestIds[i]] = data.QuestProgresses[i];
		}
	}

	private void TryStartQuest(QuestDefinition def) {
		if(!Progresses.TryGetValue(def.Id, out QuestProgress progress)) {
			progress = default;
		}
		if(!QuestSystem.CanStart(def, progress, CurrentStage)) { return; }
		Progresses[def.Id] = QuestProgress.For(def);
		Log.Info($"Quest started: '{def.Id}'");
		QuestStarted?.Invoke(def.Id);
	}

	private void TryAutoStartQuests() {
		foreach(QuestDefinition def in Quests.All) {
			TryStartQuest(def);
		}
	}

	private void UpdateProgress(string id, QuestProgress updated) {
		QuestProgress previous = Progresses[id];
		if(previous.Equals(updated)) { return; }
		Progresses[id] = updated;

		// Fire objective update events for any newly changed objectives
		if(updated.Objectives == null) { EvaluateQuest(id); return; }
		for(int i = 0; i < updated.Objectives.Length; i++) {
			if(previous.Objectives != null && i < previous.Objectives.Length && updated.Objectives[i].Equals(previous.Objectives[i])) { continue; }
			ObjectiveUpdated?.Invoke(id, i);
		}

		EvaluateQuest(id);
	}

	private void EvaluateQuest(string id) {
		QuestProgress progress = Progresses[id];
		if(progress.Status != QuestStatus.Active) { return; }
		if(!QuestSystem.AreAllComplete(progress)) { return; }

		Progresses[id] = progress with { Status = QuestStatus.Completed };
		Log.Info($"Quest completed: '{id}'");
		QuestCompleted?.Invoke(id);

		TryAdvanceStage();
	}

	private void TryAdvanceStage() {
		IEnumerable<(QuestDefinition def, QuestProgress)> allQuests = Quests.All
			.Where(def => Progresses.ContainsKey(def.Id))
			.Select(def => (def, Progresses[def.Id]));

		(bool advanced, int newStage) = QuestSystem.TryAdvanceStage(CurrentStage, allQuests);
		if(!advanced) { return; }

		CurrentStage = newStage;
		Log.Info($"Stage advanced to {CurrentStage}");
		StageAdvanced?.Invoke(CurrentStage);
		TryAutoStartQuests();
	}

	private void CheckCollectObjectives(Inventory inventory) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyCollect(def, Progresses[def.Id], inventory);
			UpdateProgress(def.Id, updated);
		}
	}

	private IEnumerable<QuestDefinition> ActiveQuestDefs() {
		return Quests.All.Where(def =>
			Progresses.TryGetValue(def.Id, out QuestProgress p) && p.Status == QuestStatus.Active);
	}
}
