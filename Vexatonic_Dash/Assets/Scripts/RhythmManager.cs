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
    private const double pp = 0.042;
    private const double p = 0.075;
    private const double gr = 0.100;
    private const double g = 0.166;

    //게임 진행 시간. -5초부터 시작하며 1번째 마디 1번째 박자가 시작하는 타이밍이 0초이다.
    private double gameTime;

    public double GameTime {
        get => gameTime;
    }

    private float notePositiondelta = 3;

    public int score;
    public int progress;    // TODO: Update progress every frame
    public JudgementType lastJudge;

    //노트 프리팹.
    [SerializeField] private List<GameObject> notePrefabs;

    //맵 시작과 동시에 노트들에 관한 정보를 전부 가져온다.
    private List<NoteSpawnInfo> noteList;

    //게임오브젝트가 활성화된 노트들.
    private Queue<GameObject> spawnedNotes = new Queue<GameObject>();
    GameObject temp = null;

    //0.166초간 저장될 입력들. 만약 spawnedNotes의 첫 번째 노트를 처리할 입력이 inputs에 존재한다면 노트가 처리된다.
    private List<PlayerInput> inputs = new List<PlayerInput>();


    //어떤 판정이 몇 개씩 나왔는지를 다 저장해두는 곳.
    public int[] judgementList = new int[5]; // 0부터 pure perfect, perfect, great, good, miss

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
        noteList = lr.ParseFile(Application.dataPath + "\\Levels\\Tutorial.txt");
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();

        state = RhythmState.BeforeGameStart;
        gameTime = -5;
        score = 0;

        // InputManager 세팅
        GameManager.myManager.im.StartLoop(
            new List<KeyCode> { KeyCode.F, KeyCode.J, KeyCode.R, KeyCode.U, KeyCode.Space },
            new List<NoteType> { NoteType.Normal, NoteType.Normal, NoteType.Dash, NoteType.Dash, NoteType.Jump }
        );
        StartCoroutine(nameof(StartReceivingInput));
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        if (gameTime > 0) state = RhythmState.Ingame;
        
        if (noteList.Any() && gameTime >= noteList[0].spawnTime - 1) { // 노트의 정확한 타이밍보다 1초 일찍 스폰되어야만 한다.
            //노트를 소환하고 spawnedNotes에 소환된 노트의 게임오브젝트를 넣는다.
            //노트의 위치는 사용자가 설정한 노트의 속도에 따라 달라야만 한다. 일단은 Vector3.zero로 두었다.
            Vector3 spawnPos = noteList[0].spawnPosition;
            GameObject myNote = Instantiate(notePrefabs[(int)noteList[0].noteType], spawnPos, Quaternion.identity);
            myNote.transform.localScale = new Vector3(noteList[0].platformScale, 1, 1);
            Note note = myNote.GetComponent<Note>();
            note.spawnPos = spawnPos;
            note.destPos = spawnPos + new Vector3(0, notePositiondelta, 0);

            noteList.Remove(noteList[0]);
            spawnedNotes.Enqueue(myNote);
        }
        
        if (state != RhythmState.Ingame) return;

        // if (gameTime >= 0)
        // {
        //     if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
        //     {
        //         inputs.Add(new PlayerInput(NoteType.Normal));
        //     }
        //     if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.U))
        //     {
        //         inputs.Add(new PlayerInput(NoteType.Dash));
        //     }
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         inputs.Add(new PlayerInput(NoteType.Jump));
        //     }
        // }

        // TODO: 또 다른 기믹(여유되면 넣기)

        // 모든 입력의 생존시간을 Time.deltaTime만큼 줄인 뒤 시간이 다 된 input은 제거한다.
        foreach (PlayerInput input in inputs)
        {
            input.inputLifeTime -= Time.deltaTime;
            if (input.inputLifeTime < 0) inputs.Remove(input);

        }
        // Comment: 입력시간의 정밀성 확보를 위한 방법상 이 부분을 앞으로 당김
        
        if (inputs.Any() && spawnedNotes.TryPeek(out temp))
        {
            var list = inputs.Where(input => input.inputType == temp.GetComponent<Note>().noteType).ToList();

            while (list.Count > 0)
            {
                // 판정을 처리한다. 어떤 판정이 나왔는지 계산해서 judgementList에 넣는다
                JudgementType judgement;
                double timingOffset = spawnedNotes.Peek().GetComponent<Note>().lifetime - list[0].inputLifeTime + 0.166;

                judgement = timingOffset switch
                {
                    >= -pp and <= pp => JudgementType.PurePerfect,
                    >= -p and <= p => JudgementType.Perfect,
                    >= -gr and <= gr => JudgementType.Great,
                    >= -g and <= g => JudgementType.Good,
                    // > l => JudgementType.Invalid,
                    _ => JudgementType.Miss,
                };

                // if (judgement != JudgementType.Invalid) {
                AddJudgement(judgement);

                // 노트 게임오브젝트를 spanwedNotes에서 빼내고 삭제한다.
                inputs.Remove(list[0]);
                Destroy(spawnedNotes.Dequeue());
                // }

                // Comment from Vexatone: Early Miss 안 쓸 거면 코드처럼 생겨먹은 주석들 체크 해제하셈
            } 
        }

        // 정확한 타이밍에서 0.166초가 넘어가도록 처리가 안 된 노트는 제거하면서 spawnedNotes에서 없애 준다.

        if (spawnedNotes.TryPeek(out temp))
        {
            if (temp.GetComponent<Note>().lifetime < -0.166f)
            {
                Destroy(spawnedNotes.Dequeue());
                AddJudgement(JudgementType.Miss);
            }
        }
    }

    // Judgement Function.
    private void AddJudgement(JudgementType type)
    {
        int judgementIndex = type switch
        {
            JudgementType.PurePerfect => 0,
            JudgementType.Perfect     => 1,
            JudgementType.Great       => 2,
            JudgementType.Good        => 3,
            JudgementType.Miss        => 4,
            _                         => 4,
        };

        judgementList[judgementIndex] += 1;
        
        lastJudge = type;
    }

    // 모든 플랫폼을 미리 스폰한다. 
    private void GenerateMap() {
        Debug.Log("Trying Map Generate..");
        //스크롤 속도를 플레이어가 설정할 수 있게 바꿀 예정.
        //float scrollSpeed = GameManager.myManager.scrollSpeed;

        Vector3 AnchorPosition = Vector3.zero;

        foreach (var note in noteList) {
            switch (note.noteType) {
                case NoteType.Normal:
                    Debug.Log($"Anchor Position: {AnchorPosition}");
                    GameObject platform = Instantiate(notePrefabs[0], AnchorPosition, Quaternion.identity);
                    platform.GetComponent<Note>().permanent = true;
                    //TODO: 사용자 지정 노트 속도 (GameManager.noteSpeed)에 따라 spawnPosition의 위치 변화
                    note.spawnPosition = AnchorPosition + notePositiondelta * Vector3.down;


                    Color c = platform.GetComponentInChildren<SpriteRenderer>().color;
                    c.a = 0.5f;
                    platform.GetComponentInChildren<SpriteRenderer>().color = c;

                    Debug.Log($"Platform scale: {note.platformScale}");
                    //플랫폼의 너비를 바꾸는 부분. 임시로만 작업했고 플랫폼 디자인이 완료되면 바꿔야 한다
                    platform.transform.localScale = new Vector3(note.platformScale, 1, 1);

                    AnchorPosition += new Vector3(note.platformScale, 0, 0);
                    
                    break;
                    
                case NoteType.Dash:
                case NoteType.Jump:
                    break;
            }
        }
        // Spawn Platform Object
        // 다른 플랫폼들이 많지만, 우선 기본 이동 플랫폼만.
        // 120bpm 4bit(0.5초) = 1칸 너비로 하자
        Debug.Log("Thanks!");
    }

    // Input 저장
    public void AddInput(PlayerInput input)
    {
        inputs.Add(input);
    }

    // Input 루프 타이밍 맞춰? 켜기
    // 이슈 발생 시 연락 요망
    private IEnumerator StartReceivingInput()
    {
        if (gameTime >= 0) GameManager.myManager.im.Activate();
        else yield return new WaitForEndOfFrame();
    }
}

public enum JudgementType
{
    PurePerfect,
    Perfect,
    Great,
    Good,
    Miss,
    Invalid,
}
