using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    //개별 노트의 게임오브젝트마다 달려 있는 스크립트.
    public float lifetime; //노트의 생존 시간. 1초부터 시작하며 0초일 때 노트를 처리하는 것이 정확한 타이밍이다.
    public NoteType noteType; // Normal, Dash, Jump, Attack, Defend. 프리팹 미리 만들 거라 알아서 0부터 하나씩 들어 있다.
    public Vector3 spawnPos;
    public Vector3 destPos;

    void Start()
    {
        lifetime = 1;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        transform.position = spawnPos * lifetime + destPos * (1 - lifetime);
    }


}
