using UnityEngine;

public class GravityData
{
    public double time;
    public Vector3 Direction { get; }

    /// <summary>
    /// Generates new GravityData with given time and angle.
    /// </summary>
    /// <param name="time">The time gravity changes. (In seconds)</param>
    /// <param name="angle">The angle of gravity direction. It should be measured counter-clockwise from downward vector.</param>
    public GravityData(double time, int angle)
    {
        this.time = time;
        Direction = Quaternion.Euler(0f, 0f, -angle) * Vector3.down;
    }
}