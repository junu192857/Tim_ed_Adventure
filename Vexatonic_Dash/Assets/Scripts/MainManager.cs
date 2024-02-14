using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainManager : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");

    [Header("Main")] [SerializeField] private GameObject mainParent;
    [SerializeField] private Animator mainTitleTextAnim;
    [SerializeField] private Animator mainPlayButtonAnim;
    [SerializeField] private Animator mainSettingsButtonAnim;
    [SerializeField] private Animator mainQuitButtonAnim;

    [Header("Settings")] [SerializeField] private GameObject settingsParent;

    [Space(10)] [SerializeField] private GameObject videoSettingsParent;

    [Space(10)] [SerializeField] private GameObject audioSettingsParent;
    [SerializeField] private Text offsetValueText;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Text sfxVolumeText;

    [Space(10)] [SerializeField] private GameObject inputSettingsParent;
    [SerializeField] private Text[] keyTexts;
    private int currentConfiguringKeyIndex;

    [Space(10)] [SerializeField] private GameObject playSettingsParent;


    private void Start()
    {
        mainParent.SetActive(true);
        settingsParent.SetActive(false);
        videoSettingsParent.SetActive(false);
        audioSettingsParent.SetActive(false);
        inputSettingsParent.SetActive(false);
        playSettingsParent.SetActive(false);
        
        // Key Initialization
        currentConfiguringKeyIndex = 0;

        StartCoroutine(MainShowAnimation());
    }

    private IEnumerator MainShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        mainTitleTextAnim.SetTrigger(AnimShowHash);
        mainPlayButtonAnim.SetTrigger(AnimShowHash);
        mainSettingsButtonAnim.SetTrigger(AnimShowHash);
        mainQuitButtonAnim.SetTrigger(AnimShowHash);
    }

    private IEnumerator MainHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        mainTitleTextAnim.SetTrigger(AnimHideHash);
        mainPlayButtonAnim.SetTrigger(AnimHideHash);
        mainSettingsButtonAnim.SetTrigger(AnimHideHash);
        mainQuitButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => mainTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator MainPlay()
    {
        yield return StartCoroutine(MainHideAnimation());
        SceneManager.LoadScene("Scenes/Select");
    }

    public void OnClickMainPlayButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(MainPlay());
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
        
        InitializeKeySettingTexts();
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
        StartCoroutine(MainShowAnimation());
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

        currentConfiguringKeyIndex = 0;
        // TODO: Add animations
    }

    public void OnClickPlaySettingsBackButton()
    {
        playSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }

    public void OnClickTutorialbutton() {

        GameManager.myManager.filepath = Application.streamingAssetsPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + "Tutorial.txt";
        GameManager.myManager.selectedSongName = "Tutorial";
        GameManager.myManager.selectedComposerName = "Vexatonic Dash";

        SceneManager.LoadScene("Scenes/Tutorial");
    }
    #region Audio Settings

    private void UpdateAudioOffsetValueText()
    {
        offsetValueText.text = GameManager.myManager.globalOffset.ToString("+##0ms;-##0ms;0ms");
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

    private void UpdateMusicVolumeText()
    {
        int percentageVolume = Mathf.RoundToInt(GameManager.myManager.musicVolume * 100);
        musicVolumeText.text = percentageVolume.ToString();
    }
    
    public void OnMusicVolumeSliderChanged(Slider slider)
    {
        GameManager.myManager.musicVolume = slider.value;
        UpdateMusicVolumeText();
    }

    private void UpdateSFXVolumeText()
    {
        int percentageVolume = Mathf.RoundToInt(GameManager.myManager.sfxVolume * 100);
        sfxVolumeText.text = percentageVolume.ToString();
    }

    public void OnSFXVolumeSliderChanged(Slider slider)
    {
        GameManager.myManager.sfxVolume = slider.value;
        UpdateSFXVolumeText();
    }

    #endregion
    
    #region Key Settings

    private void InitializeKeySettingTexts()
    {
        for (int index = 0; index < keyTexts.Length; index++)
        {
            keyTexts[index].text = GameManager.myManager.keyList[index].ToString();
        }
    }
    
    public void OnKeySettingButton(int index)
    {
        keyTexts[index - 1].text = "--";
        currentConfiguringKeyIndex = index;
    }
    
    private void OnGUI()
    {
        Event keyPressEvent = Event.current;
        if (keyPressEvent.type != EventType.KeyDown ||
            keyPressEvent.keyCode == KeyCode.None ||
            currentConfiguringKeyIndex == 0) return;

        GameManager.myManager.keyList[currentConfiguringKeyIndex - 1] = keyPressEvent.keyCode;
        keyTexts[currentConfiguringKeyIndex - 1].text = keyPressEvent.keyCode.ToString();
        currentConfiguringKeyIndex = 0;
    }

    #endregion
}
