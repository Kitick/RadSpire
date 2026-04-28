namespace GameWorld;

using System;
using System.Collections.Generic;
using Character;
using Godot;
using QuestSystem;
using Root;
using Services;

public sealed partial class NPCManager : Node, ISaveable<NPCManagerData> {
	private static readonly LogService Log = new(nameof(NPCManager), enabled: true);

	public Dictionary<string, NPC> NPCs { get; } = [];
	public event Action? NPCRegistryChanged;

	private readonly Dictionary<string, Action<string?>> PromptHandlers = [];
	private readonly Dictionary<string, Action<NPCID>> TalkedHandlers = [];
	private Action<string?>? PromptForwarder;
	private PackedScene NPCScene = null!;
	private QuestManager QuestManager = null!;
	private bool IsInitialized;
	private INPCSpawnWorld? PendingSpawnWorld;
	private bool PendingSpawnRequested;

	public void Initialize(Node worldNode, PackedScene npcScene, QuestManager questManager, bool spawnFromWorld = true) {
		if(IsInitialized) {
			Log.Info("Initialize skipped (already initialized).");
			return;
		}

		NPCScene = npcScene;
		QuestManager = questManager;

		if(spawnFromWorld && worldNode is INPCSpawnWorld spawnWorld) {
			// Defer spawn until the world scene is fully in-tree so spawn points have valid globals.
			PendingSpawnWorld = spawnWorld;
			if(!PendingSpawnRequested) {
				PendingSpawnRequested = true;
				CallDeferred(nameof(SpawnWorldNPCsDeferred));
			}
		}

		IsInitialized = true;
		Log.Info($"Initialize complete. npcs={NPCs.Count}");
	}

	private void SpawnWorldNPCsDeferred() {
		PendingSpawnRequested = false;
		if(PendingSpawnWorld == null) {
			return;
		}
		SpawnWorldNPCs(PendingSpawnWorld.NPCSpawnPoints);
		PendingSpawnWorld = null;
		NPCRegistryChanged?.Invoke();
	}

	public bool AddNPC(string id, NPC npc) {
		if(string.IsNullOrWhiteSpace(id) || npc == null || !IsInstanceValid(npc) || NPCs.ContainsKey(id)) {
			return false;
		}

		if(npc.GetParent() != this) {
			AddChild(npc);
		}

		npc.Id = id;
		npc.Init(QuestManager);
		NPCs.Add(id, npc);

		void handler(string? prompt) => PromptForwarder?.Invoke(prompt);
		PromptHandlers[id] = handler;
		npc.InteractionPromptChanged += handler;

		Action<NPCID> talkedHandler = QuestManager.NotifyPlayerTalkedToNPC;
		TalkedHandlers[id] = talkedHandler;
		npc.Talked += talkedHandler;
		Log.Info($"AddNPC: id='{id}', total={NPCs.Count}");
		NPCRegistryChanged?.Invoke();
		return true;
	}

	public bool RemoveNPC(string id) {
		if(!NPCs.Remove(id, out NPC? npc)) {
			return false;
		}

		if(IsInstanceValid(npc)) {
			if(PromptHandlers.Remove(id, out Action<string?>? handler)) {
				npc.InteractionPromptChanged -= handler;
			}
			if(TalkedHandlers.Remove(id, out Action<NPCID>? talkedHandler)) {
				npc.Talked -= talkedHandler;
			}
			npc.QueueFree();
		}
		Log.Info($"RemoveNPC: id='{id}', total={NPCs.Count}");
		NPCRegistryChanged?.Invoke();
		return true;
	}

	public void BindPromptForwarder(Action<string?> onPromptChanged) {
		PromptForwarder = onPromptChanged;
	}

	public void UnbindPromptForwarder() {
		PromptForwarder = null;
	}

