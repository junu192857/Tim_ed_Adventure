using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType { 
    Normal,
    Dash,
    Jump,
    Attack,
    Defend
}

public class NoteSpawnInfo
{
    // 노트를 스폰하기 위한 정보가 담긴 곳.

    public double spawnTime; //bpm 및 마디수, 몇 번째 비트인지를 통해 계산한 스폰 타이밍.
    public NoteType noteType;
    public float platformScale;
    public Vector3 spawnPosition;
    public NoteSpawnInfo(double spawnTime, NoteType noteType)
    {
        this.spawnTime = spawnTime;
        this.noteType = noteType;
        spawnPosition = Vector3.zero;
    }
}
