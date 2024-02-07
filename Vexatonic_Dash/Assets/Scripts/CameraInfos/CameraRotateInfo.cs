public class CameraRotateInfo : CameraControlInfo
{
    public int angle;
    
    public CameraRotateInfo(double time, double term, int angle) : base(time, term, false)
    {
        this.angle = angle;
    }
}