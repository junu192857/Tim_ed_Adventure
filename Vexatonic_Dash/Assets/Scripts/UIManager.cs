using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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

    private static int Score => GameManager.myManager.rm.score;
    private static int Progress => GameManager.myManager.rm.progress;
    private static JudgementType LastJudge => GameManager.myManager.rm.lastJudge;
    private static int[] JudgementList => GameManager.myManager.rm.judgementList;
    
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
    
    public void ShowGameOverUI(bool isNewRecord)
    {
        if (isNewRecord)
        {
            gameOverTitleDefault.SetActive(false);
            gameOverTitleNewRecord.SetActive(true);
        }
        else
        {
            gameOverTitleDefault.SetActive(true);
            gameOverTitleNewRecord.SetActive(false);
        }
        
        gameOverProgress.text = Progress + " %";
        
        gameOverPanel.SetActive(true);    // TODO: Add show animation
    }

    // Displays judge when note is hit or missed
    public void DisplayJudge()
    {
        var instance = Instantiate(judgeTextPrefab, judgeTextParent.transform);

        instance.GetComponent<Text>().text = LastJudge switch
        {
            JudgementType.PurePerfect => "Perfect",
            JudgementType.Perfect     => "Perfect",
            JudgementType.Great       => "Great",
            JudgementType.Good        => "Good",
            JudgementType.Miss        => "Miss",
            JudgementType.Invalid     => "Invalid",
            _ => throw new System.ArgumentException()
        };
    }
}
