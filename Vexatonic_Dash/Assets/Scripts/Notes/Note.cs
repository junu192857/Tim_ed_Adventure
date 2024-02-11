using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Note : MonoBehaviour
{
    public bool permanent = false;
    public bool activated;

    //개별 노트의 게임오브젝트마다 달려 있는 스크립트.
    public double lifetime; //노트의 생존 시간. 1초부터 시작하며 0초일 때 노트를 처리하는 것이 정확한 타이밍이다.
    public double noteEndTime; // noteEndTime = 다음 노트의 spawnTime
    public NoteType noteType; // Normal, Dash, Jump. 프리팹 미리 만들 거라 알아서 0부터 하나씩 들어 있다.
    public NoteSubType noteSubType; // Ground, Air, Wall의 3종류. Wall은 JumpNote에만 쓸 수 있으며, PlatformNote에 쓰면 에러가 나도록 할 생각.
    public CharacterDirection direction; // 노트를 처리했을 시 캐릭터가 이동하는 방향. 중력 방향을 아래라 했을 때 왼쪽, 오른쪽 기준
    public Vector3 spawnPos; // 움직이는 노트의 스폰 위치
    public Vector3 destPos; // 움직이는 노트의 목적지(판정선 위치)

    public Vector3 startPos; // 이 노트에서 움직이는 캐릭터의 시작 위치
    public Vector3 endPos;  // 이 노트에서 움직이는 캐릭터의 끝 위치

    public int angle; // 플랫폼의 기울기
    
    public GameObject parentNote = null; // 움직이는 노트의 목적지에 있는 고정된 노트.

    public void Deactivate() {
        activated = false;
        gameObject.SetActive(false);
    }

    public void Activate() { 
        activated  =true;
        gameObject.SetActive(true);
    }

    public void SetLifetime(double gameTime, double spawnTime) {
        lifetime = spawnTime - gameTime;
    }

    public void FixNote() {
        permanent = true;
        if (parentNote != null) Destroy(parentNote);
        transform.position = destPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!permanent && activated)
        {
            lifetime -= Time.deltaTime;
            transform.position = spawnPos * (float)lifetime + destPos * (float)(1 - lifetime);
        }
    }


}
