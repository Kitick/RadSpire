namespace GameWorld;

using System;
using System.Collections.Generic;
using Character;
using Components;
using Godot;
using ItemSystem.Icons;
using QuestSystem;
using Services;

public sealed partial class EnemyManager : Node, ISaveable<EnemyManagerData> {
	private static readonly LogService Log = new(nameof(EnemyManager), enabled: true);

	public Dictionary<string, Enemy> Enemies { get; } = [];

	private readonly Dictionary<string, Action> QuestDeathUnsubscribers = [];
	private readonly Dictionary<string, Action> ItemDropUnsubscribers = [];
	private Item3DIconManager? Item3DIconManager = null;
	private PackedScene EnemyScene = null!;
	private bool IsInitialized;

	public void Initialize(Node worldNode, PackedScene enemyScene, bool spawnFromWorld = true) {
		if(IsInitialized) {
			Log.Info("Initialize skipped (already initialized).");
			return;
		}

		EnemyScene = enemyScene;
		if(spawnFromWorld && worldNode is IEnemySpawnWorld spawnWorld) {
			foreach(Marker3D spawnPoint in spawnWorld.EnemySpawnPoints) {
				if(!IsInstanceValid(spawnPoint)) {
					continue;
				}
				SpawnEnemyAt(spawnPoint);
			}
		}

		IsInitialized = true;
		Log.Info($"Initialize complete. enemies={Enemies.Count}");
	}

	public bool AddEnemy(string id, Enemy enemy) {
		if(string.IsNullOrWhiteSpace(id) || enemy == null || !IsInstanceValid(enemy) || Enemies.ContainsKey(id)) {
			return false;
		}

		if(enemy.GetParent() != this) {
			AddChild(enemy);
		}

		enemy.Id = id;
		Enemies.Add(id, enemy);
		Log.Info($"AddEnemy: id='{id}', total={Enemies.Count}");
		return true;
	}

	public bool RemoveEnemy(string id) {
		if(!Enemies.Remove(id, out Enemy? enemy)) {
			return false;
		}

		if(QuestDeathUnsubscribers.Remove(id, out Action? unsubscribe)) {
			unsubscribe();
		}

		if(ItemDropUnsubscribers.Remove(id, out Action? dropUnsubscribe)) {
			dropUnsubscribe();
		}

		if(IsInstanceValid(enemy)) {
			enemy.QueueFree();
		}
		Log.Info($"RemoveEnemy: id='{id}', total={Enemies.Count}");
		return true;
	}

	public void SetTarget(Node3D target) {
		foreach(Enemy enemy in Enemies.Values) {
			if(IsInstanceValid(enemy)) {
				enemy.SetTarget(target);
			}
		}
	}

	public void BindQuestEvents(QuestManager questManager) {
		UnbindQuestEvents();

		foreach((string id, Enemy enemy) in Enemies) {
			if(!IsInstanceValid(enemy)) {
				continue;
			}

			QuestDeathUnsubscribers[id] = enemy.WhenDead(() => questManager.NotifyEnemyKilled(enemy.EnemyType));
		}
	}

	public void UnbindQuestEvents() {
		foreach(Action unsubscribe in QuestDeathUnsubscribers.Values) {
			unsubscribe();
		}
		QuestDeathUnsubscribers.Clear();
	}

	public void SetItem3DIconManager(Item3DIconManager iconManager) {
		Item3DIconManager = iconManager;
	}

	public void BindItemDropEvents() {
		UnbindItemDropEvents();

		if(Item3DIconManager == null) {
			Log.Warn("Item3DIconManager not set. Item drops will not spawn.");
			return;
		}

		foreach((string id, Enemy enemy) in Enemies) {
			if(!IsInstanceValid(enemy)) {
				continue;
			}

			ItemDropUnsubscribers[id] = enemy.WhenDead(() => {
				if(IsInstanceValid(enemy)) {
					enemy.DropItems(itemId => Item3DIconManager.SpawnItem(itemId, enemy.GlobalPosition));
				}
			});
		}
	}

	public void UnbindItemDropEvents() {
		foreach(Action unsubscribe in ItemDropUnsubscribers.Values) {
			unsubscribe();
		}
		ItemDropUnsubscribers.Clear();
	}

	public EnemyManagerData Export() {
		Dictionary<string, EnemyData> data = [];
		foreach((string id, Enemy enemy) in Enemies) {
			if(!IsInstanceValid(enemy)) {
				continue;
			}

			data[id] = enemy.Export() with { Id = id };
		}
		return new EnemyManagerData { Enemies = data };
	}

	public void Import(EnemyManagerData data) {
		Log.Info($"Import start. incoming={data.Enemies?.Count ?? 0}");
		Cleanup();

		if(data.Enemies == null) {
			Log.Info("Import complete. total=0");
			return;
		}

		foreach((string id, EnemyData enemyData) in data.Enemies) {
			Enemy? enemy = CreateAndAddEnemy(id);
			if(enemy == null) {
				continue;
			}
			// Enemy._Ready initializes CharacterBase components (Health/Offense/Defense),
			// so import must happen after the enemy is added to the tree.
			enemy.Import(enemyData with { Id = id });
		}
		Log.Info($"Import complete. total={Enemies.Count}");
	}

	public void Cleanup() {
		Log.Info($"Cleanup start. total={Enemies.Count}");
		UnbindQuestEvents();
		UnbindItemDropEvents();

		foreach(Enemy enemy in Enemies.Values) {
			if(IsInstanceValid(enemy)) {
				enemy.QueueFree();
			}
		}
		Enemies.Clear();
		Log.Info("Cleanup complete.");
	}

	private void SpawnEnemyAt(Marker3D spawnPoint) {
		PackedScene sceneToSpawn = EnemyScene;
		if(spawnPoint is EnemySpawnPoint typedSpawnPoint && typedSpawnPoint.EnemyScene != null) {
			sceneToSpawn = typedSpawnPoint.EnemyScene;
		}

		Enemy? enemy = CreateAndAddEnemy(Guid.NewGuid().ToString(), sceneToSpawn);
		if(enemy == null) { return; }

		enemy.Position = spawnPoint.GlobalPosition;
	}

	private Enemy? CreateAndAddEnemy(string id, PackedScene? enemySceneOverride = null) {
		PackedScene sceneToSpawn = enemySceneOverride ?? EnemyScene;
		Enemy enemy = sceneToSpawn.Instantiate<Enemy>();
		return AddEnemy(id, enemy) ? enemy : null;
	}
}

public readonly record struct EnemyManagerData : ISaveData {
	public Dictionary<string, EnemyData> Enemies { get; init; }
}
