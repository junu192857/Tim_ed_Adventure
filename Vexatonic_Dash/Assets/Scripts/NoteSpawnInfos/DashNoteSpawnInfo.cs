using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashNoteSpawnInfo : NoteSpawnInfo
{
    public float dashCoeff = 1;

    public DashNoteSpawnInfo(double spawnTime, NoteType noteType, float dashCoeff) : base(spawnTime, noteType)
    {
        this.dashCoeff = dashCoeff;
    }
}
