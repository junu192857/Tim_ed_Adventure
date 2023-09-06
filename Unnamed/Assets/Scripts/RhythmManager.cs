using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    // 판정 범위
    private const float pp = 0.025f;
    private const float p = 0.042f;
    private const float q = 0.075f;
    private const float n = 0.1f;
    private const float l = 0.167f;

    //게임오브젝트가 활성화된 노트들
    private Queue<GameObject> notes;

    //0.166초간 저장될 입력들. 만약 notes의 첫 번째 노트를 처리할 입력이 inputs에 존재한다면 노트가 처리된다.
    private List<int> inputs;

    // 게임에 활용되는 리듬게임적 요소를 다룬다.
    // 조작은 다양해도 판정은 같으므로 판정에 해당하는 공통적인 요소를 여기서 다루면 된다.

    // 노트를 프리팹으로 만든 뒤 RhythmManager에서 노트들에 대한 판정을 다루면 될 듯.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
