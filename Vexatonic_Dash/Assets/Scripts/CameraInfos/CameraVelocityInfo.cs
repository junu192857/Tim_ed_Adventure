using UnityEngine;

public class CameraVelocityInfo : CameraControlInfo
{
    public Vector2 cameraVelocity;
    
    public CameraVelocityInfo(double time, bool isDefault, Vector2 velocity) : base(time, 0)
    {
        this.type = CameraControlType.Velocity;
        cameraVelocity = velocity;
    }
}