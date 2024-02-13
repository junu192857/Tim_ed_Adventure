public class CameraReturnInfo : CameraControlInfo
{
    public CameraReturnInfo(double time, double term) : base(time, term)
    {
        type = CameraControlType.Return;
    }
}
