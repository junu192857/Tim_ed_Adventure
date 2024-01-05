using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private RhythmManager rm;

    public Text score;
    public Text combo;
    public Text songNameText;
    public Text composerNameText;

    public string songName;
    public string composerName;

    public void Start()
    {
        rm = GameManager.myManager.rm;
        InitiateUI();
    }
    public void InitiateUI() {
        score.text = "0";
        combo.text = "";
        songNameText.text = songName;
        composerNameText.text = composerName;
    }

    public void UpdateUI() {
        score.text = rm.score.ToString();
        combo.text = rm.combo.ToString();
    }
}
