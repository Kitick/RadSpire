namespace ItemSystem.WorldObjects.House;

using System;
using System.Collections.Generic;
using Camera;
using Godot;

public partial class Walls : Node3D, ICameraFadingObject {
    [Export(PropertyHint.Range, "0.0,1.0,0.05")] private float FadedAlpha = 0.0f;
    [Export(PropertyHint.Range, "0.01,1.0,0.01")] private float FadeDuration = 0.12f;

    private readonly List<MeshInstance3D> FadingMeshes = new();
    private readonly List<CollisionObject3D> WallColliders = new();
    private readonly Dictionary<MeshInstance3D, Tween> ActiveTweens = new();

    public void FadeIn() {
        RefreshWallNodes();
        SetCameraRayIgnored(false);
        foreach(MeshInstance3D mesh in FadingMeshes) {
            if(!GodotObject.IsInstanceValid(mesh)) { continue; }
            StartTransparencyTween(mesh, 0.0f);
        }
    }

    public void FadeOut() {
        RefreshWallNodes();
        SetCameraRayIgnored(true);
        foreach(MeshInstance3D mesh in FadingMeshes) {
            if(!GodotObject.IsInstanceValid(mesh)) { continue; }
            StartTransparencyTween(mesh, 1.0f - FadedAlpha);
        }
    }

    public void AddFadingMesh(MeshInstance3D mesh) {
        if(!FadingMeshes.Contains(mesh)) {
            FadingMeshes.Add(mesh);
        }
    }

    public void RemoveFadingMesh(MeshInstance3D mesh) {
        FadingMeshes.Remove(mesh);
        if(ActiveTweens.TryGetValue(mesh, out Tween? tween) && GodotObject.IsInstanceValid(tween)) {
            tween.Kill();
        }
        ActiveTweens.Remove(mesh);
    }

    public void FindFadingMeshesRecursively() {
        // Keep the list fresh to match dynamic house scene children.
        FadingMeshes.Clear();
        WallColliders.Clear();
        CollectMeshes(this);
        CollectColliders(this);
    }

    private void CollectMeshes(Node node) {
        if(node is MeshInstance3D mesh) {
            AddFadingMesh(mesh);
        }

        foreach(Node child in node.GetChildren()) {
            CollectMeshes(child);
        }
    }

    private void CollectColliders(Node node) {
        if(node is CollisionObject3D collider) {
            WallColliders.Add(collider);
        }

        foreach(Node child in node.GetChildren()) {
            CollectColliders(child);
        }
    }

    private void RefreshWallNodes() {
        FindFadingMeshesRecursively();
    }

    private void SetCameraRayIgnored(bool ignored) {
        foreach(CollisionObject3D collider in WallColliders) {
            if(!GodotObject.IsInstanceValid(collider)) { continue; }
            if(ignored) {
                CameraCollisionExclusions.Add(collider.GetRid());
            }
            else {
                CameraCollisionExclusions.Remove(collider.GetRid());
            }
        }
    }

    private void StartTransparencyTween(MeshInstance3D mesh, float targetTransparency) {
        if(ActiveTweens.TryGetValue(mesh, out Tween? activeTween) && GodotObject.IsInstanceValid(activeTween)) {
            activeTween.Kill();
        }

        targetTransparency = Mathf.Clamp(targetTransparency, 0.0f, 1.0f);
        Tween tween = CreateTween();
        ActiveTweens[mesh] = tween;

        tween.TweenProperty(mesh, "transparency", targetTransparency, FadeDuration);
        tween.Finished += () => {
            if(ActiveTweens.TryGetValue(mesh, out Tween? currentTween) && currentTween == tween) {
                ActiveTweens.Remove(mesh);
            }
        };
    }

    public override void _ExitTree() {
        SetCameraRayIgnored(false);
        base._ExitTree();
    }
}
