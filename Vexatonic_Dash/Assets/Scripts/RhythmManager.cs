using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum RhythmState { 
    BeforeGameStart,
    Ingame,
    Paused,
    GameOver,
    GameClear
}

public class RhythmManager : MonoBehaviour
{

    public RhythmState state;
    // 레벨 텍스트 파일이 저장될 위치.
    [SerializeField]private string levelFilePath;

    // 레벨을 읽기 위해 있는 오브젝트
    private LevelReader lr;

    // 판정 범위
    private const double pp = 0.042;
    private const double p = 0.075;
    private const double gr = 0.100;
    private const double g = 0.166;
    private const double l = 0.3;

    // 튜토리얼 전용 판정 범위
    private const double tpp = 0.100;
    private const double tp = 0.300;

    private float overtime;
    //게임 진행 시간. -5초부터 시작하며 1번째 마디 1번째 박자가 시작하는 타이밍이 0초이다.
    public double gameTime;
    public double unbeatTime = 3.0f;
    private double lastHit;

    private AudioSource song;

    private IEnumerator pauseCoroutine;
    private IEnumerator pauseUICoroutine;

    public double GameTime {
        get => gameTime;
    }

    public float notePositiondelta => GameManager.myManager.noteSpeed;

    public int score;   // 반올림된 스코어
    private double realScore;   // 실제 스코어
    public int minusScore;    // -방식 스코어
    private double scorePerNotes;    // 노트당 만점
    public int progress;
    private int playedNotes;
    private int combo;
    public int health;
    public int highProgress;
    public int highScore;
    private int noteCount => LevelReader.noteCount;
    private int myNoteCount;
    public JudgementType lastJudge;

    [SerializeField] private List<GameObject> notePrefabs;

    public List<KeyCode> keyList => GameManager.myManager.keyList;

    //맵 시작과 동시에 노트들에 관한 정보를 전부 가져온다.
    private int levelOffset;
    private List<NoteSpawnInfo> noteList;
    private List<GravityData> gravityDataList;
    private List<CameraControlInfo> cameraInfoList;

    private Queue<GravityData> gravityQueue;
    public Queue<CameraControlInfo> cameraInfoQueue;
    
    private Queue<GameObject> preSpawnedNotes = new Queue<GameObject>();
    //게임오브젝트가 활성화된 노트들.
    private Queue<GameObject> spawnedNotes = new Queue<GameObject>();
    GameObject temp = null;

    //캐릭터 게임오브젝트
    [SerializeField] private GameObject player;
    [SerializeField] private CharacterControl myPlayer;
    [SerializeField] private GameObject particlePrefab;

    private readonly Vector3 particleLocalPos = new(-0.177f, 0f, 0f);

    private Vector3 playerArrive;
    private Vector3 playerDest;
    private bool playerCoroutineRunning;

    //0.166초간 저장될 입력들. 만약 spawnedNotes의 첫 번째 노트를 처리할 입력이 inputs에 존재한다면 노트가 처리된다.
    private List<PlayerInput> inputs = new List<PlayerInput>();


    //어떤 판정이 몇 개씩 나왔는지를 다 저장해두는 곳.
    public int[] judgementList = new int[5]; // 0부터 pure perfect, perfect, great, good, miss

    private Note currentPlayingNote;

    public Note CurrentPlayingNote
    {
        get
        {
            Note ret = currentPlayingNote;
            currentPlayingNote = null;
            return ret;
        }
        set => currentPlayingNote = value;
    }


    [Header ("Tutorial")]
    [SerializeField] private AudioClip tutorialBgm;
    public bool isTutorial => GameManager.myManager.isTutorial;
    private int tutorialIndicatorIndex;
    private double[] startTimeForIndex = new double[] { 0, 28, 51, 52, 69, 70, 85, 86, 103, 104, 118, 119, 120, 126, 127, 128, 129, 147 };


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
      
        song = GameManager.myManager.sm.GetComponent<AudioSource>();
        song.loop = false;
        if (isTutorial) song.clip = tutorialBgm;

        overtime = isTutorial ? 0.3f : 0.166f;
        tutorialIndicatorIndex = 0;

        song.Stop();
        song.volume = 0f;

       
        levelFilePath = GameManager.myManager.filepath;
        lr = new LevelReader();
        gravityDataList = new List<GravityData>(); // 임시로 빈 리스트를 만들어놓음.
        levelOffset = 0;
        noteList = lr.ParseFile(levelFilePath, false,  out gravityDataList, out cameraInfoList, out levelOffset);