	public NPCManagerData Export() {
		Dictionary<string, NPCData> data = [];
		foreach((string id, NPC npc) in NPCs) {
			if(!IsInstanceValid(npc)) {
				continue;
			}

			data[id] = npc.Export() with { Id = id };
		}
		return new NPCManagerData { NPCs = data };
	}

	public void Import(NPCManagerData data) {
		Log.Info($"Import start. incoming={data.NPCs?.Count ?? 0}");
		Cleanup();

		if(data.NPCs == null) {
			Log.Info("Import complete. total=0");
			return;
		}

		foreach((string id, NPCData npcData) in data.NPCs) {
			NPC? npc = CreateAndAddNPC(id);
			if(npc == null) {
				continue;
			}
			// NPC node should be in tree before applying transform from save data.
			npc.Import(npcData with { Id = id });
		}
		Log.Info($"Import complete. total={NPCs.Count}");
		NPCRegistryChanged?.Invoke();
	}

	public void Cleanup() {
		Log.Info($"Cleanup start. total={NPCs.Count}");
		foreach((string id, NPC npc) in NPCs) {
			if(IsInstanceValid(npc) && PromptHandlers.Remove(id, out Action<string?>? handler)) {
				npc.InteractionPromptChanged -= handler;
			}

			if(IsInstanceValid(npc) && TalkedHandlers.Remove(id, out Action<NPCID>? talkedHandler)) {
				npc.Talked -= talkedHandler;
			}

			if(IsInstanceValid(npc)) {
				npc.QueueFree();
			}
		}

		NPCs.Clear();
		PromptHandlers.Clear();
		TalkedHandlers.Clear();
		PromptForwarder = null;
		Log.Info("Cleanup complete.");
		NPCRegistryChanged?.Invoke();
	}

	private void SpawnWorldNPCs(Godot.Collections.Array<NPCSpawnPoint> spawnPoints) {
		if(spawnPoints == null) {
			return;
		}

		HashSet<NPCID> spawnedNpcIds = [];
		foreach(NPCSpawnPoint? spawnPoint in spawnPoints) {
			if(spawnPoint == null || !IsInstanceValid(spawnPoint) || !spawnPoint.IsInsideTree() || spawnPoint.NpcId == NPCID.None) {
				continue;
			}
			if(!spawnedNpcIds.Add(spawnPoint.NpcId)) {
				Log.Warn($"Skipping duplicate NPC spawn point for '{spawnPoint.NpcId}'.");
				continue;
			}

			string displayName = string.IsNullOrWhiteSpace(spawnPoint.DisplayNameOverride)
				? spawnPoint.NpcId.ToString()
				: spawnPoint.DisplayNameOverride;
			string id = $"npc-{spawnPoint.NpcId.ToString().ToLowerInvariant()}";
			SpawnNPCAt(spawnPoint.GlobalPosition, spawnPoint.GlobalRotation, id, spawnPoint.NpcId, displayName, spawnPoint.SceneOverride);
		}
	}

	private void SpawnNPCAt(
		Vector3 worldPosition,
		Vector3 worldRotation,
		string id,
		NPCID identity,
		string displayName,
		PackedScene? sceneOverride = null
	) {
		NPC? npc = CreateAndAddNPC(id, identity, displayName, sceneOverride);
		if(npc == null) { return; }

		npc.GlobalPosition = worldPosition;
		npc.GlobalRotation = worldRotation;
	}

	private NPC? CreateAndAddNPC(string id, NPCID? identity = null, string displayName = "", PackedScene? sceneOverride = null) {
		PackedScene npcSource = sceneOverride ?? NPCScene;
		NPC npc = npcSource.Instantiate<NPC>();
		if(identity.HasValue) {
			npc.ConfigureIdentity(identity.Value, displayName);
		}
		return AddNPC(id, npc) ? npc : null;
	}
}

public readonly record struct NPCManagerData : ISaveData {
	public Dictionary<string, NPCData> NPCs { get; init; }
}
