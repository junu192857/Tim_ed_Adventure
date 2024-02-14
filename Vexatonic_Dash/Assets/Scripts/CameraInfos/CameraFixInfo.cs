using UnityEngine;

public class CameraFixInfo : CameraControlInfo
{
    public Vector2 fixPivotDelta;
    
    public CameraFixInfo(double time, double term, Vector2 fixPivotDelta) : base(time, term)
    {
        this.type = CameraControlType.Fix;
        this.fixPivotDelta = fixPivotDelta;
    }
}
