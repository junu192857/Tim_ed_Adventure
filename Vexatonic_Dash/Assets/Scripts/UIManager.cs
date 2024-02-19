using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");

    [Header("Backgrounds")]
    [SerializeField] private List<Sprite> backgrounds;
    [SerializeField] private Image backgroundUI;

    [Header("In-Game UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text fpsText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthImage;
    [SerializeField] private RectTransform healthAnimationRect;
    [SerializeField] private List<GameObject> halos;

    [Header("Level Info UI")]
    [SerializeField] private GameObject levelInfo;
    [SerializeField] private Text levelInfoSongNameText;
    [SerializeField] private Text levelInfoComposerNameText;
    [SerializeField] private Animator levelInfoSongNameAnim;
    [SerializeField] private Animator levelInfoComposerNameAnim;

    [Header("Countdown UI")]
    [SerializeField] private GameObject countdown;
    [SerializeField] private Text countdownText;

    [Header("Result UI")]
    [SerializeField] private GameObject result;
    [SerializeField] private RankIcon rankIcon;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text resultSongNameText;
    [SerializeField] private Text resultComposerNameText;
    private bool _isResultAnimationFinished;

    [Space(5)]
    [SerializeField] private Text resultPurePerfectText;
    [SerializeField] private Text resultPerfectText;
    [SerializeField] private Text resultGreatText;
    [SerializeField] private Text resultGoodText;
    [SerializeField] private Text resultMissText;

    [Space(10)]
    [SerializeField] private Animator resultPanelAnim;
    [SerializeField] private Animator resultRankIconAnim;

    [Header("Judgement & Combo")]
    [SerializeField] private GameObject judgeParent;
    [SerializeField] private GameObject resultPurePerfect;
    [SerializeField] private GameObject resultPerfect;
    [SerializeField] private GameObject resultGreat;
    [SerializeField] private GameObject resultGood;
    [SerializeField] private GameObject resultMiss;
    [SerializeField] private List<GameObject> numbers;
    private readonly Vector3 distFromParent = Vector3.up * 0.1f;

    [Header("Pause UI")]
    [SerializeField] private GameObject pause;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOver;
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text gameOverProgressText;

    [Header("Song Info")]
    public string songName;
    public string composerName;
    public Difficulty difficulty;

    [Header("Tutorial")]
    private FileStream fs;
    private StreamReader sr;
    private List<TutorialInfo> infos;
    private Color c = new Color(1, 1, 1);
    [SerializeField] private Text tutorialText;
    private bool fadeinRunning = false;
    private bool fadeoutRunning = false;
    [SerializeField] private List<GameObject> TutorialIndicators;
    [SerializeField] private GameObject keyboard;
    [SerializeField] private List<GameObject> keyboardArrows;
    private List<int> fjArrowTimings = new List<int> { 18, 29, 45, 53, 121, 130 };
    private List<int> dkArrowTimings = new List<int> { 59, 70, 79, 86, 121, 130 };
    private List<int> spaceArrowTimings = new List<int> { 93, 104, 114, 118, 121, 130 };
    private bool IsTutorial => GameManager.myManager.rm.isTutorial;

    private static int Score => GameManager.myManager.rm.score;
    private static int Progress => GameManager.myManager.rm.progress;
    private static int[] JudgementList => GameManager.myManager.rm.judgementList;
    private static double GameTime => GameManager.myManager.rm.GameTime;
    private static JudgementType LastJudge => GameManager.myManager.rm.lastJudge;

    private static int Health => GameManager.myManager.rm.health;

    private void Awake()
    {
        GameManager.myManager.um = this;
        songName = GameManager.myManager.selectedSongName;
        composerName = GameManager.myManager.selectedComposerName;
        difficulty = GameManager.myManager.selectedDifficulty;
    }

    private void Start()
    {
        infos = new List<TutorialInfo>();
        tutorialText.gameObject.SetActive(false);
        InitializeUI();
        _isResultAnimationFinished = false;
    }

    private void InitializeUI()
    {
        Debug.Log(GameManager.myManager.selectedSongName);
        //Background part trash-like coded to submit game file as soon as possible. should must be fixed
        //원래는 레벨 폴더 안에 백그라운드 png 이미지도 있는 게 이상적
        backgroundUI.sprite = GameManager.myManager.selectedSongName switch
        {
            "Savage_Terminal" => backgrounds[1],
            "Reminiscence" => backgrounds[2],
            _ => backgrounds[0]
        };

        scoreText.text = "0";
        healthSlider.value = 1f;
        healthImage.color = new Color(0.5f, 1f, 0.5f);
        progressText.text = "0 %";
        StartCoroutine(ShowFPSCoroutine());
    }

    private IEnumerator LevelInfoUICoroutine()
    {
        levelInfo.SetActive(true);

        // Show animation
        levelInfoSongNameAnim.SetTrigger(AnimShowHash);
        yield return new WaitForSeconds(0.2f);
        levelInfoComposerNameAnim.SetTrigger(AnimShowHash);
        yield return new WaitForSeconds(1.8f);

        // Hide animation
        levelInfoComposerNameAnim.SetTrigger(AnimHideHash);
        levelInfoSongNameAnim.SetTrigger(AnimHideHash);
        yield return new WaitForSeconds(1f);

        levelInfo.SetActive(false);
        //ShowCountdownUI();
    }

    private IEnumerator CountdownUICoroutine()
    {
        countdown.SetActive(true);

        while (GameTime < 0)
        {
            countdownText.text = $"{-GameTime:0.0}";
            yield return null;
        }

        countdown.SetActive(false);
    }

    public void UpdateInGameUI()
    {
        scoreText.text = Score.ToString();
        healthSlider.value = Health / 100f;
        healthImage.color = Health switch
        {
            <= 20 => new Color(1f, 0.5f, 0.5f),
            <= 40 => new Color(1f, 0.75f, 0.5f),
            _ => new Color(0.5f, 1f, 0.5f)
        };
        progressText.text = Progress + " %";
    }

    public IEnumerator HealthBarAnimation(float dest)
    {
        yield return new WaitForSeconds(0.2f);

        var initialAnchorMax = healthAnimationRect.anchorMax.x;
        var startTime = Time.time;
        var progress = 0f;

        while (progress < 1f)
        {
            progress = (Time.time - startTime) / 0.2f;

            healthAnimationRect.anchorMax = new Vector2(Mathf.Lerp(initialAnchorMax, dest, progress), 1f);
            yield return null;
        }

        healthAnimationRect.anchorMax = new Vector2(dest, 1f);
    }

    public void ShowLevelInfoUI()
    {
        levelInfoSongNameText.text = GameManager.myManager.selectedSongName;
        levelInfoComposerNameText.text = GameManager.myManager.selectedComposerName;
        StartCoroutine(LevelInfoUICoroutine());
    }

    public void ShowCountdownUI()
    {
        countdownText.text = $"{-GameTime:0.0}";
        StartCoroutine(CountdownUICoroutine());
    }

    public IEnumerator ShowCountdownUIForContinue() {
        ClosePauseUI();
        countdown.SetActive(true);
        double pauseTime = -3;

        while (pauseTime < 0)
        {
            countdownText.text = $"{-pauseTime:0.0}";
            pauseTime += Time.unscaledDeltaTime;
            yield return null;
        }

        countdown.SetActive(false);
    }

    public void ShowResultUI(bool isNewRecord)
    {
        rankIcon.SetRank(GameManager.GetRank(Score));

        resultScoreText.text = Score.ToString();
        resultSongNameText.text = songName;
        resultComposerNameText.text = composerName;

        // Set Judgement texts
        resultPurePerfectText.text = JudgementList[0].ToString();
        resultPerfectText.text = JudgementList[1].ToString();
        resultGreatText.text = JudgementList[2].ToString();
        resultGoodText.text = JudgementList[3].ToString();
        resultMissText.text = JudgementList[4].ToString();

        result.SetActive(true);    // TODO: Add show animation

        StartCoroutine(ResultShowAnimation());
    }

    private IEnumerator ResultShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        resultPanelAnim.SetTrigger(AnimShowHash);

        yield return new WaitForSecondsRealtime(1.8f);

        _isResultAnimationFinished = true;
        resultRankIconAnim.SetTrigger(AnimShowHash);
        GameManager.myManager.sm.PlaySFX("Game Clear");
        Debug.Log("Result show animation finished");
    }

    public void ShowGameOverUI(bool isNewRecord)
    {
        if (isNewRecord)
        {
            gameOverTitleText.text = "NEW RECORD!";
            gameOverTitleText.color = new Color(1f, 1f, 0.5f);
        }
        else
        {
            gameOverTitleText.text = "GAME OVER";
            gameOverTitleText.color = new Color(1f, 0.5f, 0.5f);
        }

        gameOverProgressText.text = Progress + " %";

        gameOver.SetActive(true);    // TODO: Add show animation
    }

    // Displays judge when note is hit or missed
    public void DisplayJudge(Vector3 transformPosition, int combo)
    {

        GameObject myParent = Instantiate(judgeParent,
            transformPosition + Quaternion.AngleAxis(Camera.main.transform.rotation.z, Vector3.forward) * (0.7f * Vector3.up),
            Quaternion.identity);

        switch (LastJudge)
        {
            case JudgementType.PurePerfect:
                Instantiate(resultPurePerfect, myParent.transform).transform.localPosition = -distFromParent;
                break;
            case JudgementType.Perfect:
                Instantiate(resultPerfect, myParent.transform).transform.localPosition = -distFromParent;
                break;
            case JudgementType.Great:
                Instantiate(resultGreat, myParent.transform).transform.localPosition = -distFromParent;
                break;
            case JudgementType.Good:
                Instantiate(resultGood, myParent.transform).transform.localPosition = -distFromParent;
                break;
            case JudgementType.Miss:
                Instantiate(resultMiss, myParent.transform).transform.localPosition = -distFromParent;
                break;
            case JudgementType.Invalid:
                Debug.LogError("Invalid note should not be dealed with");
                break;
            default:
                throw new ArgumentException();
        }

        switch (combo) {
            case > 0 and < 10:
                Instantiate(numbers[combo], myParent.transform).transform.localPosition = distFromParent;
                break;
            case >= 10 and < 100:
                Instantiate(numbers[combo / 10], myParent.transform).transform.localPosition = distFromParent - 0.1f * Vector3.right;
                Instantiate(numbers[combo % 10], myParent.transform).transform.localPosition = distFromParent + 0.1f * Vector3.right;
                break;
            case >= 100 and < 1000:
                Instantiate(numbers[combo / 100], myParent.transform).transform.localPosition = distFromParent - 0.2f * Vector3.right;
                Instantiate(numbers[combo / 10 % 10], myParent.transform).transform.localPosition = distFromParent;
                Instantiate(numbers[combo % 10], myParent.transform).transform.localPosition = distFromParent + 0.2f * Vector3.right;
                break;
            case >= 1000 and < 10000:
                Instantiate(numbers[combo / 1000], myParent.transform).transform.localPosition = distFromParent - 0.3f * Vector3.right;
                Instantiate(numbers[combo / 100 % 10], myParent.transform).transform.localPosition = distFromParent - 0.1f * Vector3.right;
                Instantiate(numbers[combo / 10 % 10], myParent.transform).transform.localPosition = distFromParent + 0.1f * Vector3.right;
                Instantiate(numbers[combo % 10], myParent.transform).transform.localPosition = distFromParent + 0.3f * Vector3.right;
                break;
            default:
                break;
        }

        myParent.transform.localEulerAngles = Camera.main.transform.localEulerAngles;
    }

    //only regards damage is 20, should be fixed later..? but there is no 'later'
    public void HitAnimation(Vector3 transformPosition) {
        GameObject myParent = Instantiate(judgeParent,
            transformPosition + Quaternion.AngleAxis(Camera.main.transform.rotation.z, Vector3.forward) * (0.7f * Vector3.up),
            Quaternion.identity);

        GameObject minus = Instantiate(numbers[10], myParent.transform);
        minus.transform.localPosition = distFromParent - 0.2f * Vector3.right;
        minus.transform.GetComponent<SpriteRenderer>().color = new Color(1, 0.3f, 0.3f);

        GameObject _10digit = Instantiate(numbers[2], myParent.transform);
        _10digit.transform.localPosition =  distFromParent - 0.1f * Vector3.right;
        _10digit.transform.GetComponent<SpriteRenderer>().color = new Color(1, 0.3f, 0.3f);

        GameObject _1digit = Instantiate(numbers[0], myParent.transform);
        _1digit.transform.localPosition = distFromParent + 0.1f * Vector3.right;
        _1digit.transform.GetComponent<SpriteRenderer>().color = new Color(1, 0.3f, 0.3f);

        myParent.transform.localEulerAngles = Camera.main.transform.localEulerAngles;
    }
    private IEnumerator ShowFPSCoroutine() {
        float time = 0;
        int fps = 0;
        while (true) {
            time += Time.deltaTime;
            fps++;
            if (time > 1f) {
                fpsText.text = "FPS: " + fps;
                time = 0f;
                fps = 0;
            }
            yield return null;

        }
    }

    public void OnClickMusicSelectButton()
    {
        if (!_isResultAnimationFinished) return;
        
        GameManager.myManager.im.Deactivate();
        Time.timeScale = 1f;
        GameManager.myManager.sm.PlaySFX("Button");
        
        if (IsTutorial) SceneManager.LoadScene("Scenes/Main");
        else SceneManager.LoadScene("Scenes/Select");
    }


    public void OpenPauseUI() => pause.SetActive(true);

    public void ClosePauseUI() => pause.SetActive(false);

    public IEnumerator TutorialCoroutine() {
        while (infos.Count > 0) {
            if (GameTime >= infos[0].startTime - 0.4f && !fadeinRunning) {
                fadeinRunning = true;
                tutorialText.text = infos[0].guide;
                StartCoroutine(TextFadeIn());
            }
            else if (GameTime >= infos[0].endTime && fadeinRunning && !fadeoutRunning){
                fadeoutRunning = true;
                StartCoroutine(TextFadeOut());
            }
            if (fjArrowTimings.Count > 0 && GameTime > fjArrowTimings[0]) {
                keyboardArrows[0].SetActive(!keyboardArrows[0].activeSelf);
                fjArrowTimings.RemoveAt(0);
            }
            if (dkArrowTimings.Count > 0 && GameTime > dkArrowTimings[0])
            {
                keyboardArrows[1].SetActive(!keyboardArrows[1].activeSelf);
                dkArrowTimings.RemoveAt(0);
            }
            if (spaceArrowTimings.Count > 0 && GameTime > spaceArrowTimings[0])
            {
                keyboardArrows[2].SetActive(!keyboardArrows[2].activeSelf);
                spaceArrowTimings.RemoveAt(0);
            }

            yield return null;
        }
    }

    private IEnumerator TextFadeIn() {
        float time = 0f;
        while (time < 0.4f) {
            tutorialText.transform.localScale = new Vector3(time * 2.5f, time * 2.5f, 1f);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator TextFadeOut()
    {
        float time = 0f;
        while (time < 0.4f)
        {
            tutorialText.transform.localScale = new Vector3(1f - time * 2.5f, 1f - time * 2.5f, 1f);
            time += Time.deltaTime;
            yield return null;
        }
        infos.RemoveAt(0);
        fadeinRunning = false;
        fadeoutRunning = false;
    }

    public void ReadTutorial() {
        tutorialText.gameObject.SetActive(true);
        foreach (var obj in TutorialIndicators) obj.SetActive(true);
        keyboard.SetActive(true);
        fs = new FileStream(Application.streamingAssetsPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + "TutorialInfo.txt", FileMode.Open);
        sr = new StreamReader(fs);
        string guide;
        while (!sr.EndOfStream) { 
            guide = sr.ReadLine();
            var guideList = guide.Split('^');
            infos.Add(new TutorialInfo(float.Parse(guideList[0]), float.Parse(guideList[1]), guideList[2]));
        }
        sr.Close();
    }

    public void DeactivateKeyboard() => keyboard.SetActive(false);

    public void SpawnHalo(Note note) {
        int haloIndex = note.noteType switch
        {
            NoteType.Normal => 0,
            NoteType.Dash => 1,
            NoteType.Jump => 2,
            _ => throw new ArgumentException()
        };

        Vector3 haloPositionDelta = note.noteSubType switch
        {
            NoteSubType.Air or NoteSubType.Wall => Vector3.zero,
            NoteSubType.Ground or NoteSubType.End => new Vector3(0.17f, -0.13f),
            _ => throw new ArgumentException()
        };

        Instantiate(halos[haloIndex], note.startPos + haloPositionDelta, Quaternion.identity);
    }

    //public void FadeoutIndicator(int index) => StartCoroutine(TutorialIndicators[index].GetComponent<TutorialIndicatroBehaviour>().Fadeout());
}

struct TutorialInfo {
    public float startTime;
    public float endTime;
    public string guide;

    public TutorialInfo(float startTime, float endTime, string guide) {
        this.startTime = startTime;
        this.endTime = endTime;
        this.guide = guide.Replace("\\n", "\n");
    }
}