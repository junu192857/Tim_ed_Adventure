using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Text patternTypeText;

    private readonly List<SongData> _songList = new();
    private int _currentIndex;
    
    private PatternType _currentPatternType;
    private int _currentPatternDifficulty;

    private bool _songMoving;
    
    private void Start()
    {
        _currentIndex = 0;
        _currentPatternType = PatternType.Easy;
        _songMoving = false;
        
        // Song list initialization
        _songList.Clear();
        
        foreach (var song in SongListData.SongList)
        {
            _songList.Add(song);
        }
        
        SetSongListText();
        SetCurrentSongUI();
        SetCurrentPatternUI();
    }

    private void Update()
    {
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
            SwitchPatternType();
        }
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
        var progress = Random.Range(98, 101);                               // TODO: Get progress
        var score = (progress == 100) ? Random.Range(1000000, 1010001) : 0; // TODO: Get score
        
        _currentPatternDifficulty = _songList[_currentIndex].Difficulty[(int)_currentPatternType];
        
        var patternText = _currentPatternType switch
        {
            PatternType.Easy => "Easy",
            PatternType.Hard => "Hard",
            PatternType.Vex  => "Vex",
            _ => throw new System.ArgumentException()
        };

        patternText += " " + _currentPatternDifficulty;
        
        patternTypeText.text = patternText;
        
        rankImage.sprite = rankImages[(int)GameManager.GetRank(score)];
        
        if (score == 0)
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
        StartCoroutine(MoveSongCoroutine(true));
    }

    private void MoveDown()
    {
        if (_currentIndex == _songList.Count - 1) return;
        _currentIndex++;
        StartCoroutine(MoveSongCoroutine(false));
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

    private void SwitchPatternType()
    {
        _currentPatternType = _currentPatternType switch
        {
            PatternType.Easy => PatternType.Hard,
            PatternType.Hard => PatternType.Vex,
            PatternType.Vex => PatternType.Easy,
            _ => throw new System.ArgumentException()
        };
        
        SetCurrentPatternUI();
    }

    public void OnClickPatternSelectButton()
    {
        SwitchPatternType();
    }

    public void OnClickSelectBackButton()
    {
        SceneManager.LoadScene("Scenes/Main");
    }
}