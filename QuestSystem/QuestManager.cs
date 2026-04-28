namespace QuestSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Godot;
using InventorySystem;
using ItemSystem;
using ItemSystem.WorldObjects;
using Root;
using Services;

public sealed partial class QuestManager : Node, ISaveable<QuestProgressionData> {
	private static readonly LogService Log = new(nameof(QuestManager), enabled: true);

	public event Action<QuestID>? QuestBecamePending;
	public event Action<QuestID>? QuestActivated;
	public event Action<QuestID>? QuestCompleted;
	public event Action<int>? StageAdvanced;
	public event Action<QuestID, int>? ObjectiveUpdated;
	public event Action? GameWon;

	private event Action? OnExit;
	private int CurrentStage = 0;
	private readonly Dictionary<QuestID, QuestProgress> Progresses = [];
	private readonly Random Rng = new();
	private Player? PlayerRef;

	public void Init(Player player) {
		PlayerRef = player;
		player.Inventory.OnInventoryChanged += () => CheckCollectObjectives(player.Inventory);
		player.Hotbar.OnInventoryChanged += () => CheckCollectObjectives(player.Hotbar);
		TryMakeQuestsPending();
	}

	public bool IsQuestCompleted(QuestID id) =>
		Progresses.TryGetValue(id, out QuestProgress progress) && progress.Status == QuestStatus.Completed;

	public string[] GetDialogueForQuest(QuestID id) {
		if(!Quests.All.TryGetValue(id, out QuestDefinition? def)) {
			return [];
		}

		if(!Progresses.TryGetValue(id, out QuestProgress progress)) {
			string[] intro = def.InitialDialogue ?? [];
			return intro.Length > 0 ? intro : [];
		}

		return progress.Status switch {
			QuestStatus.Pending => def.InitialDialogue ?? [],
			QuestStatus.Active when !progress.InitialDialogueDelivered => def.InitialDialogue ?? [],
			QuestStatus.Active => def.ActiveDialogue ?? [],
			QuestStatus.Completed => def.CompletionDialogue ?? [],
			_ => [],
		};
	}

	public string[] GetDialogueFor(NPCID npc) {
		foreach((QuestID id, QuestDefinition def) in Quests.All) {
			if(def.NpcId != npc) { continue; }
			string[] lines = GetDialogueForQuest(id);

			if(lines.Length > 0) { return lines; }
		}
		return [];
	}

	public string[] NotifyDialogueFinished(NPCID npc) {
		List<string> notifications = [];
		foreach((QuestID id, QuestDefinition def) in Quests.All) {
			if(def.NpcId != npc) { continue; }
			if(!Progresses.TryGetValue(id, out QuestProgress progress)) { continue; }

			if(progress.Status == QuestStatus.Active && progress.ReturnObjectivePending) {
				Progresses[id] = progress with { ReturnObjectivePending = false };
				EvaluateQuest(id);
				continue;
			}

			if(progress.Status == QuestStatus.Pending) {
				Progresses[id] = QuestProgress.Active(def);
				Log.Info($"Quest activated after dialogue: '{id}'");
				QuestActivated?.Invoke(id);
				CheckCollectsForAllInventories();
				// If this quest starts from speaking to this NPC, count the same interaction immediately.
				QuestProgress talkUpdated = QuestSystem.ApplyTalk(def, Progresses[id], npc);
				UpdateProgress(id, talkUpdated);
				notifications.Add($"Quest Started: {def.Title}");
			} else if(progress.Status == QuestStatus.Active && !progress.InitialDialogueDelivered) {
				Progresses[id] = progress with { InitialDialogueDelivered = true };
				Log.Info($"Initial dialogue delivered for '{id}'");
			}
		}
		return [.. notifications];
	}

	public void NotifyEnemyKilled(EnemyType enemyType) {
		foreach((QuestID id, QuestDefinition def) in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyKill(def, Progresses[id], enemyType);
			UpdateProgress(id, updated);
		}
	}

