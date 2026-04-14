namespace Camera;

using System.Collections.Generic;
using Godot;

public class CollidingObjects {
    private HashSet<Node3D> PreviousWalls = new();
    private HashSet<Node3D> CurrentWalls = new();

    public void BeginFrame() {
        CurrentWalls.Clear();
    }

    public void AddCurrentWall(Node3D wall) {
        CurrentWalls.Add(wall);
    }

    public void EndFrame() {
        foreach(Node3D wall in CurrentWalls) {
            if(!PreviousWalls.Contains(wall)) {
                FadeOutWall(wall);
            }
        }

        foreach(Node3D wall in PreviousWalls) {
            if(!CurrentWalls.Contains(wall)) {
                FadeInWall(wall);
            }
        }

        HashSet<Node3D> temp = PreviousWalls;
        PreviousWalls = CurrentWalls;
        CurrentWalls = temp;
    }

    public void Clear() {
        foreach(Node3D wall in PreviousWalls) {
            FadeInWall(wall);
        }

        foreach(Node3D wall in CurrentWalls) {
            if(!PreviousWalls.Contains(wall)) {
                FadeInWall(wall);
            }
        }

        PreviousWalls.Clear();
        CurrentWalls.Clear();
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