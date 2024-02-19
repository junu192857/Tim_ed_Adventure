using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");
    
    private const float SongListSpace = 180f;
    private const float SongScrollTime = 0.05f;

    [Header("Highlighted Song")]
    [SerializeField] private Text highlightedSongNameText;
    [SerializeField] private Text highlightedSongComposerText;
    [SerializeField] private Text highlightedSongEasyText;
    [SerializeField] private Text highlightedSongHardText;
    [SerializeField] private Text highlightedSongVexText;

    [Header("Song List")]
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
    
    [Header("Pattern Info")]
    [SerializeField] private RankIcon rankIcon;
    [SerializeField] private Text highScoreDescriptionText;
    [SerializeField] private Text highScoreValueText;

    [Space(10)]
    [SerializeField] private Image patternInfoImage;
    [SerializeField] private Text patternInfoText;
    [SerializeField] private Button patternSelectButton;
    [SerializeField] private Button StartButton;

    [Header("Animator")]
    [SerializeField] private Animator titleTextAnim;
    [SerializeField] private Animator songsParentAnim;
    [SerializeField] private Animator highlightedSongAnim;
    [SerializeField] private Animator eventInfoAnim;
    [SerializeField] private Animator rankIconAnim;
    [SerializeField] private Animator highScoreAnim;
    [SerializeField] private Animator patternInfoAnim;
    [SerializeField] private Animator startButtonAnim;
    [SerializeField] private Animator backButtonAnim;

    private readonly List<SongData> _songList = new();
    private int _currentIndex;
    
    private Difficulty _currentDifficulty;
    private int _currentPatternLevel;

    private bool _songMoving;
    private bool _songListInvalid;
    private bool _isAnimationPlaying;

    private List<Coroutine> coroutines = new();
    
    private readonly List<AudioClip> _audioClips = new();
    
    private void Start()
    {
        _currentIndex = 0;
        _currentDifficulty = Difficulty.Easy;
        _songMoving = false;
        _songListInvalid = false;
        _isAnimationPlaying = true;
        
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
        
        // Select last played pattern
        _currentIndex = _songList.FindIndex(s =>
            s.SongName == GameManager.myManager.selectedSongName &&
            s.ComposerName == GameManager.myManager.selectedComposerName);
        
        if (_currentIndex != -1)
        {
            _currentDifficulty = GameManager.myManager.selectedDifficulty;
            Debug.Log($"Last played song index: {_currentIndex}, difficulty: {_currentDifficulty}");
        }
        else
        {
            _currentIndex = 0;
        }

        StartCoroutine(SelectEnterCoroutine());
    }

    private IEnumerator SelectEnterCoroutine()
    {
        foreach (var song in _songList)
        {
            string fullPath = "file://" + song.AudioFilePath;
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG);
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.ConnectionError) Debug.LogError(uwr.error);
            else
            {
                try
                {
                    _audioClips.Add(DownloadHandlerAudioClip.GetContent(uwr));
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    _audioClips.Add(null);
                }
            }

            yield return null;
        }
        
        SetSongListText();
        SetCurrentSongUI();
        SetCurrentPatternUI();

        StartCoroutine(SelectShowAnimation());
        GameManager.myManager.sm.PlaySelectedSong(_audioClips[_currentIndex]);
    }

    private IEnumerator SelectShowAnimation()
    {
        _isAnimationPlaying = true;
        
        yield return new WaitForEndOfFrame();
        
        titleTextAnim.SetTrigger(AnimShowHash);
        songsParentAnim.SetTrigger(AnimShowHash);
        highlightedSongAnim.SetTrigger(AnimShowHash);
        rankIconAnim.SetTrigger(AnimShowHash);
        highScoreAnim.SetTrigger(AnimShowHash);
        patternInfoAnim.SetTrigger(AnimShowHash);
        startButtonAnim.SetTrigger(AnimShowHash);
        backButtonAnim.SetTrigger(AnimShowHash);
        
        yield return new WaitUntil(() => titleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        
        _isAnimationPlaying = false;
    }

    private IEnumerator SelectHideAnimation()
    {
        _isAnimationPlaying = true;
        
        yield return new WaitForEndOfFrame();
        
        titleTextAnim.SetTrigger(AnimHideHash);
        songsParentAnim.SetTrigger(AnimHideHash);
        highlightedSongAnim.SetTrigger(AnimHideHash);
        rankIconAnim.SetTrigger(AnimHideHash);
        highScoreAnim.SetTrigger(AnimHideHash);
        patternInfoAnim.SetTrigger(AnimHideHash);
        startButtonAnim.SetTrigger(AnimHideHash);
        backButtonAnim.SetTrigger(AnimHideHash);
        eventInfoAnim.SetTrigger(AnimHideHash);
        
        yield return new WaitUntil(() => titleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
        
        _isAnimationPlaying = false;
    }

    private IEnumerator EventSongShowAnimation()
    {
        if (eventInfoAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
            eventInfoAnim.GetCurrentAnimatorStateInfo(0).IsName("Show")) yield break;
        
        yield return new WaitForEndOfFrame();
        
        eventInfoAnim.SetTrigger(AnimShowHash);
    }
    
    private IEnumerator EventSongHideAnimation()
    {
        if (eventInfoAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden") ||
            eventInfoAnim.GetCurrentAnimatorStateInfo(0).IsName("Hide")) yield break;
        
        yield return new WaitForEndOfFrame();
        
        eventInfoAnim.SetTrigger(AnimHideHash);
    }

    public void OnChangeSong(InputValue inputValue)
    {
        if (_songListInvalid) return;
        
        float input = inputValue.Get<float>();
        if (input < 0)
        {
            MoveUp();
        }
        else
        {
            MoveDown();
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
        var currentSong = _songList[_currentIndex];
        highlightedSongNameText.text = currentSong.SongName;
        highlightedSongComposerText.text = currentSong.ComposerName;
        highlightedSongEasyText.text = currentSong.Levels[0].ToString();
        highlightedSongHardText.text = currentSong.Levels[1].ToString();
        highlightedSongVexText.text = currentSong.Levels[2].ToString();
        coroutines.Add(StartCoroutine(currentSong.IsEvent ? EventSongShowAnimation() : EventSongHideAnimation()));
    }

    private void SetCurrentPatternUI()
    {
        (int progress, int score) = GameManager.GetScore(_songList[_currentIndex].SongName, _currentDifficulty);
        
        _currentPatternLevel = _songList[_currentIndex].Levels[(int)_currentDifficulty];
        
        rankIcon.SetRank(GameManager.GetRank(score));
        rankIcon.ShowAnimation();
        
        if (progress == 100)
        {
            highScoreDescriptionText.text = "Score";
            highScoreValueText.text = $"{score}";
        }
        else
        {
            highScoreDescriptionText.text = "Progress";
            highScoreValueText.text = $"{progress} %";
        }

        patternInfoImage.color = _currentDifficulty switch
        {
            Difficulty.Easy => new Color(0.75f, 1.00f, 0.75f),
            Difficulty.Hard => new Color(1.00f, 1.00f, 0.75f),
            Difficulty.Vex  => new Color(1.00f, 0.75f, 0.75f),
            _ => throw new System.ArgumentException()
        };
        patternInfoText.text = _currentDifficulty + " " + _currentPatternLevel;
    }

    private void MoveUp()
    {
        if (_currentIndex == 0) return;
        _currentIndex--;
        Debug.Log("current Index" + _currentIndex.ToString());
        coroutines.Add(StartCoroutine(MoveSongCoroutine(true)));
        GameManager.myManager.sm.PlaySelectedSong(_audioClips[_currentIndex]);
    }

    private void MoveDown()
    {
        if (_currentIndex == _songList.Count - 1) return;
        _currentIndex++;
        Debug.Log("current Index" + _currentIndex.ToString());
        coroutines.Add(StartCoroutine(MoveSongCoroutine(false)));
        GameManager.myManager.sm.PlaySelectedSong(_audioClips[_currentIndex]);
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

    private void StartGame()
    {
        if (_songMoving || _isAnimationPlaying) return;
        
        SongData selectedSong = _songList[_currentIndex];

        GameManager.myManager.filepath = selectedSong.PatternFilePath[(int)_currentDifficulty];
        GameManager.myManager.selectedComposerName = selectedSong.ComposerName;
        GameManager.myManager.selectedSongName = selectedSong.SongName;
        GameManager.myManager.selectedDifficulty = _currentDifficulty;

        /*
        foreach (var c in coroutines)
        {
            StopCoroutine(c);
        }
        */
        coroutines.Clear();
        GameManager.myManager.isTutorial = false;
        SceneManager.LoadScene("Scenes/LevelTest");
    }

    public void OnClickBackButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(SelectBack());
    }

    private IEnumerator SelectBack()
    {
        yield return StartCoroutine(SelectHideAnimation());
        SceneManager.LoadScene("Scenes/Main");
    }

    public void OnSwitchDifficulty()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        SwitchDifficulty();
    }

    public void OnStartGame()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartGame();
    }

    public void OnBackToMain() {
        OnClickBackButton();
    }
}
