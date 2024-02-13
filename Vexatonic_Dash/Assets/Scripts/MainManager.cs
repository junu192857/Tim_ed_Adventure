using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainParent;
    
    [Header("Settings")]
    [SerializeField] private GameObject settingsParent;

    [Space(10)]
    [SerializeField] private GameObject videoSettingsParent;

    [Space(10)]
    [SerializeField] private GameObject audioSettingsParent;
    [SerializeField] private Text offsetValueText;
    
    [Space(10)]
    [SerializeField] private GameObject inputSettingsParent;

    [Space(10)]
    [SerializeField] private GameObject playSettingsParent;


    private void Start()
    {
        mainParent.SetActive(true);
        settingsParent.SetActive(false);
        videoSettingsParent.SetActive(false);
        audioSettingsParent.SetActive(false);
        inputSettingsParent.SetActive(false);
        playSettingsParent.SetActive(false);
        // TODO: Add animations
    }

    public void OnClickMainPlayButton()
    {
        mainParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        SceneManager.LoadScene("Scenes/Select");
        // TODO: Add animations
    }
    
    public void OnClickMainSettingsButton()
    {
        mainParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickMainQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickSettingsVideoButton()
    {
        videoSettingsParent.SetActive(true);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(false);
        // TODO: Add animations
    }
    
    public void OnClickSettingsAudioButton()
    {
        audioSettingsParent.SetActive(true);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(false);
        
        UpdateAudioOffsetValueText();
        // TODO: Add animations
    }

    public void OnClickSettingsInputButton()
    {
        inputSettingsParent.SetActive(true);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(false);
        // TODO: Add animations
    }

    public void OnClickSettingsPlayButton()
    {
        playSettingsParent.SetActive(true);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(false);
        // TODO: Add animations
    }

    public void OnClickSettingsBackButton()
    {
        settingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        mainParent.SetActive(true);
        // TODO: Add animations
    }

    public void OnClickVideoSettingsBackButton()
    {
        videoSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickAudioSettingsBackButton()
    {
        audioSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickInputSettingsBackButton()
    {
        inputSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickPlaySettingsBackButton()
    {
        playSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    #region Audio Settings

    private void UpdateAudioOffsetValueText()
    {
        offsetValueText.text = GameManager.myManager.globalOffset.ToString("+##0;-##0;0");
    }
    
    public void OnClickAudioOffsetUpButton()
    {
        GameManager.myManager.globalOffset += 1;
        UpdateAudioOffsetValueText();
    }
    
    public void OnClickAudioOffsetDownButton()
    {
        GameManager.myManager.globalOffset -= 1;
        UpdateAudioOffsetValueText();
    }
    
    #endregion
}
