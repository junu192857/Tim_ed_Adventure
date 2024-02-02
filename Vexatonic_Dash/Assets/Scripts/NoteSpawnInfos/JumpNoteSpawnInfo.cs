using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpNoteSpawnInfo : NoteSpawnInfo
{

    public float jumpHeight = 0f;
    public JumpNoteSpawnInfo(double spawnTime, NoteType noteType, float jumpHeight) : base(spawnTime, noteType) {
        this.jumpHeight = jumpHeight;
    }
}
