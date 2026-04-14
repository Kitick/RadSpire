namespace Camera;

using System.Collections.Generic;
using Godot;

public static class CameraCollisionExclusions {
    private static readonly HashSet<Rid> Excluded = new();

    public static void Add(Rid rid) {
        if(!rid.IsValid) { return; }
        Excluded.Add(rid);
    }

    public static void Remove(Rid rid) {
        if(!rid.IsValid) { return; }
        Excluded.Remove(rid);
    }

    public static IReadOnlyCollection<Rid> GetAll() => Excluded;
}
