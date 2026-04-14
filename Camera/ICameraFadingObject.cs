namespace Camera;

using System;
using Godot;
using Services;
using Root;

public interface ICameraFadingObject {
    public void FadeIn();
    public void FadeOut();
}