        myNoteCount = isTutorial ? noteCount - 6 : noteCount;


        scorePerNotes = (double)1000000 / myNoteCount;

        gravityQueue = new Queue<GravityData>(gravityDataList);
        cameraInfoQueue = new Queue<CameraControlInfo>(cameraInfoList);

        GenerateMap();
        Time.timeScale = 1f;
        GameManager.myManager.um.ShowLevelInfoUI();

        state = RhythmState.BeforeGameStart;
        gameTime = -4;
        score = 0;
        realScore = 0;
        minusScore = 1010000;
        combo = 0;
        progress = 0;
        health = 100;
        lastHit = -unbeatTime - 1;
        pauseCoroutine = null;
        pauseUICoroutine = null;


        (highProgress, highScore) =
            GameManager.GetScore(GameManager.myManager.selectedSongName, GameManager.myManager.selectedDifficulty);

        Debug.Log($"High Progress: {highProgress}, High Score: {highScore}");
        
        // InputManager 세팅
        GameManager.myManager.im.StartLoop(
            keyList,
            new List<NoteType> { NoteType.Normal, NoteType.Normal, NoteType.Dash, NoteType.Dash, NoteType.Jump }
        );
        StartCoroutine(nameof(StartReceivingInput));

        myPlayer.InitialCharacterMove();
        StartCoroutine(LatelyStartSong());
        if (isTutorial) {
            GameManager.myManager.um.ReadTutorial();
            StartCoroutine(GameManager.myManager.um.TutorialCoroutine());     
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime;



        if (state == RhythmState.BeforeGameStart && gameTime > -1f) { 
            state = RhythmState.Ingame;
            StartCoroutine(StartParticle());
        }
        
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
                //if (input.inputLifeTime < 0) AddJudgement(JudgementType.Miss);
            }

            // 제거할 input을 따로 빼놓고 나중에 처리
            foreach (PlayerInput input in inputsToBeRemoved)
            {
                inputs.Remove(input);
            }

