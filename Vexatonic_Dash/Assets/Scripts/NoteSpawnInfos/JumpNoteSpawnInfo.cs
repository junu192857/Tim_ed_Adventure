using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpNoteSpawnInfo : NoteSpawnInfo
{

    public float targetHeightDelta = 0f;
    public JumpNoteSpawnInfo(double spawnTime, NoteType noteType, float targetHeightDelta) : base(spawnTime, noteType) {
        this.targetHeightDelta = targetHeightDelta;
    }
}
