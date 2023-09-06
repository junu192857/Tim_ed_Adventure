using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    //개별 노트마다 달려 있는 스크립트.
    public float lifetime; //노트의 생존 시간. 1초부터 시작하며 0초일 때 노트를 처리하는 것이 정확한 타이밍이다.

    void Start()
    {
        lifetime = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
    }
}
