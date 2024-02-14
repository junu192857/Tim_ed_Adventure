using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlInfo
{
    public CameraControlType type = CameraControlType.None;
    public double time;
    public double term;

    //for Editor
    public GameObject parent;

    public CameraControlInfo(double time, double term)
    {
        this.time = time;
        this.term = term;
    }
}

public enum CameraControlType
{
    None,
    Zoom,
    Fix,
    Rotate,
    Velocity,
    Return,
}