            inputsToBeRemoved.Clear();
        }
        // Comment: 입력시간의 정밀성 확보를 위한 방법상 이 부분을 앞으로 당김
        if (isTutorial && spawnedNotes.TryPeek(out temp)) {
            Note note = temp.GetComponent<Note>();
            if (note.tutorialDisable) {
                inputs.Clear();
                if (gameTime > startTimeForIndex[tutorialIndicatorIndex]) {
                    DequeueNoteFromQueue();
                    myPlayer.MoveCharacter(note, gameTime);
                    tutorialIndicatorIndex++;
                }
            }
        }


        
        if (inputs.Any() && spawnedNotes.TryPeek(out temp))
        {
            Note note = temp.GetComponent<Note>();
            var list = inputs.Where(input => input.inputType == note.noteType).ToList();

            while (list.Count > 0)
            {
                // 판정을 처리한다. 어떤 판정이 나왔는지 계산해서 judgementList에 넣는다
                JudgementType judgement;
                double timingOffset = note.lifetime - list[0].inputLifeTime + 0.166;

                if (!isTutorial)
                {
                    judgement = timingOffset switch
                    {
                        >= -pp and <= pp => JudgementType.PurePerfect,
                        >= -p and <= p => JudgementType.Perfect,
                        >= -gr and <= gr => JudgementType.Great,
                        >= -g and <= g => JudgementType.Good,
                        >= l => JudgementType.Invalid,
                        _ => JudgementType.Miss,
                    };
                }
                else {
                    judgement = timingOffset switch
                    {
                        >= -tpp and <= tpp => JudgementType.PurePerfect,
                        >= -tp and <= tp => JudgementType.Perfect,
                        _ => JudgementType.Invalid,
                    };
                }

                if (judgement != JudgementType.Invalid) 
                {
                    // 노트 게임오브젝트를 spanwedNotes에서 빼내고 삭제한다.
                    inputs.Remove(list[0]);
                    list.RemoveAt(0);

                    DequeueNoteFromQueue();
                    note.FixNote();
                    GameManager.myManager.um.SpawnHalo(note);
                    myPlayer.MoveCharacter(note, gameTime);
                    AddJudgement(judgement);
                }
                else
                {
                    //유효하지 않은 입력 -> 입력만 제거
                    inputs.Remove(list[0]);
                    list.RemoveAt(0);
                }

                // Comment from Vexatone: Early Miss 안 쓸 거면 코드처럼 생겨먹은 주석들 체크 해제하셈
            } 
        }

        // 정확한 타이밍에서 0.166초가 넘어가도록 처리가 안 된 노트는 제거하면서 spawnedNotes에서 없애 준다.

        if (spawnedNotes.TryPeek(out temp))
        {
            Note note = temp.GetComponent<Note>();
            if (note.lifetime < -overtime)
            {
                DequeueNoteFromQueue();
                note.FixNote();
                GameManager.myManager.um.SpawnHalo(note);
                myPlayer.MoveCharacter(note, gameTime);
                AddJudgement(JudgementType.Miss);
            }else if (note.lifetime < 0.18f && note.sound == false)
            {
                note.sound = true;
                GameManager.myManager.sm.PlayUTouch();
            }
        }
        UpdateGravity();

        if (isTutorial && song.time >= 56.95f) {
            song.Stop();
            song.Play();
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


        if (type == JudgementType.Miss) { 
            if (gameTime - lastHit >= unbeatTime) {
                lastHit = gameTime;
                if (!isTutorial)
                {
                    health -= 20;
                    StartCoroutine(GameManager.myManager.um.HealthBarAnimation(health / 100f));
                    GameManager.myManager.um.HitAnimation(health);
                }
                myPlayer.HurtPlayer(health);
            }
        }

        if (type == JudgementType.Miss) combo = 0;
        else
        {
            combo++;
            GameManager.myManager.sm.PlayTouch();
        }
        if (isTutorial) tutorialIndicatorIndex++;

        UpdateScore(type);
        UpdatePercentage();
        GameManager.myManager.um.DisplayJudge(myPlayer.transform.position, combo);
        GameManager.myManager.um.UpdateInGameUI();
        
        if (health <= 0) {
            GameOver();
            return;
        }

        if (playedNotes == myNoteCount && state == RhythmState.Ingame) GameClear();
    }

    private void GameOver() {
        state = RhythmState.GameOver;
        song.Stop();
        Time.timeScale = 0f;
        GameManager.myManager.im.Deactivate();
        GameManager.myManager.sm.PlaySFX("Game Over");

        if (progress > highProgress)
        {
            var patternKey = GameManager.myManager.selectedSongName + '_' + GameManager.myManager.selectedDifficulty;
            highProgress = progress;
            PlayerPrefs.SetInt(patternKey + "Progress", highProgress);
            GameManager.myManager.um.ShowGameOverUI(true);
            Debug.Log($"Saved progress {highProgress} for {patternKey}");
        }
        else
        {
            GameManager.myManager.um.ShowGameOverUI(false);
        }
    }

    private void GameClear() {
        if (isTutorial) GameManager.myManager.um.DeactivateKeyboard();

        state = RhythmState.GameClear;
        song.Stop();
        //Time.timeScale = 0f;
        GameManager.myManager.im.Deactivate();

        if (score > highScore)
        {
            var patternKey = GameManager.myManager.selectedSongName + '_' + GameManager.myManager.selectedDifficulty;
            highProgress = 100;
            highScore = score;
            PlayerPrefs.SetInt(patternKey + "Progress", highProgress);
            PlayerPrefs.SetInt(patternKey + "Score", highScore);
            GameManager.myManager.um.ShowResultUI(true);
            Debug.Log($"Saved score {highScore} for {patternKey}");
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
        CharacterDirection endNoteDirection = CharacterDirection.Right;

        foreach (var note in noteList) { 
            AnchorPosition = SpawnNote(note, AnchorPosition);
            endNoteDirection = note.direction;
        }

        GameObject lastNote = Instantiate(notePrefabs[0], AnchorPosition, Quaternion.identity);
        lastNote.GetComponent<Note>().destPos = AnchorPosition + (int)endNoteDirection * 0.32f * Vector3.left;
        lastNote.transform.localScale = new Vector3((int)endNoteDirection, 1f, 1f);
        lastNote.GetComponent<Note>().FixNote();
        lastNote.GetComponentInChildren<SpriteRenderer>().size = new Vector3(170f, 2.5f, 1f);

        if (isTutorial) {
            List<GameObject> list = preSpawnedNotes.ToList();
            int[] popIndexes = new int[] { 12, 9, 7, 5, 3, 0 };
            foreach(int index in popIndexes) {
                GameObject deactivateNote = list[index];
                deactivateNote.GetComponent<Note>().FixNote();
                deactivateNote.GetComponent<Note>().tutorialDisable = true;
                deactivateNote.SetActive(true);
            }
            preSpawnedNotes = new Queue<GameObject>();
            foreach (GameObject leftNote in list) {
                preSpawnedNotes.Enqueue(leftNote);
            }
        }
    }

    // 노트 찍고 다음 AnchorPosition 돌려주는 역할
    private Vector3 SpawnNote(NoteSpawnInfo info, Vector3 AnchorPosition)
    {
        
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
        if (type == NoteType.Normal && subType == NoteSubType.End) notePrefabIndex = 18;
            // Should be fixed later, but from now on I'll just hard-code these due to time lack
            // Me too

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
                30 or -30 => new Vector2(10 * inputWidth, 8.27f),
                45 or -45 => new Vector2(10 * inputWidth, 12.5f),
                60 or -60 => new Vector2(10 * inputWidth, 19.82f),
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
        note.noteSubType = subType;
        note.noteEndTime = info.spawnTime + info.noteLastingTime;
        // note.noteEndTime = noteList.IndexOf(note) == noteList.Count - 1 ? note.spawnTime + 1 : noteList[noteList.IndexOf(note) + 1].spawnTime;

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

    private IEnumerator LatelyStartSong() {
        double songStartTiming = -(double)(GameManager.myManager.globalOffset + levelOffset) / 1000;
        yield return new WaitForSeconds(1f);
        song.volume = GameManager.myManager.musicVolume;
        song.PlayScheduled(AudioSettings.dspTime + songStartTiming - gameTime);
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

        realScore += rate * scorePerNotes;
        score = (int) (realScore + 0.5);
    }

    // -방식 스코어 업데이트
    private void UpdateMinusScore(JudgementType judgement)
    {
        UpdateScore(judgement);

        double tempScore = 1.01 * scorePerNotes * (myNoteCount - playedNotes) + realScore;
        minusScore = (int)(tempScore + 0.5);
    }

    // 퍼센트 업데이트
    private void UpdatePercentage()
    {
        playedNotes += 1;
        progress = 100 * playedNotes / myNoteCount;
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

    public void OnPause() { //Pressed Esc Button
        switch (state)
        {
            case RhythmState.BeforeGameStart:
            case RhythmState.Ingame:
                GameManager.myManager.sm.PlaySFX("Button");
                Time.timeScale = 0f;
                song.Pause();
                if (pauseCoroutine != null) StopCoroutine(pauseCoroutine);
                if (pauseUICoroutine != null) StopCoroutine(pauseUICoroutine);
                state = RhythmState.Paused;
                GameManager.myManager.im.Deactivate();
                GameManager.myManager.um.OpenPauseUI();
                break;
            case RhythmState.Paused:
                GameManager.myManager.sm.PlaySFX("Button");
                if (pauseCoroutine != null) StopCoroutine(pauseCoroutine);
                if (pauseUICoroutine != null) StopCoroutine(pauseUICoroutine);
                pauseCoroutine = ReturnToGame();
                GameManager.myManager.im.Activate();
                pauseUICoroutine = GameManager.myManager.um.ShowCountdownUIForContinue();
                StartCoroutine(pauseCoroutine);
                break;
            case RhythmState.GameOver:
            case RhythmState.GameClear:
                break;
            default:
                break;
        }
    }
    public void OnMain() { // Pressed Enter Button
        if (state == RhythmState.Paused || state == RhythmState.GameClear || state == RhythmState.GameOver) GameManager.myManager.um.OnClickMusicSelectButton();
    }


    public void OnRestart() // Pressed Space Button 
    {
        if (state != RhythmState.Paused) return;
        GameManager.myManager.sm.PlaySFX("Button");
        GameManager.myManager.im.Deactivate();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelTest");
    }

    private IEnumerator ReturnToGame() {
        if (gameTime < -1f) state = RhythmState.BeforeGameStart;
        else state = RhythmState.Ingame;
        StartCoroutine(pauseUICoroutine);
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
        song.Play();
        pauseCoroutine = null;
        pauseUICoroutine = null;
    }

    public GameObject DequeueNoteFromQueue()
    {
        GameObject dequeuedNote = spawnedNotes.Dequeue();

        CurrentPlayingNote = dequeuedNote.GetComponent<Note>();

        return dequeuedNote;
    }

    private IEnumerator StartParticle() {
        GameObject particle;
        while (true) {
            if (state == RhythmState.Ingame && gameTime > 0f)
            {
                particle = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity, player.transform);
                particle.transform.localPosition = particleLocalPos;
                particle.transform.localRotation = Quaternion.identity;
                particle.GetComponent<ParticleSystem>().Play();
                particle.transform.parent = null;
                yield return new WaitForSeconds(0.03f);
            }
            else {
                yield return new WaitUntil(() => state == RhythmState.Ingame && gameTime > 0f);
            }
        }
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
