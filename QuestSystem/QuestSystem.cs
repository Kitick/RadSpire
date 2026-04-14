namespace QuestSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;

public static class QuestSystem {
	public static bool CanStart(QuestDefinition def, QuestProgress progress, int currentStage) {
		if(progress.Status != QuestStatus.NotStarted) { return false; }
		if(def.IsMainQuest && def.StageRequirement != currentStage) { return false; }
		return true;
	}

	public static QuestProgress ApplyKill(QuestDefinition def, QuestProgress progress, string enemyGroup) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not KillObjective kill) { return; }
			if(!string.IsNullOrEmpty(kill.EnemyGroup) && kill.EnemyGroup != enemyGroup) { return; }
			int next = objectives[i].CurrentCount + 1;
			objectives[i] = new QuestObjectiveProgress { CurrentCount = next, IsCompleted = next >= kill.RequiredCount };
		});
	}

	public static QuestProgress ApplyCollect(QuestDefinition def, QuestProgress progress, Inventory inventory) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not CollectObjective collect) { return; }
			int count = CountItemInInventory(collect.ItemId, inventory);
			objectives[i] = new QuestObjectiveProgress { CurrentCount = count, IsCompleted = count >= collect.RequiredCount };
		});
	}

	public static QuestProgress ApplyLocationReached(QuestDefinition def, QuestProgress progress, string locationId) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not LocationObjective location) { return; }
			if(location.LocationId != locationId) { return; }
			objectives[i] = new QuestObjectiveProgress { CurrentCount = 1, IsCompleted = true }; // Binary objective — CurrentCount = 1 marks completion
		});
	}

	public static QuestProgress ApplyTalk(QuestDefinition def, QuestProgress progress, string npcId) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not TalkObjective talk) { return; }
			if(talk.NpcId != npcId) { return; }
			objectives[i] = new QuestObjectiveProgress { CurrentCount = 1, IsCompleted = true }; // Binary objective — CurrentCount = 1 marks completion
		});
	}

	public static bool AreAllComplete(QuestProgress progress) {
		if(progress.Objectives is null || progress.Objectives.Length == 0) { return true; }
		foreach(QuestObjectiveProgress obj in progress.Objectives) {
			if(!obj.IsCompleted) { return false; }
		}
		return true;
	}

	public static (bool Advanced, int NewStage) TryAdvanceStage(
		int currentStage,
		IEnumerable<(QuestDefinition Def, QuestProgress Progress)> quests) {
		foreach((QuestDefinition? def, QuestProgress progress) in quests) {
			if(!def.IsMainQuest) { continue; }
			if(def.StageRequirement != currentStage) { continue; }
			if(progress.Status != QuestStatus.Completed) { return (false, currentStage); }
		}
		return (true, currentStage + 1);
	}

	private static QuestProgress ApplyToObjectives(
		QuestDefinition def, QuestProgress progress,
		Action<QuestObjectiveProgress[], int> update) {
		QuestObjectiveProgress[] objectives = (QuestObjectiveProgress[]) progress.Objectives.Clone();
		for(int i = 0; i < def.Objectives.Length; i++) {
			if(objectives[i].IsCompleted) { continue; }
			update(objectives, i);
		}
		return progress with { Objectives = objectives };
	}

	private static int CountItemInInventory(string itemId, Inventory inventory) {
		return inventory.ItemSlots
			.Where(slot => !slot.IsEmpty() && slot.Item!.Id == itemId)
			.Sum(slot => slot.Quantity);
	}
}
