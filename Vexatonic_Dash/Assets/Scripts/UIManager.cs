using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");

    [Header("In-Game UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text fpsText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthImage;

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
    [SerializeField] private Button musicSelectButton;

    [Space(10)]
    [SerializeField] private Text resultPurePerfectText;
    [SerializeField] private Text resultPerfectText;
    [SerializeField] private Text resultGreatText;
    [SerializeField] private Text resultGoodText;
    [SerializeField] private Text resultMissText;

    [Header("Judgement & Combo")]
    private GameObject myParent;
    [SerializeField] private GameObject judgeParent;
    [SerializeField] private GameObject resultPurePerfect;
    [SerializeField] private GameObject resultPerfect;
    [SerializeField] private GameObject resultGreat;
    [SerializeField] private GameObject resultGood;
    [SerializeField] private GameObject resultMiss;
    [SerializeField] private List<GameObject> numbers;
    private readonly Vector3 distFromParent =  Vector3.up * 0.1f;

    [Header("Pause UI")]
    [SerializeField] private GameObject pause;

    [Header ("Game Over UI")]
    [SerializeField] private GameObject gameOver;
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text gameOverProgressText;

    [Header ("Song Info")]
    public string songName;
    public string composerName;
    public Difficulty difficulty;

    private int _highProgress;
    private int _highScore;
    
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
        InitializeUI();
    }
    
    private void InitializeUI()
    {
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
        ShowCountdownUI();
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
                Instantiate(numbers[combo / 1000],  myParent.transform).transform.localPosition = distFromParent - 0.3f * Vector3.right;
                Instantiate(numbers[combo / 100 % 10], myParent.transform).transform.localPosition = distFromParent - 0.1f * Vector3.right;
                Instantiate(numbers[combo / 10 % 10], myParent.transform).transform.localPosition = distFromParent + 0.1f * Vector3.right;
                Instantiate(numbers[combo % 10], myParent.transform).transform.localPosition = distFromParent + 0.3f * Vector3.right;
                break;
            default:
                break;
        }

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
        GameManager.myManager.im.Deactivate();
        Time.timeScale = 1f;
        GameManager.myManager.sm.PlaySFX("Button");
        SceneManager.LoadScene("Scenes/Select");
    }

    public void OpenPauseUI() => pause.SetActive(true);

    public void ClosePauseUI() => pause.SetActive(false);
}
