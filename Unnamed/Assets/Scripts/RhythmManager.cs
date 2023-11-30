using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum RhythmState { 
    BeforeGameStart,
    Ingame,
    GameOver
}

public class RhythmManager : MonoBehaviour
{
    private RhythmState state;
    // 레벨 텍스트 파일이 저장될 위치.
    [SerializeField]private string levelFilePath;

    // 레벨을 읽기 위해 있는 오브젝트
    private LevelReader lr;

    // 판정 범위
    private const double pp = 0.025;
    private const double p = 0.042;
    private const double q = 0.075;
    private const double n = 0.1;
    private const double l = 0.166;

    //게임 진행 시간. -3초부터 시작하며 1번째 마디 1번째 박자가 시작하는 타이밍이 0초이다.
    private double gameTime;

    //노트 프리팹.
    [SerializeField] private List<GameObject> notePrefabs;

    //맵 시작과 동시에 노트들에 관한 정보를 전부 가져온다.
    private Queue<NoteSpawnInfo> noteList;

    //게임오브젝트가 활성화된 노트들.
    private Queue<GameObject> spawnedNotes;

    //0.166초간 저장될 입력들. 만약 spawnedNotes의 첫 번째 노트를 처리할 입력이 inputs에 존재한다면 노트가 처리된다.
    private List<PlayerInput> inputs;


    //어떤 판정이 몇 개씩 나왔는지를 다 저장해두는 곳.
    private int[] judgementList = new int[5]; // 0부터 pure perfect, perfect, great, nice, least, miss

    // 게임에 활용되는 리듬게임적 요소를 다룬다.
    // 조작은 다양해도 판정은 같으므로 판정에 해당하는 공통적인 요소를 여기서 다루면 된다.

    // 노트를 프리팹으로 만든 뒤 RhythmManager에서 노트들에 대한 판정을 다루면 될 듯.

    //게임 로직
    //1. txt 파일을 처음부터 끝까지 한 줄씩 읽으면서 noteList에 Note 타입의 객체들을 하나씩 넣는다. 
    //2. 게임이 시작하자마자, noteList의 맨 앞에 있는 spawnTiming과 gameTime을 비교해서 gameTime >= spawnTiming이 되는 순간 노트를 소환한다.
    //3. 노트를 소환하면 해당 노트를 spawnedNotes에 넣는다.
    //3. 이후에는 노트 입력을 열심히 처리한다.


    private void Awake()
    {
        lr = new LevelReader();
    }
    // Start is called before the first frame update
    void Start()
    {
        noteList = lr.ParseFile(levelFilePath);
        gameTime = -3;
    }

    // Update is called once per frame
    void Update()
    {
        if (state != RhythmState.Ingame) return;

        gameTime += Time.deltaTime;
        
        if (gameTime >= noteList.Peek().spawnTime - 1) { // 노트의 정확한 타이밍보다 1초 일찍 스폰되어야만 한다.
            //노트를 소환하고 spawnedNotes에 소환된 노트의 게임오브젝트를 넣는다.
            //노트의 위치는 사용자가 설정한 노트의 속도에 따라 달라야만 한다. 일단은 Vector3.zero로 두었다.
            GameObject myNote = Instantiate(notePrefabs[(int)noteList.Dequeue().noteType], Vector3.zero, Quaternion.identity);
            spawnedNotes.Enqueue(myNote);
        }
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J)) {
            inputs.Add(new PlayerInput(NoteType.Normal));
        }
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.U)) {
            inputs.Add(new PlayerInput(NoteType.Dash));
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            inputs.Add(new PlayerInput(NoteType.Jump));
        }
        // TODO: 공격과 방어 버튼


        var list = inputs.Where(input => input.inputType == spawnedNotes.Peek().GetComponent<Note>().noteType).ToList();
        while (list.Count > 0) {

            // TODO: 판정을 처리한다. 어떤 판정이 나왔는지 계산해서 judgementList에 넣는다

            // 노트 게임오브젝트를 spanwedNotes에서 빼내고 삭제한다.
            inputs.Remove(list[0]);
            Destroy(spawnedNotes.Dequeue());
            
        }

        

        // 모든 입력의 생존시간을 Time.deltaTime만큼 줄인 뒤 시간이 다 된 input은 제거한다.
        foreach (PlayerInput input in inputs) {
            input.inputLifeTime -= Time.deltaTime;
            if (input.inputLifeTime < 0) inputs.Remove(input);
            
        }

        // 정확한 타이밍에서 0.166초가 넘어가도록 처리가 안 된 노트는 제거하면서 spawnedNotes에서 없애 준다.
        while (spawnedNotes.Peek().GetComponent<Note>().lifetime < -0.166) {
            Destroy(spawnedNotes.Dequeue());
            //TODO: Miss 판정을 하나 추가한다.
        }
    }


    

}
