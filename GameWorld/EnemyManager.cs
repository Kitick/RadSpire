namespace GameWorld;

using System;
using System.Collections.Generic;
using Character;
using Components;
using Godot;
using QuestSystem;
using Services;

public sealed partial class EnemyManager : Node, ISaveable<EnemyManagerData> {
	private static readonly LogService Log = new(nameof(EnemyManager), enabled: true);

	public Dictionary<string, Enemy> Enemies { get; } = [];

	private readonly Dictionary<string, Action> DeathUnsubscribers = [];
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
				SpawnEnemyAt(spawnPoint.GlobalPosition);
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

		if(DeathUnsubscribers.Remove(id, out Action? unsubscribe)) {
			unsubscribe();
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

			DeathUnsubscribers[id] = enemy.WhenDead(() => questManager.NotifyEnemyKilled(enemy.EnemyType));
		}
	}

	public void UnbindQuestEvents() {
		foreach(Action unsubscribe in DeathUnsubscribers.Values) {
			unsubscribe();
		}
		DeathUnsubscribers.Clear();
	}

	public EnemyManagerData Export() {
		Dictionary<string, EnemyData> data = new();
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

		foreach(Enemy enemy in Enemies.Values) {
			if(IsInstanceValid(enemy)) {
				enemy.QueueFree();
			}
		}
		Enemies.Clear();
		Log.Info("Cleanup complete.");
	}

	private void SpawnEnemyAt(Vector3 worldPosition) {
		Enemy? enemy = CreateAndAddEnemy(Guid.NewGuid().ToString());
		if(enemy == null) {
			return;
		}
		enemy.GlobalPosition = worldPosition;
	}

	private Enemy? CreateAndAddEnemy(string id) {
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		return AddEnemy(id, enemy) ? enemy : null;
	}
}

public readonly record struct EnemyManagerData : ISaveData {
	public Dictionary<string, EnemyData> Enemies { get; init; }
}
