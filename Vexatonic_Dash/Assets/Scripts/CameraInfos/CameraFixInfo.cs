using UnityEngine;

public class CameraFixInfo : CameraControlInfo
{
    public bool isFixActivation;
    public Vector2 fixPivot;
    
    public CameraFixInfo(double time, double term, bool isDefault, bool isFixActivation, Vector2 fixPivot) : base(time, term, isDefault)
    {
        this.isFixActivation = isFixActivation;
        this.fixPivot = fixPivot;
    }
}
