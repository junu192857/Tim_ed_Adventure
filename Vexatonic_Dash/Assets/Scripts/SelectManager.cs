using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    private const float SongListSpace = 180f;
    private const float SongScrollTime = 0.1f;

    [Space(10)]
    [SerializeField] private Text highlightedSongNameText;
    [SerializeField] private Text highlightedSongComposerText;

    [Space(10)]
    [SerializeField] private RectTransform currentSongRect;
    [SerializeField] private RectTransform prevSong1Rect;
    [SerializeField] private RectTransform prevSong2Rect;
    [SerializeField] private RectTransform nextSong1Rect;
    [SerializeField] private RectTransform nextSong2Rect;
    [SerializeField] private Text currentSongText;
    [SerializeField] private Text prevSong1Text;
    [SerializeField] private Text prevSong2Text;
    [SerializeField] private Text nextSong1Text;
    [SerializeField] private Text nextSong2Text;
    
    [Space(10)]
    [SerializeField] private Sprite[] rankImages;   // TODO: Add rank images
    [SerializeField] private Image rankImage;
    [SerializeField] private Text highScoreDescriptionText;
    [SerializeField] private Text highScoreValueText;
    [SerializeField] private Text patternInfoText;

    private readonly List<SongData> _songList = new();
    private int _currentIndex;
    
    private Difficulty _currentDifficulty;
    private int _currentPatternLevel;

    private bool _songMoving;

    private bool _songListInvalid;

    private List<Coroutine> coroutines = new();
    
    private void Start()
    {
        _currentIndex = 0;
        _currentDifficulty = Difficulty.Easy;
        _songMoving = false;
        _songListInvalid = false;
        
        // Song list initialization
        _songList.Clear();
        
        foreach (var song in MetaReader.SongMetaList)
        {
            _songList.Add(song);
        }
        
        if (_songList.Count == 0)
        {
            Debug.LogError("No song found");
            _songListInvalid = true;
            highlightedSongNameText.text = "No song found";
            currentSongText.text = "";
            prevSong1Text.text = "";
            prevSong2Text.text = "";
            nextSong1Text.text = "";
            nextSong2Text.text = "";
            return;
        }
        
        SetSongListText();
        SetCurrentSongUI();
        SetCurrentPatternUI();
    }

    private void Update()
    {
        if (_songListInvalid) return;
        /*
        // Song scroll
        if (!_songMoving)
        {
            switch (Input.GetKeyDown(KeyCode.UpArrow), Input.GetKeyDown(KeyCode.DownArrow))
            {
                case (true, false):
                    MoveUp();
                    break;
                case (false, true):
                    MoveDown();
                    break;
            }
        }
        
        // Pattern type select
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchDifficulty();
        }
        */
    }

    public void OnChangeSong(InputValue inputValue)
    {
        float input = inputValue.Get<float>();
        Debug.Log(_songList.Count);
        if (input < 0)
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }
    }

    public void OnSwitchDifficulty()
    {
        SwitchDifficulty();
    }

    private void SetSongListText()
    {
        currentSongText.text = _songList[_currentIndex].SongName;
        prevSong1Text.text = (_currentIndex >= 1) ? _songList[_currentIndex - 1].SongName : "";
        prevSong2Text.text = (_currentIndex >= 2) ? _songList[_currentIndex - 2].SongName : "";
        nextSong1Text.text = (_currentIndex <= _songList.Count - 2) ? _songList[_currentIndex + 1].SongName : "";
        nextSong2Text.text = (_currentIndex <= _songList.Count - 3) ? _songList[_currentIndex + 2].SongName : "";
    }

    private void SetCurrentSongUI()
    {
        highlightedSongNameText.text = _songList[_currentIndex].SongName;
        highlightedSongComposerText.text = _songList[_currentIndex].ComposerName;
    }

    private void SetCurrentPatternUI()
    {
        (int progress, int score) = GameManager.GetScore(_songList[_currentIndex].SongName, _currentDifficulty);
        
        _currentPatternLevel = _songList[_currentIndex].Levels[(int)_currentDifficulty];
        
        var patternText = _currentDifficulty switch
        {
            Difficulty.Easy => "Easy",
            Difficulty.Hard => "Hard",
            Difficulty.Vex  => "Vex",
            _ => throw new System.ArgumentException()
        };

        patternText += " " + _currentPatternLevel;
        
        patternInfoText.text = patternText;
        
        rankImage.sprite = rankImages[(int)GameManager.GetRank(score)];
        
        if (progress == 100)
        {
            highScoreDescriptionText.text = "Progress";
            highScoreValueText.text = $"{progress} %";
        }
        else
        {
            highScoreDescriptionText.text = "Score";
            highScoreValueText.text = $"{score}";
        }
    }

    private void MoveUp()
    {
        if (_currentIndex == 0) return;
        _currentIndex--;
        Debug.Log("current Index" + _currentIndex.ToString());
        coroutines.Add(StartCoroutine(MoveSongCoroutine(true)));
        coroutines.Add(StartCoroutine(GameManager.myManager.sm.PlaySelectedSong(_currentIndex)));
    }

    private void MoveDown()
    {
        if (_currentIndex == _songList.Count - 1) return;
        _currentIndex++;
        Debug.Log("current Index" + _currentIndex.ToString());
        coroutines.Add(StartCoroutine(MoveSongCoroutine(false)));
        coroutines.Add(StartCoroutine(GameManager.myManager.sm.PlaySelectedSong(_currentIndex)));
    }

    private IEnumerator MoveSongCoroutine(bool up)
    {
        _songMoving = true;
        var elapsedTime = 0f;
        
        while (elapsedTime < SongScrollTime)
        {
            var deltaPosition = Mathf.Lerp(0, up ? -SongListSpace : SongListSpace, elapsedTime / SongScrollTime);
            
            currentSongRect.anchoredPosition = new Vector2(0f, deltaPosition);
            prevSong1Rect.anchoredPosition = new Vector2(0f, 180f + deltaPosition);
            prevSong2Rect.anchoredPosition = new Vector2(0f, 360f + deltaPosition);
            nextSong1Rect.anchoredPosition = new Vector2(0f, -180f + deltaPosition);
            nextSong2Rect.anchoredPosition = new Vector2(0f, -360f + deltaPosition);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
            
        currentSongRect.anchoredPosition = new Vector2(0f, 0f);
        prevSong1Rect.anchoredPosition = new Vector2(0f, 180f);
        prevSong2Rect.anchoredPosition = new Vector2(0f, 360f);
        nextSong1Rect.anchoredPosition = new Vector2(0f, -180f);
        nextSong2Rect.anchoredPosition = new Vector2(0f, -360f);
        
        SetSongListText();
        SetCurrentSongUI();
        SetCurrentPatternUI();
        _songMoving = false;
    }

    private void SwitchDifficulty()
    {
        _currentDifficulty = _currentDifficulty switch
        {
            Difficulty.Easy => Difficulty.Hard,
            Difficulty.Hard => Difficulty.Vex,
            Difficulty.Vex  => Difficulty.Easy,
            _ => throw new System.ArgumentException()
        };
        
        SetCurrentPatternUI();
    }

    public void OnClickPatternSelectButton()
    {
        SwitchDifficulty();
    }

    public void StartGame()
    {
        if (_songMoving) return;
        
        SongData selectedSong = _songList[_currentIndex];

        GameManager.myManager.filepath = selectedSong.PatternFilePath[(int)_currentDifficulty];
        GameManager.myManager.selectedComposerName = selectedSong.ComposerName;
        GameManager.myManager.selectedSongName = selectedSong.SongName;

        foreach (var c in coroutines)
        {
            StopCoroutine(c);
        }
        coroutines.Clear();
        SceneManager.LoadScene("Scenes/LevelTest");
    }

    public void OnClickBackButton()
    {
        SceneManager.LoadScene("Scenes/Main");
    }
}
