using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager myManager;

    public float scrollSpeed; // 카메라 이동 속도를 의미한다.
    public float noteSpeed; // 노트의 속력을 의미한다.
    public float notePosition; // 판정선의 위치를 의미한다.
    public float volume; // 소리 크기를 의미한다.

    public string filepath; // 레벨의 맵 파일 위치를 의미한다.
    // 맵 하나 추가할 때마다 업데이트는 불가능 --> filepath도 외부에 있어야 한다.
    // 프로세카 같은 경우 곡 하나 추가되면 인게임에 반영

    public RhythmManager rm;
    public UIManager um;

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
