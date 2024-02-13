using UnityEngine;

public class CameraFixInfo : CameraControlInfo
{
    public Vector2 fixPivot;
    
    public CameraFixInfo(double time, double term, Vector2 fixPivot) : base(time, term)
    {
        this.type = CameraControlType.Fix;
        this.fixPivot = fixPivot;
    }
}
