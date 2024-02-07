using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum RhythmState { 
    BeforeGameStart,
    Ingame,
    GameOver,
    GameClear
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
    public double unbeatTime = 3.0f;
    private double lastHit;

    public double GameTime {
        get => gameTime;
    }

    private float notePositiondelta = 3;

    public int score;   // 반올림된 스코어
    private double realScore;   // 실제 스코어
    public int minusScore;    // -방식 스코어
    private double scorePerNotes;    // 노트당 만점
    public int progress;
    private int playedNotes;
    public int health;
    public int highProgress;
    public int highScore;
    private int noteCount => LevelReader.noteCount;
    public JudgementType lastJudge;

    //노트 프리팹.
    [Header("Prefabs")]
    [SerializeField] private GameObject player;
    [SerializeField] private List<GameObject> notePrefabs;

    //맵 시작과 동시에 노트들에 관한 정보를 전부 가져온다.
    private List<NoteSpawnInfo> noteList;
    private List<GravityData> gravityDataList;
    private Queue<GravityData> gravityQueue;

    private Queue<GameObject> preSpawnedNotes = new Queue<GameObject>();
    //게임오브젝트가 활성화된 노트들.
    private Queue<GameObject> spawnedNotes = new Queue<GameObject>();
    GameObject temp = null;

    //캐릭터 게임오브젝트
    private CharacterControl myPlayer;
    private Vector3 playerArrive;
    private Vector3 playerDest;
    private bool playerCoroutineRunning;

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
        GameManager.myManager.rm = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        levelFilePath = GameManager.myManager.filepath;
        lr = new LevelReader();
        gravityDataList = new List<GravityData>(); // 임시로 빈 리스트를 만들어놓음.
        noteList = lr.ParseFile(levelFilePath, out gravityDataList);
        scorePerNotes = (double)1000000 / noteCount;

        gravityQueue = new Queue<GravityData>(gravityDataList);

        GenerateMap();
        Time.timeScale = 1f;
        GameManager.myManager.um.ShowLevelInfoUI();

        state = RhythmState.BeforeGameStart;
        gameTime = -5;
        score = 0;
        realScore = 0;
        minusScore = 1010000;
        progress = 0;
        health = 100;
        lastHit = -unbeatTime - 1;


        myPlayer = Instantiate(player, Vector3.zero, Quaternion.identity).GetComponent<CharacterControl>();

        (highProgress, highScore) =
            GameManager.GetScore(GameManager.myManager.um.songName, GameManager.myManager.um.difficulty);

        // InputManager 세팅
        GameManager.myManager.im.StartLoop(
            new List<KeyCode> { KeyCode.F, KeyCode.J, KeyCode.D, KeyCode.K, KeyCode.Space },
            new List<NoteType> { NoteType.Normal, NoteType.Normal, NoteType.Dash, NoteType.Dash, NoteType.Jump }
        );
        StartCoroutine(nameof(StartReceivingInput));
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        if (state == RhythmState.BeforeGameStart && gameTime > -1f) state = RhythmState.Ingame;
        
        if (noteList.Any() && gameTime >= noteList[0].spawnTime - 1) { // 노트의 정확한 타이밍보다 1초 일찍 스폰되어야만 한다.
            GameObject nextNote = preSpawnedNotes.Dequeue();
            Note note = nextNote.GetComponent<Note>();
            note.Activate();
            note.SetLifetime(gameTime, noteList[0].spawnTime);

            noteList.Remove(noteList[0]);
            spawnedNotes.Enqueue(nextNote);
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
        if (inputs.Any())
        {
            List<PlayerInput> inputsToBeRemoved = new List<PlayerInput>();
            
            foreach (PlayerInput input in inputs)
            {
                input.inputLifeTime -= Time.deltaTime;
                if (input.inputLifeTime < 0) inputsToBeRemoved.Add(input);
                if (input.inputLifeTime < 0) AddJudgement(JudgementType.Miss);
            }

            // 제거할 input을 따로 빼놓고 나중에 처리
            foreach (PlayerInput input in inputsToBeRemoved)
            {
                inputs.Remove(input);
            }

            inputsToBeRemoved.Clear();
        }
        // Comment: 입력시간의 정밀성 확보를 위한 방법상 이 부분을 앞으로 당김
        
        if (inputs.Any() && spawnedNotes.TryPeek(out temp))
        {
            Note note = temp.GetComponent<Note>();
            var list = inputs.Where(input => input.inputType == note.noteType).ToList();

            while (list.Count > 0)
            {
                // 판정을 처리한다. 어떤 판정이 나왔는지 계산해서 judgementList에 넣는다
                JudgementType judgement;
                double timingOffset = note.lifetime - list[0].inputLifeTime + 0.166;

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
                list.RemoveAt(0);


                spawnedNotes.Dequeue();
                note.FixNote();
                myPlayer.MoveCharacter(note, gameTime);
                
                // }

                // Comment from Vexatone: Early Miss 안 쓸 거면 코드처럼 생겨먹은 주석들 체크 해제하셈
            } 
        }

        // 정확한 타이밍에서 0.166초가 넘어가도록 처리가 안 된 노트는 제거하면서 spawnedNotes에서 없애 준다.

        if (spawnedNotes.TryPeek(out temp))
        {
            Note note = temp.GetComponent<Note>();
            if (note.lifetime < -0.166f)
            {
                spawnedNotes.Dequeue();
                note.FixNote();
                myPlayer.MoveCharacter(note, gameTime);
                AddJudgement(JudgementType.Miss);
            }
        }
        UpdateGravity();
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

        if (type == JudgementType.Miss) { 
            if (gameTime - lastHit >= unbeatTime) {
                lastHit = gameTime;
                health -= 20;
            }
        }


        UpdateScore(type);
        UpdatePercentage();
        GameManager.myManager.um.UpdateInGameUI();
        
        if (health <= 0) {
            GameOver();
            return;
        }

        if (playedNotes == noteCount && state == RhythmState.Ingame) GameClear();
    }

    private void GameOver() {
        state = RhythmState.GameOver;
        Time.timeScale = 0f;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(GameManager.myManager.um.songName + '_' + GameManager.myManager.um.difficulty + "Score",
                highScore);
            GameManager.myManager.um.ShowGameOverUI(true);
        }
        else
        {
            GameManager.myManager.um.ShowGameOverUI(false);
        }
    }

    private void GameClear() {
        state = RhythmState.GameClear;
        Time.timeScale = 0f;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(GameManager.myManager.um.songName + '_' + GameManager.myManager.um.difficulty + "Score",
                highScore);
            GameManager.myManager.um.ShowResultUI(true);
        }
        else
        {
            GameManager.myManager.um.ShowResultUI(false);
        }
    }

    // 모든 플랫폼을 미리 스폰한다. 
    private void GenerateMap() {
        //스크롤 속도를 플레이어가 설정할 수 있게 바꿀 예정.
        //float scrollSpeed = GameManager.myManager.scrollSpeed;

        Vector3 AnchorPosition = Vector3.zero;

        foreach (var note in noteList)
            AnchorPosition = SpawnNote(note, AnchorPosition);
        
        // Spawn Platform Object
        // 다른 플랫폼들이 많지만, 우선 기본 이동 플랫폼만.
        // 120bpm 4bit(0.5초) = 1칸 너비로 하자
        Debug.Log("Thanks!");
    }

    // 노트 찍고 다음 AnchorPosition 돌려주는 역할
    private Vector3 SpawnNote(NoteSpawnInfo info, Vector3 AnchorPosition)
    {
        Debug.Log($"Anchor Position: {AnchorPosition}");
        
        // Local variable declaration area
        float inputWidth = GameManager.myManager.CalculateInputWidthFromTime((float) info.noteLastingTime);
        NoteType type = info.noteType;
        NoteSubType subType = info.noteSubType;
        Quaternion noteGravity = Quaternion.AngleAxis(GetGravityByTiming(info.spawnTime), Vector3.forward);
        // 다음 입력까지 캐릭터의 x방향 이동 거리
        int notePrefabIndex = type switch
        {
            NoteType.Normal => 0,
            NoteType.Dash => 7,
            NoteType.Jump => 15,
            _ => throw new ArgumentException("Unknown or Unimplemented Note Type")
        };

        if (type != NoteType.Jump && subType == NoteSubType.Ground)
            notePrefabIndex += info.angle switch
            {
                -60 => 6,
                -45 => 5,
                -30 => 4,
                0 => 0,
                30 => 1,
                45 => 2,
                60 => 3,
                _ => 0
            };
        if (type == NoteType.Dash && subType == NoteSubType.Air) notePrefabIndex = 14;
        if (type == NoteType.Jump && subType == NoteSubType.Air) notePrefabIndex = 16;
        if (type == NoteType.Jump && subType == NoteSubType.Wall) notePrefabIndex = 17;
            // Should be fixed later, but from now on I'll just hard-code these due to time lack

        GameObject notePrefab = notePrefabs[notePrefabIndex];
        
        // Marker spawn area
        GameObject noteMarker = Instantiate(notePrefab, AnchorPosition, Quaternion.identity);
        noteMarker.transform.localScale = new Vector3((int)info.direction, 1f, 1f);
        noteMarker.GetComponent<Note>().permanent = true; 
        // TODO: 사용자 지정 노트 속도 (GameManager.noteSpeed)에 따라 spawnPosition의 위치 변화
        if (subType == NoteSubType.Wall)
            info.spawnPosition = AnchorPosition + notePositiondelta * (int) info.direction * (noteGravity * Vector3.left);
        else info.spawnPosition = AnchorPosition + notePositiondelta * (noteGravity * Vector3.down);

        // Marker style modification area
        SpriteRenderer markerRenderer = noteMarker.GetComponentInChildren<SpriteRenderer>();
        Color tempColor = markerRenderer.color;
        tempColor.a = 0.5f;
        markerRenderer.color = tempColor;

        if (type == NoteType.Dash)
            inputWidth *= (info as DashNoteSpawnInfo).dashCoeff;
        if (type != NoteType.Jump && subType == NoteSubType.Ground)
            markerRenderer.size = info.angle switch
            {
                0 => new Vector2(10 * inputWidth, 2.5f),
                30 => new Vector2(10 * inputWidth, 8.27f * inputWidth),
                45 => new Vector2(10 * inputWidth, 12.5f * inputWidth),
                60 => new Vector2(10 * inputWidth, 19.82f * inputWidth),
                _ => new Vector2()
            };

        noteMarker.transform.localScale = new Vector3((int) info.direction, 1, 1);
        noteMarker.transform.rotation = noteGravity;
        
        // Note spawn area
        GameObject noteObject = Instantiate(notePrefab, 100 * Vector3.down, Quaternion.identity);
        noteObject.transform.localScale = new Vector3((int)info.direction, 1f, 1f);
        SpriteRenderer noteRenderer = noteObject.GetComponentInChildren<SpriteRenderer>();
        if (type != NoteType.Jump)
            noteRenderer.size = markerRenderer.size;
        
        // Note Configuration area
        Note note = type switch
        {
            NoteType.Normal or NoteType.Dash => noteObject.GetComponent<PlatformNote>(),
            NoteType.Jump                    => noteObject.GetComponent<JumpNote>(),
            _ => throw new ArgumentException("Unknown or Unimplemented Note Type")
        };
        note.noteType = type;
        note.noteEndTime = info.spawnTime + info.noteLastingTime;
        // note.noteEndTime = noteList.IndexOf(note) == noteList.Count - 1 ? note.spawnTime + 1 : noteList[noteList.IndexOf(note) + 1].spawnTime;
        Debug.Log($"noteEndTime: {note.noteEndTime}");

        note.spawnPos = info.spawnPosition;
        note.destPos = AnchorPosition;
        note.direction = info.direction;
        note.transform.localScale = new Vector3((int) info.direction, 1, 1);
        note.transform.rotation = noteGravity;

        note.parentNote = noteMarker;

        Vector3 nextPosition = type switch
        {
            NoteType.Normal or NoteType.Dash => (note as PlatformNote)
                .GetInformationForPlayer(inputWidth, AnchorPosition, GetGravityByTiming(info.spawnTime)),
            NoteType.Jump                    => (note as JumpNote)
                .GetInformationForPlayer(inputWidth, (info as JumpNoteSpawnInfo).jumpHeight, AnchorPosition, GetGravityByTiming(info.spawnTime)),
            _ => throw new ArgumentException("Unknown or Unimplemented Note Type")
        };
        note.Deactivate();
        preSpawnedNotes.Enqueue(noteObject);
        
        // Done! The next AnchorPosition will be returned.
        return nextPosition;
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
        while (gameTime < -1f) yield return new WaitForEndOfFrame();
        GameManager.myManager.im.Activate();
    }

    // 스코어 업데이트
    private void UpdateScore(JudgementType judgement)
    {
        double rate = judgement switch
        {
            JudgementType.PurePerfect => 1.01,
            JudgementType.Perfect => 1,
            JudgementType.Great => 0.9,
            JudgementType.Good => 0.5,
            JudgementType.Miss => 0,
            _ => 0
        };

        Debug.Log("Updating Score..");
        realScore += rate * scorePerNotes;
        score = (int) (realScore + 0.5);
    }

    // -방식 스코어 업데이트
    private void UpdateMinusScore(JudgementType judgement)
    {
        UpdateScore(judgement);

        double tempScore = 1.01 * scorePerNotes * (noteCount - playedNotes) + realScore;
        minusScore = (int)(tempScore + 0.5);
    }

    // 퍼센트 업데이트
    private void UpdatePercentage()
    {
        playedNotes += 1;
        progress = 100 * playedNotes / noteCount;
    }

    /*private IEnumerator CharacterMovementCoroutine() {
        myPlayer.transform.position = 
        yield return null;
    }*/
    
    // 중력 업데이트
    private void UpdateGravity()
    {
        GravityData nextGravity;
        if (gravityQueue.TryPeek(out nextGravity) && (gameTime >= nextGravity.time))
        {
            GameManager.myManager.gravity = nextGravity.Angle;
            gravityQueue.Dequeue();
        }
    }
    
    // 특정 타이밍에서의 중력 방향 판단
    private int GetGravityByTiming(double timing)
    {
        int prevAngle = 0;
        foreach (GravityData data in gravityDataList)
        {
            if (timing >= data.time) prevAngle = data.Angle;
            else break;
        }
        
        return prevAngle;
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
