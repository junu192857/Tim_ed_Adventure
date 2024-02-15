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
    [SerializeField] private Animator mainTutorialButtonAnim;
    [SerializeField] private Animator mainCreditButtonAnim;

    [Header("Settings")] [SerializeField] private GameObject settingsParent;

    [Space(5)]
    [SerializeField] private Animator settingsTitleTextAnim;
    [SerializeField] private Animator videoButtonAnim;
    [SerializeField] private Animator audioButtonAnim;
    [SerializeField] private Animator inputButtonAnim;
    [SerializeField] private Animator playButtonAnim;
    [SerializeField] private Animator settingsBackButtonAnim;

    [Space(10)] [SerializeField] private GameObject videoSettingsParent;
    [SerializeField] private Text speedValueText;

    [Space(5)]
    [SerializeField] private Animator videoTitleTextAnim;
    [SerializeField] private Animator noteSpeedSettingAnim;
    [SerializeField] private Animator videoBackButtonAnim;

    [Space(10)] [SerializeField] private GameObject audioSettingsParent;
    [SerializeField] private Text offsetValueText;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Text sfxVolumeText;

    [Space(5)]
    [SerializeField] private Animator audioTitleTextAnim;
    [SerializeField] private Animator offsetSettingAnim;
    [SerializeField] private Animator musicVolumeSettingAnim;
    [SerializeField] private Animator sfxVolumeSettingAnim;
    [SerializeField] private Animator audioBackButtonAnim;

    [Space(10)] [SerializeField] private GameObject inputSettingsParent;
    [SerializeField] private Text[] keyTexts;
    private int currentConfiguringKeyIndex;
    
    [Space(5)]
    [SerializeField] private Animator inputTitleTextAnim;
    [SerializeField] private Animator normalButton1Anim;
    [SerializeField] private Animator normalButton2Anim;
    [SerializeField] private Animator dashButton1Anim;
    [SerializeField] private Animator dashButton2Anim;
    [SerializeField] private Animator inputBackButtonAnim;

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

    #region Main Animation
    
    private IEnumerator MainShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        mainTitleTextAnim.SetTrigger(AnimShowHash);
        mainPlayButtonAnim.SetTrigger(AnimShowHash);
        mainSettingsButtonAnim.SetTrigger(AnimShowHash);
        mainQuitButtonAnim.SetTrigger(AnimShowHash);
        mainTutorialButtonAnim.SetTrigger(AnimShowHash);
        mainCreditButtonAnim.SetTrigger(AnimShowHash);
    }

    private IEnumerator MainHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        mainTitleTextAnim.SetTrigger(AnimHideHash);
        mainPlayButtonAnim.SetTrigger(AnimHideHash);
        mainSettingsButtonAnim.SetTrigger(AnimHideHash);
        mainQuitButtonAnim.SetTrigger(AnimHideHash);
        mainTutorialButtonAnim.SetTrigger(AnimHideHash);
        mainCreditButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => mainTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator MainPlay()
    {
        yield return StartCoroutine(MainHideAnimation());
        SceneManager.LoadScene("Scenes/Select");
    }
    
    #endregion
    
    #region Settings Animation

    private IEnumerator SettingsShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        settingsTitleTextAnim.SetTrigger(AnimShowHash);
        videoButtonAnim.SetTrigger(AnimShowHash);
        audioButtonAnim.SetTrigger(AnimShowHash);
        inputButtonAnim.SetTrigger(AnimShowHash);
        playButtonAnim.SetTrigger(AnimShowHash);
        settingsBackButtonAnim.SetTrigger(AnimShowHash);
    }
    
    private IEnumerator SettingsHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        settingsTitleTextAnim.SetTrigger(AnimHideHash);
        videoButtonAnim.SetTrigger(AnimHideHash);
        audioButtonAnim.SetTrigger(AnimHideHash);
        inputButtonAnim.SetTrigger(AnimHideHash);
        playButtonAnim.SetTrigger(AnimHideHash);
        settingsBackButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => settingsTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator EnterSettings()
    {
        settingsParent.SetActive(true);
        
        yield return StartCoroutine(MainHideAnimation());
        yield return StartCoroutine(SettingsShowAnimation());

        mainParent.SetActive(false);
    }

    private IEnumerator ExitSettings()
    {
        mainParent.SetActive(true);

        yield return StartCoroutine(SettingsHideAnimation());
        yield return StartCoroutine(MainShowAnimation());

        settingsParent.SetActive(false);
    }
    
    #endregion
    
    #region Video Setting Animation

    private IEnumerator VideoSettingShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        videoTitleTextAnim.SetTrigger(AnimShowHash);
        noteSpeedSettingAnim.SetTrigger(AnimShowHash);
        videoBackButtonAnim.SetTrigger(AnimShowHash);
    }

    private IEnumerator VideoSettingHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        videoTitleTextAnim.SetTrigger(AnimHideHash);
        noteSpeedSettingAnim.SetTrigger(AnimHideHash);
        videoBackButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => videoTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator EnterVideoSetting()
    {
        videoSettingsParent.SetActive(true);

        yield return StartCoroutine(SettingsHideAnimation());
        yield return StartCoroutine(VideoSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitVideoSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(VideoSettingHideAnimation());
        yield return StartCoroutine(SettingsShowAnimation());

        videoSettingsParent.SetActive(false);

    }
    
    #endregion
    
    #region Audio Setting Animation

    private IEnumerator AudioSettingShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        audioTitleTextAnim.SetTrigger(AnimShowHash);
        offsetSettingAnim.SetTrigger(AnimShowHash);
        musicVolumeSettingAnim.SetTrigger(AnimShowHash);
        sfxVolumeSettingAnim.SetTrigger(AnimShowHash);
        audioBackButtonAnim.SetTrigger(AnimShowHash);
    }
    
    private IEnumerator AudioSettingHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        audioTitleTextAnim.SetTrigger(AnimHideHash);
        offsetSettingAnim.SetTrigger(AnimHideHash);
        musicVolumeSettingAnim.SetTrigger(AnimHideHash);
        sfxVolumeSettingAnim.SetTrigger(AnimHideHash);
        audioBackButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => audioTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator EnterAudioSetting()
    {
        audioSettingsParent.SetActive(true);

        yield return StartCoroutine(SettingsHideAnimation());
        yield return StartCoroutine(AudioSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitAudioSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(AudioSettingHideAnimation());
        yield return StartCoroutine(SettingsShowAnimation());

        audioSettingsParent.SetActive(false);
    }
    
    #endregion

    #region Input Setting Animation
    
    private IEnumerator InputSettingShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        inputTitleTextAnim.SetTrigger(AnimShowHash);
        normalButton1Anim.SetTrigger(AnimShowHash);
        normalButton2Anim.SetTrigger(AnimShowHash);
        dashButton1Anim.SetTrigger(AnimShowHash);
        dashButton2Anim.SetTrigger(AnimShowHash);
        inputBackButtonAnim.SetTrigger(AnimShowHash);
    }
    
    private IEnumerator InputSettingHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        inputTitleTextAnim.SetTrigger(AnimHideHash);
        normalButton1Anim.SetTrigger(AnimHideHash);
        normalButton2Anim.SetTrigger(AnimHideHash);
        dashButton1Anim.SetTrigger(AnimHideHash);
        dashButton2Anim.SetTrigger(AnimHideHash);
        inputBackButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => inputTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator EnterInputSetting()
    {
        inputSettingsParent.SetActive(true);

        yield return StartCoroutine(SettingsHideAnimation());
        yield return StartCoroutine(InputSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitInputSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(InputSettingHideAnimation());
        yield return StartCoroutine(SettingsShowAnimation());

        inputSettingsParent.SetActive(false);
    }
    
    #endregion
    
    public void OnClickMainPlayButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(MainPlay());
    }

    public void OnClickMainSettingsButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterSettings());
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
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterVideoSetting());
        
        UpdateNoteSpeedValueText();
    }

    public void OnClickSettingsAudioButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterAudioSetting());
        
        UpdateAudioOffsetValueText();
    }

    public void OnClickSettingsInputButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterInputSetting());
        
        InitializeKeySettingTexts();
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
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(ExitSettings());
    }

    public void OnClickVideoSettingsBackButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(ExitVideoSetting());
    }

    public void OnClickAudioSettingsBackButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(ExitAudioSetting());
    }

    public void OnClickInputSettingsBackButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(ExitInputSetting());

        currentConfiguringKeyIndex = 0;
    }

    public void OnClickPlaySettingsBackButton()
    {
        playSettingsParent.SetActive(false);
        GameManager.myManager.sm.PlaySFX("Button");
        settingsParent.SetActive(true);
        // TODO: Add animations
    }

    public void OnClickTutorialbutton() {

        GameManager.myManager.sm.PlaySFX("Button");
        GameManager.myManager.filepath = Application.streamingAssetsPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + "Tutorial.txt";
        GameManager.myManager.selectedSongName = "Tutorial";
        GameManager.myManager.selectedComposerName = "Vexatonic Dash";

        GameManager.myManager.isTutorial = true;
        SceneManager.LoadScene("Scenes/LevelTest");
    }

    #region Video Settings

    private void UpdateNoteSpeedValueText()
    {
        speedValueText.text = GameManager.myManager.noteSpeed.ToString("#0.0");
    }

    public void OnClickNoteSpeedUpButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed + 0.1f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValueText();
    }

    public void OnClickNoteSpeedDownButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed - 0.1f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValueText();
    }
    
    #endregion

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
