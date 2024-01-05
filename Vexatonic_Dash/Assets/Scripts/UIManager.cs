using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const float JudgeDisplayTime = 0.5f;
    
    private static RhythmManager Rm => GameManager.myManager.rm;

    [Header ("In-Game UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text progressText;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultRankText;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text resultSongNameText;
    [SerializeField] private Text resultComposerNameText;
    
    [Space (10)]
    [SerializeField] private Text resultPurePerfectText;
    [SerializeField] private Text resultPerfectText;
    [SerializeField] private Text resultGreatText;
    [SerializeField] private Text resultGoodText;
    
    [Header ("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameOverTitleDefault;
    [SerializeField] private GameObject gameOverTitleNewRecord;
    [SerializeField] private Text gameOverProgress;
    
    [Header ("Judge Text")]
    [SerializeField] private GameObject judgeTextParent;
    [SerializeField] private GameObject judgeTextPrefab;

    [Header ("Song Info")]
    public string songName;
    public string composerName;

    private static int Score => Rm.score;
    private static int Combo => Rm.combo;
    private static int Progress => Rm.progress;
    private static int[] JudgementList => Rm.judgementList;
    
    private void Start()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        scoreText.text = "0";
        progressText.text = "0 %";
    }

    public void UpdateInGameUI()
    {
        scoreText.text = Score.ToString();
        progressText.text = Progress + " %";
    }

    public void ShowResultUI()
    {
        switch (Score)  // Set rank text
        {
            // TODO: Add conditions for each rank
            default:
                resultRankText.text = "SSS";
                resultRankText.color = new Color(1f, 0.75f, 0f);
                break;
        }
        
        resultScoreText.text = Score.ToString();
        resultSongNameText.text = songName;
        resultComposerNameText.text = composerName;
        
        // Set Judgement texts
        resultPurePerfectText.text = JudgementList[0].ToString();
        resultPerfectText.text = JudgementList[1].ToString();
        resultGreatText.text = JudgementList[2].ToString();
        resultGoodText.text = JudgementList[3].ToString();

        resultPanel.SetActive(true);    // TODO: Add show animation
    }
    
    public void ShowGameOverUI()
    {
        if (true) // TODO: Detect whether it is a new record or not
        {
            gameOverTitleDefault.SetActive(true);
            gameOverTitleNewRecord.SetActive(false);
        }
        else      // New Record
        {
            gameOverTitleDefault.SetActive(false);
            gameOverTitleNewRecord.SetActive(true);
        }
        
        gameOverProgress.text = Progress + " %";
        
        gameOverPanel.SetActive(true);    // TODO: Add show animation
    }

    // Displays judge when note is hit or missed
    public void DisplayJudge()
    {
        var instance = Instantiate(judgeTextPrefab, judgeTextParent.transform);
    }
}
