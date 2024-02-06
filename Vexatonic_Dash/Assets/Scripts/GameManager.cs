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

    public static GameManager myManager;

    public float scrollSpeed; // 카메라 이동 속도를 의미한다.
    public float noteSpeed; // 노트의 속력을 의미한다.
    public float notePosition; // 판정선의 위치를 의미한다.
    public float volume; // 소리 크기를 의미한다.

    public string filepath; // 레벨의 맵 파일 위치를 의미한다.
    // 맵 하나 추가할 때마다 업데이트는 불가능 --> filepath도 외부에 있어야 한다.

    public RhythmManager rm;
    public UIManager um;
    public InputManager im;

    public string selectedSongName;
    public string selectedComposerName;

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
        if (scrollSpeed == 0f) scrollSpeed = 1f;

        SongsDirectory = Application.dataPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + LevelsDirectoryName;
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
        >= 950000  and < 1000000 => RankType.A,
        >= 900000  and < 950000  => RankType.B,
        >= 800000  and < 900000  => RankType.C,
        >= 0       and < 800000  => RankType.D,
        _ => throw new ArgumentOutOfRangeException()
    };

    public float CalculateInputWidthFromTime(double time) =>  scrollSpeed * 2 * (float)time;
    public double CalculateTimeFromInputWidth(float width) => width / (2 * scrollSpeed);

    public const float g = -9.8f;
    public int gravity = 0;

    public Vector3 GravityAsVector => g * (Quaternion.AngleAxis(gravity, Vector3.forward) * Vector3.up);
}
