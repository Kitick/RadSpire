namespace ItemSystem.WorldObjects.House;

using System;
using System.Collections.Generic;
using Components;
using Godot;
using ItemSystem;
using Services;

public partial class GameWorldManager : Node, ISaveable<GameWorldManagerData> {
    public string CurrentGameWorldId = null!;
    public Dictionary<string, GameWorld> GameWorlds = new();

    public bool SwitchToGameWorld(string gameWorldId) {

    }

    public bool HasGameWorld(string gameWorldId) {
        return GameWorlds.ContainsKey(gameWorldId);
    }

    public GameWorld? GetCurrentGameWorld() {
        if(CurrentGameWorldId == null) {
            return null;
        }
        if(GameWorlds.TryGetValue(CurrentGameWorldId, out GameWorld? gameWorld)) {
            return gameWorld;
        }
        return null;
    }

    public bool RegisterGameWorld(GameWorld gameWorld) {
        if(GameWorlds.ContainsKey(gameWorld.Id)) {
            return false;
        }
        GameWorlds.Add(gameWorld.Id, gameWorld);
        return true;
    }

    public bool UnregisterGameWorld(string gameWorldId) {
        return GameWorlds.Remove(gameWorldId);
    }

    public GameWorldManagerData Export() => new GameWorldManagerData {
        CurrentGameWorldId = CurrentGameWorldId,
        GameWorlds = ExportGameWorlds()
    };
    
    private Dictionary<string, GameWorldData> ExportGameWorlds() {
        Dictionary<string, GameWorldData> data = new();
        foreach(KeyValuePair<string, GameWorld> pair in GameWorlds) {
            data.Add(pair.Key, pair.Value.Export());
        }
        return data;
    }

    public void Import(GameWorldManagerData data) {
        CurrentGameWorldId = data.CurrentGameWorldId;
        ImportGameWorlds(data.GameWorlds);
    }

    private void ImportGameWorlds(Dictionary<string, GameWorldData> data) {
        GameWorlds = new();
        foreach(KeyValuePair<string, GameWorldData> pair in data) {
            GameWorld gameWorld = new();
            gameWorld.Import(pair.Value);
            GameWorlds.Add(pair.Key, gameWorld);
        }
    }
}

public readonly record struct GameWorldManagerData : ISaveData {
    public string CurrentGameWorldId { get; init; }
    public Dictionary<string, GameWorldData> GameWorlds { get; init; }
}