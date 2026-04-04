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