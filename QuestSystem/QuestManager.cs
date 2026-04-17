namespace QuestSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Godot;
using InventorySystem;
using Root;
using Services;

public sealed partial class QuestManager : Node, ISaveable<QuestProgressionData> {
	private static readonly LogService Log = new(nameof(QuestManager), enabled: true);

	public event Action<string>? QuestBecamePending;
	public event Action<string>? QuestActivated;
	public event Action<string>? QuestCompleted;
	public event Action<int>? StageAdvanced;
	public event Action<string, int>? ObjectiveUpdated;
	public event Action? GameWon;

	private int CurrentStage = 0;
	private readonly Dictionary<string, QuestProgress> Progresses = [];
	private readonly Random Rng = new();

	public void Init(Player player) {
		player.Inventory.OnInventoryChanged += () => CheckCollectObjectives(player.Inventory);
		player.Hotbar.OnInventoryChanged += () => CheckCollectObjectives(player.Hotbar);
		TryMakeQuestsPending();
	}

	public string[] GetDialogueFor(NPCID npc) {
		StringName npcId = npc.ToString();
		foreach(QuestDefinition def in Quests.All) {
			if(def.NpcId != npcId) { continue; }
			if(!Progresses.TryGetValue(def.Title, out QuestProgress progress)) { continue; }

			if(progress.Status == QuestStatus.Pending) {
				return def.InitialDialogue ?? [];
			}

			if(progress.Status == QuestStatus.Active) {
				string[] hints = def.ActiveDialogue;
				if(hints == null || hints.Length == 0) { return []; }
				return [hints[Rng.Next(hints.Length)]];
			}

			if(progress.Status == QuestStatus.Completed) {
				return def.CompletionDialogue ?? [];
			}
		}
		return [];
	}

	public void NotifyDialogueFinished(NPCID npc) {
		StringName npcId = npc.ToString();
		foreach(QuestDefinition def in Quests.All) {
			if(def.NpcId != npcId) { continue; }
			if(!Progresses.TryGetValue(def.Title, out QuestProgress progress)) { continue; }
			if(progress.Status != QuestStatus.Pending) { continue; }

			Progresses[def.Title] = QuestProgress.Active(def);
			Log.Info($"Quest activated after dialogue: '{def.Title}'");
			QuestActivated?.Invoke(def.Title);
			CheckCollectsForAllInventories();
			return;
		}
	}

	public void NotifyEnemyKilled(Group enemyGroup) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyKill(def, Progresses[def.Title], enemyGroup);
			UpdateProgress(def.Title, updated);
		}
	}

	public void NotifyPlayerTalkedToNPC(NPCID npc) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyTalk(def, Progresses[def.Title], npc);
			UpdateProgress(def.Title, updated);
		}
	}

	public void NotifyLocationReached(LocationID location) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyLocationReached(def, Progresses[def.Title], location);
			UpdateProgress(def.Title, updated);
		}
	}

	public IReadOnlyDictionary<string, QuestProgress> GetAllProgresses() => Progresses;

	public QuestProgress GetProgress(string id) =>
		Progresses.TryGetValue(id, out QuestProgress p) ? p : default;

	public QuestDefinition? GetDefinition(string id) =>
		Array.Find(Quests.All, q => q.Title == id);

	public QuestProgressionData Export() => new() {
		CurrentStage = CurrentStage,
		QuestTitles = [.. Progresses.Keys],
		QuestProgresses = [.. Progresses.Values],
	};

	public void Import(QuestProgressionData data) {
		CurrentStage = data.CurrentStage;
		Progresses.Clear();
		if(data.QuestTitles is null) { return; }
		if(data.QuestProgresses is null || data.QuestProgresses.Length != data.QuestTitles.Length) {
			Log.Error("QuestProgressionData arrays length mismatch — skipping quest import");
			return;
		}
		for(int i = 0; i < data.QuestTitles.Length; i++) {
			Progresses[data.QuestTitles[i]] = data.QuestProgresses[i];
		}
	}

	private void TryMakePending(QuestDefinition def) {
		bool alreadyRegistered = Progresses.ContainsKey(def.Title);
		if(!QuestSystem.CanMakePending(def, alreadyRegistered, CurrentStage)) { return; }
		Progresses[def.Title] = QuestProgress.Pending(def);
		Log.Info($"Quest pending: '{def.Title}'");
		QuestBecamePending?.Invoke(def.Title);
	}

	private void TryMakeQuestsPending() {
		foreach(QuestDefinition def in Quests.All) {
			TryMakePending(def);
		}
	}

	private void UpdateProgress(string id, QuestProgress updated) {
		QuestProgress previous = Progresses[id];
		if(previous.Equals(updated)) { return; }
		Progresses[id] = updated;

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
		CheckGameWon();
	}

	private void TryAdvanceStage() {
		IEnumerable<(QuestDefinition def, QuestProgress)> allQuests = Quests.All
			.Where(def => Progresses.ContainsKey(def.Title))
			.Select(def => (def, Progresses[def.Title]));

		(bool advanced, int newStage) = QuestSystem.TryAdvanceStage(CurrentStage, allQuests);
		if(!advanced) { return; }

		CurrentStage = newStage;
		Log.Info($"Stage advanced to {CurrentStage}");
		StageAdvanced?.Invoke(CurrentStage);
		TryMakeQuestsPending();
	}

	private void CheckCollectObjectives(Inventory inventory) {
		foreach(QuestDefinition def in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyCollect(def, Progresses[def.Title], inventory);
			UpdateProgress(def.Title, updated);
		}
	}

	private void CheckCollectsForAllInventories() { }

	private void CheckGameWon() {
		bool allMainComplete = Quests.All
			.Where(def => def.Type == QuestType.Main)
			.All(def => Progresses.TryGetValue(def.Title, out QuestProgress p) && p.Status == QuestStatus.Completed);

		if(allMainComplete) {
			Log.Info("All main quests completed — game won.");
			GameWon?.Invoke();
		}
	}

	private IEnumerable<QuestDefinition> ActiveQuestDefs() {
		return Quests.All.Where(def =>
			Progresses.TryGetValue(def.Title, out QuestProgress p) && p.Status == QuestStatus.Active);
	}
}
