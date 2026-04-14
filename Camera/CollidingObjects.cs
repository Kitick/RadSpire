namespace Camera;

using System;
using Godot;
using Services;
using Root;
using System.Collections.Generic;
using System.ComponentModel;

public class CollidingObjects {
    public List<Node3D> Objects { get; } = new List<Node3D>();

    public void Add(Node3D obj) {
        if(!Objects.Contains(obj)) {
            Objects.Add(obj);
            if(obj is ICameraFadingObject fadingObj) {
                fadingObj.FadeOut();
            }
        }
    }

    public void Remove(Node3D obj) {
        Objects.Remove(obj);
        if(obj is ICameraFadingObject fadingObj) {
            fadingObj.FadeIn();
        }
    }

    public void Clear() {
        foreach(Node3D obj in Objects) {
            if(obj is ICameraFadingObject fadingObj) {
                fadingObj.FadeIn();
            }
        }
        Objects.Clear();
    }
}