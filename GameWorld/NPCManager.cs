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

	private readonly Dictionary<string, Action<string?>> PromptHandlers = [];
	private readonly Dictionary<string, Action<NPCID>> TalkedHandlers = [];
	private Action<string?>? PromptForwarder;
	private PackedScene NPCScene = null!;
	private QuestManager QuestManager = null!;
	private bool IsInitialized;

	public void Initialize(Node worldNode, PackedScene npcScene, QuestManager questManager, bool spawnFromWorld = true) {
		if(IsInitialized) {
			Log.Info("Initialize skipped (already initialized).");
			return;
		}

		NPCScene = npcScene;
		QuestManager = questManager;

		if(spawnFromWorld && worldNode is INPCSpawnWorld spawnWorld && IsInstanceValid(spawnWorld.NPCSpawnMarker)) {
			SpawnDefaultWorldNPCs(spawnWorld.NPCSpawnMarker.GlobalPosition);
		}

		IsInitialized = true;
		Log.Info($"Initialize complete. npcs={NPCs.Count}");
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
	}

	private void SpawnDefaultWorldNPCs(Vector3 baseWorldPosition) {
		SpawnNPCAt(baseWorldPosition, "npc-sera", NPCID.Sera, "Sera");
		SpawnNPCAt(baseWorldPosition + new Vector3(4f, 0f, 2f), "npc-rowan", NPCID.Rowan, "Rowan");
	}

	private void SpawnNPCAt(Vector3 worldPosition, string id, NPCID identity, string displayName) {
		NPC? npc = CreateAndAddNPC(id, identity, displayName);
		if(npc == null) { return; }

		npc.Position = worldPosition;
	}

	private NPC? CreateAndAddNPC(string id, NPCID? identity = null, string displayName = "") {
		NPC npc = NPCScene.Instantiate<NPC>();
		if(identity.HasValue) {
			npc.ConfigureIdentity(identity.Value, displayName);
		}
		return AddNPC(id, npc) ? npc : null;
	}
}

public readonly record struct NPCManagerData : ISaveData {
	public Dictionary<string, NPCData> NPCs { get; init; }
}
