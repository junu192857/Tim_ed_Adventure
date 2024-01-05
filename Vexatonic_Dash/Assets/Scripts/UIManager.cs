using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const float JudgeDisplayTime = 0.5f;
    
    private static RhythmManager Rm => GameManager.myManager.rm;

    public Text score;
    public Text combo;
    public Text songNameText;
    public Text composerNameText;

    public string songName;
    public string composerName;

    [SerializeField] private GameObject judgeTextParent;
    [SerializeField] private GameObject judgeTextPrefab;

    public void Start()
    {
        InitiateUI();
    }
    public void InitiateUI() {
        score.text = "0";
        combo.text = "";
        songNameText.text = songName;
        composerNameText.text = composerName;
    }

    public void UpdateIngameUI() {
        score.text = Rm.score.ToString();
        combo.text = Rm.combo.ToString();
        StartCoroutine(DisplayJudge());
    }

    public void UpdateResultUI()
    {
        // TODO
    }
    
    public void UpdateGameOverUI()
    {
        // TODO
    }

    // Displays judge when note is hit or missed
    public IEnumerator DisplayJudge()
    {
        var instance = Instantiate(judgeTextPrefab, judgeTextParent.transform);
        
        instance.GetComponent<Text>().text = Rm.lastJudge.ToString();

        yield return new WaitForSeconds(JudgeDisplayTime);
        
        Destroy(instance);
    }
}
