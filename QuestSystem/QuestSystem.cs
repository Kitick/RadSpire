namespace QuestSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Root;

public static class QuestSystem {
	public static bool CanMakePending(QuestDefinition def, bool alreadyRegistered, int currentStage) => !alreadyRegistered && def.StageRequirement == currentStage;

	public static QuestProgress ApplyKill(QuestDefinition def, QuestProgress progress, EnemyType enemyType) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not KillObjective kill) { return; }
			if(kill.EnemyType != enemyType) { return; }
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

	public static QuestProgress ApplyLocationReached(QuestDefinition def, QuestProgress progress, LocationID location) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not LocationObjective loc) { return; }
			if(loc.LocationId != location) { return; }
			objectives[i] = new QuestObjectiveProgress { CurrentCount = 1, IsCompleted = true };
		});
	}

	public static QuestProgress ApplyTalk(QuestDefinition def, QuestProgress progress, NPCID npc) {
		return ApplyToObjectives(def, progress, (objectives, i) => {
			if(def.Objectives[i] is not TalkObjective talk) { return; }
			if(talk.NpcId != npc) { return; }
			objectives[i] = new QuestObjectiveProgress { CurrentCount = 1, IsCompleted = true };
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
			if(def.Type != QuestType.Main) { continue; }
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
