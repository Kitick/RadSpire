namespace ItemSystem.WorldObjects.House;

using System;
using System.Collections.Generic;
using Camera;
using Godot;

public partial class Walls : Node3D, ICameraFadingObject {
    private List<MeshInstance3D> FadingMeshes = new List<MeshInstance3D>();

    public void FadeIn() {
        FindFadingMeshesRecursively();
    }

    public void FadeOut() {
        FindFadingMeshesRecursively();
    }

    public void AddFadingMesh(MeshInstance3D mesh) {
        if(!FadingMeshes.Contains(mesh)) {
            FadingMeshes.Add(mesh);
        }
    }

    public void RemoveFadingMesh(MeshInstance3D mesh) {
        FadingMeshes.Remove(mesh);
    }

    public void FindFadingMeshesRecursively() {
        
    }
}