	public void NotifyPlayerTalkedToNPC(NPCID npc) {
		foreach((QuestID id, QuestDefinition def) in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyTalk(def, Progresses[id], npc);
			UpdateProgress(id, updated);
		}
	}

	public void NotifyLocationReached(LocationID location) {
		foreach((QuestID id, QuestDefinition def) in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyLocationReached(def, Progresses[id], location);
			UpdateProgress(id, updated);
		}
	}

	public bool OfferQuest(QuestID id) {
		if(Progresses.ContainsKey(id)) {
			return false;
		}
		if(!Quests.All.TryGetValue(id, out QuestDefinition? def)) {
			return false;
		}
		if(def.OfferMode != QuestOfferMode.OfferedByNpc) {
			return false;
		}

		foreach(QuestID prerequisite in def.Prerequisites ?? []) {
			if(!IsQuestCompleted(prerequisite)) {
				return false;
			}
		}

		Progresses[id] = def.StageRequirement == 0 ? QuestProgress.Active(def) : QuestProgress.Pending(def);
		if(def.StageRequirement == 0) {
			Log.Info($"Quest auto-activated after explicit offer: '{id}'");
			QuestActivated?.Invoke(id);
			CheckCollectsForAllInventories();
			return true;
		}

		Log.Info($"Quest offered and set pending: '{id}'");
		QuestBecamePending?.Invoke(id);
		return true;
	}

	public IReadOnlyDictionary<QuestID, QuestProgress> GetAllProgresses() => Progresses;

	public QuestProgress GetProgress(QuestID id) =>
		Progresses.TryGetValue(id, out QuestProgress p) ? p : default;

	public QuestDefinition? GetDefinition(QuestID id) =>
		Quests.All.TryGetValue(id, out QuestDefinition? def) ? def : null;

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

	private void TryMakePending(QuestID id, QuestDefinition def) {
		bool alreadyRegistered = Progresses.ContainsKey(id);
		if(!QuestSystem.CanMakePending(def, alreadyRegistered, CurrentStage, IsQuestCompleted)) { return; }

		if(def.StageRequirement == 0) {
			Progresses[id] = QuestProgress.Active(def);
			Log.Info($"Quest auto-activated (stage 0): '{id}'");
			QuestActivated?.Invoke(id);
		} else {
			Progresses[id] = QuestProgress.Pending(def);
			Log.Info($"Quest pending: '{id}'");
			QuestBecamePending?.Invoke(id);
		}
	}

	private void TryMakeQuestsPending() {
		foreach((QuestID id, QuestDefinition def) in Quests.All) {
			TryMakePending(id, def);
		}
	}

