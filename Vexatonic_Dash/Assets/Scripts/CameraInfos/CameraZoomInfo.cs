public class CameraZoomInfo : CameraControlInfo
{
    public double scale;
    
    public CameraZoomInfo(double time, double term, double scale) : base(time, term)
    {
        this.type = CameraControlType.Zoom;
        this.scale = scale;
    }
}