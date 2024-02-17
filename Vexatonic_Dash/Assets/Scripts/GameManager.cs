using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum RankType
{
    V,
    S,
    A,
    B,
    C,
    D,
}

public class GameManager : MonoBehaviour
{
    private const string LevelsDirectoryName = "Levels";
    public static string SongsDirectory;

    public const float MinNoteSpeed = 1.0f;
    public const float MaxNoteSpeed = 20.0f;
    
    public static GameManager myManager;

    public bool isTutorial;

    public float scrollSpeed; // 카메라 이동 속도를 의미한다.
    public float noteSpeed; // 노트의 속력을 의미한다.
    public int globalOffset = 0; // 인겜 setting에서 설정할 수 있는 음악 오프셋이다.
    public double levelOffset; // 에디터를 통해 정한 각 레벨에서의 오프셋이다.
    public float musicVolume; // 음악 크기를 의미한다. (0 ~ 1)
    public float sfxVolume; // 효과음 크기를 의미한다. (0 ~ 1)

    public List<KeyCode> keyList;

    [HideInInspector] public bool isMetaLoaded = false;

    public string filepath; // 레벨의 맵 파일 위치를 의미한다.
    // 맵 하나 추가할 때마다 업데이트는 불가능 --> filepath도 외부에 있어야 한다.

    public RhythmManager rm;
    public UIManager um;
    public InputManager im;
    public SoundManager sm;

    public string selectedSongName;
    public string selectedComposerName;
    public Difficulty selectedDifficulty;

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
        if (scrollSpeed == 0f) scrollSpeed = 1.5f;

        SongsDirectory = Application.streamingAssetsPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + LevelsDirectoryName;

        noteSpeed = 3f;

        keyList = new List<KeyCode>()
        {
            KeyCode.F,
            KeyCode.J,
            KeyCode.D,
            KeyCode.K,
            KeyCode.Space,
        };
        
        musicVolume = 1f;
        sfxVolume = 1f;
    }

    public void Start()
    {
        MetaReader.GetSongMeta();
    }

    /// <summary>
    /// Get progress and score of the song from PlayerPrefs.
    /// </summary>
    /// <returns>(progress, score)</returns>
    public static (int, int) GetScore(string songName, Difficulty difficulty)
    {
        var patternKey = songName + '_' + difficulty;
        var progress = PlayerPrefs.GetInt(patternKey + "Progress", -1);

        switch (progress)
        {
            // Cleared
            case 100:
                var score = PlayerPrefs.GetInt(patternKey + "Score", -1);

                if (score is < 0 or > 1010000)
                {
                    Debug.LogError($"Invalid score {score} for {patternKey}");
                    return (100, -1);
                }

                Debug.Log($"Loaded score {score} for {patternKey}");
                return (100, score);
            
            // Played, not cleared
            case >= 0 and < 100:
                Debug.Log($"Loaded progress {progress} for {patternKey}");
                return (progress, 0);
            
            // Not found
            case -1:
                Debug.Log($"No score found for {patternKey}");
                return (0, 0);
            
            // Invalid
            default:
                Debug.LogError($"Invalid progress {progress} for {patternKey}");
                return (-1, 0);
        }
    }

    public static RankType GetRank(int score) => score switch
    {
        1010000                  => RankType.V,
        >= 1000000 and < 1010000 => RankType.S,
        >= 900000  and < 1000000 => RankType.A,
        >= 800000  and < 900000  => RankType.B,
        >= 700000  and < 800000  => RankType.C,
        >= 0       and < 700000  => RankType.D,
        _ => throw new ArgumentOutOfRangeException()
    };

    public float CalculateInputWidthFromTime(double time) =>  scrollSpeed * 2 * (float)time;
    public double CalculateTimeFromInputWidth(float width) => width / (2 * scrollSpeed);

    public const float g = -9.8f;
    public int gravity = 0;

    public Vector3 GravityAsVector => g * (Quaternion.AngleAxis(gravity, Vector3.forward) * Vector3.up);
}
