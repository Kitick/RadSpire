namespace Camera;

using System.Collections.Generic;
using System.Linq;
using Godot;

public class CollidingObjects {
    private readonly HashSet<Node3D> FadedWalls = new();
    private readonly HashSet<Node3D> CurrentWalls = new();
    private readonly Dictionary<Node3D, int> TransitionFrames = new();
    public int FadeDebounceFrames { get; set; } = 4;

    public void BeginFrame() {
        CurrentWalls.Clear();
    }

    public void AddCurrentWall(Node3D wall) {
        CurrentWalls.Add(wall);
    }

    public void EndFrame() {
        HashSet<Node3D> allWalls = new HashSet<Node3D>(FadedWalls);
        allWalls.UnionWith(CurrentWalls);

        foreach(Node3D wall in allWalls) {
            bool shouldBeFaded = CurrentWalls.Contains(wall);
            bool isFaded = FadedWalls.Contains(wall);
            if(shouldBeFaded == isFaded) {
                TransitionFrames.Remove(wall);
                continue;
            }

            int nextFrames = TransitionFrames.TryGetValue(wall, out int frames) ? frames + 1 : 1;
            if(nextFrames < FadeDebounceFrames) {
                TransitionFrames[wall] = nextFrames;
                continue;
            }

            TransitionFrames.Remove(wall);
            if(shouldBeFaded) {
                FadeOutWall(wall);
                FadedWalls.Add(wall);
            }
            else {
                FadeInWall(wall);
                FadedWalls.Remove(wall);
            }
        }

        foreach(Node3D wall in TransitionFrames.Keys.ToList()) {
            if(!allWalls.Contains(wall)) {
                TransitionFrames.Remove(wall);
            }
        }
    }

    public void Clear() {
        foreach(Node3D wall in FadedWalls) {
            FadeInWall(wall);
        }

        FadedWalls.Clear();
        CurrentWalls.Clear();
        TransitionFrames.Clear();
    }

    private static void FadeOutWall(Node3D wall) {
        if(GodotObject.IsInstanceValid(wall) && wall is ICameraFadingObject fadingWall) {
            fadingWall.FadeOut();
        }
    }

    private static void FadeInWall(Node3D wall) {
        if(GodotObject.IsInstanceValid(wall) && wall is ICameraFadingObject fadingWall) {
            fadingWall.FadeIn();
        }
    }
}