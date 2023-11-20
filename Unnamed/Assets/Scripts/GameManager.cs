using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager myManager;

    public float noteSpeed; // 노트의 속력을 의미한다.
    public float notePosition; // 판정선의 위치를 의미한다.
    public float volume; // 소리 크기를 의미한다.

    public string filepath; // 레벨의 맵 파일 위치를 의미한다.

    public void Awake()
    {
        if (myManager == null)
        {
            myManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    
}
