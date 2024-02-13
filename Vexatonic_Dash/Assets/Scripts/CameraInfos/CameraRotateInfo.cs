public class CameraRotateInfo : CameraControlInfo
{
    public int angle;
    
    public CameraRotateInfo(double time, double term, int angle) : base(time, term)
    {
        this.type = CameraControlType.Rotate;
        this.angle = angle;
    }
}