	private void UpdateProgress(QuestID id, QuestProgress updated) {
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

	private void EvaluateQuest(QuestID id) {
		QuestProgress progress = Progresses[id];
		if(progress.Status != QuestStatus.Active) { return; }
		if(!QuestSystem.AreAllComplete(progress)) { return; }

		if(!progress.ReturnObjectivePending) {
			QuestDefinition? def = GetDefinition(id);
			if(def != null && def.NpcId != NPCID.None) {
				Progresses[id] = progress with { ReturnObjectivePending = true };
				Log.Info($"Quest '{id}' objectives done — waiting for return to {def.NpcId}");
				ObjectiveUpdated?.Invoke(id, -1);
				return;
			}
		}

		Progresses[id] = progress with { Status = QuestStatus.Completed };
		Log.Info($"Quest completed: '{id}'");
		QuestCompleted?.Invoke(id);
		GiveRewards(id);
		// A completed quest can unlock prerequisite-gated quests at the same stage.
		TryMakeQuestsPending();

		TryAdvanceStage();
		CheckGameWon();
	}

	private void GiveRewards(QuestID id) {
		if(PlayerRef == null) { return; }
		if(!Quests.All.TryGetValue(id, out QuestDefinition? def)) { return; }

		if(def.Rewards != null) {
			foreach(QuestItemReward reward in def.Rewards) {
				GiveItem(reward.ItemId, reward.Quantity);
			}
		}

		if(def.RewardPools != null) {
			foreach(QuestRewardPool pool in def.RewardPools) {
				if(pool.Choices == null || pool.Choices.Length == 0) { continue; }
				for(int i = 0; i < pool.Count; i++) {
					QuestItemReward pick = pool.Choices[Rng.Next(pool.Choices.Length)];
					GiveItem(pick.ItemId, pick.Quantity);
				}
			}
		}
	}

	private void GiveItem(string itemId, int quantity) {
		if(PlayerRef == null) { return; }
		for(int i = 0; i < quantity; i++) {
			Item item = DatabaseManager.Instance.CreateItemInstanceById(new StringName(itemId));
			if(item == null) { continue; }
			PlayerRef.Inventory.AddItem(item);
		}
	}

	private void TryAdvanceStage() {
		IEnumerable<(QuestDefinition def, QuestProgress)> allQuests = Quests.All
			.Where(kvp => Progresses.ContainsKey(kvp.Key))
			.Select(kvp => (kvp.Value, Progresses[kvp.Key]));

		(bool advanced, int newStage) = QuestSystem.TryAdvanceStage(CurrentStage, allQuests);
		if(!advanced) { return; }

		CurrentStage = newStage;
		Log.Info($"Stage advanced to {CurrentStage}");
		StageAdvanced?.Invoke(CurrentStage);
		TryMakeQuestsPending();
	}

	private void CheckCollectObjectives(Inventory inventory) {
		foreach((QuestID id, QuestDefinition def) in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyCollect(def, Progresses[id], inventory);
			UpdateProgress(id, updated);
		}
	}

	public void NotifyStructureValuesChanged(WorldObjectManager? worldObjectManager) {
		foreach((QuestID id, QuestDefinition def) in ActiveQuestDefs()) {
			QuestProgress updated = QuestSystem.ApplyStructureValue(
				def,
				Progresses[id],
				npcId => ResolveStructureTotalValueForNpc(worldObjectManager, npcId)
			);
			UpdateProgress(id, updated);
		}
	}

	private void CheckCollectsForAllInventories() { }

	private static int ResolveStructureTotalValueForNpc(WorldObjectManager? worldObjectManager, NPCID npcId) {
		if(worldObjectManager == null || npcId == NPCID.None) {
			return 0;
		}

		foreach(ItemSystem.WorldObjects.Object worldObject in worldObjectManager.GetWorldObjects()) {
			if(!worldObject.ComponentDictionary.Has<StructureComponent>()) {
				continue;
			}

			StructureComponent structure = worldObject.ComponentDictionary.Get<StructureComponent>();
			structure.ResolveAttachedNPC();
			if(structure.AttachedNPC == null || !GodotObject.IsInstanceValid(structure.AttachedNPC)) {
				continue;
			}
			if(structure.AttachedNPC.NpcIdentity != npcId) {
				continue;
			}

			structure.UpdateTotalValue();
			return Math.Max(0, structure.TotalValue);
		}

		return 0;
	}

	private void CheckGameWon() {
		bool allMainComplete = Quests.All
			.Where(kvp => kvp.Value.Type == QuestType.Main)
			.All(kvp => Progresses.TryGetValue(kvp.Key, out QuestProgress p) && p.Status == QuestStatus.Completed);

		if(allMainComplete) {
			Log.Info("All main quests completed — game won.");
			GameWon?.Invoke();
		}
	}

	private IEnumerable<KeyValuePair<QuestID, QuestDefinition>> ActiveQuestDefs() {
		return Quests.All.Where(kvp =>
			Progresses.TryGetValue(kvp.Key, out QuestProgress p) && p.Status == QuestStatus.Active);
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		ClearEvents();
	}

	private void ClearEvents() {
		QuestBecamePending = null;
		QuestActivated = null;
		QuestCompleted = null;
		StageAdvanced = null;
		ObjectiveUpdated = null;
		GameWon = null;
	}
}
