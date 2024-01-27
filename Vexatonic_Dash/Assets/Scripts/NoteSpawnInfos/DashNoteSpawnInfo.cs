using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashNoteSpawnInfo : NoteSpawnInfo
{
    public float dashSpeedCoeff = 1;

    public DashNoteSpawnInfo(double spawnTime, NoteType noteType, float dashSpeedCoeff) : base(spawnTime, noteType)
    {
        this.dashSpeedCoeff = dashSpeedCoeff;
    }
}
