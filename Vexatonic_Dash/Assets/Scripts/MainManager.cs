using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum MainState
{
    Main          = 0,
    Settings      = 1,
    VideoSettings = 2,
    AudioSettings = 3,
    InputSettings = 4,
    PlaySettings  = 5,
    Credits       = 6
}

public class MainManager : MonoBehaviour
{
    private static readonly int[] CursorMaxIndex = {5, 3, 1, 3, 4, 1, 1};
    
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");

    [Header("Loading")]
    [SerializeField] private GameObject loadingParent;
    
    [Header("Main")]
    [SerializeField] private GameObject mainParent;
    [SerializeField] private Button mainPlayButton;
    [SerializeField] private Button mainSettingsButton;
    [SerializeField] private Button mainQuitButton;
    [SerializeField] private Button mainCreditButton;
    [SerializeField] private Button mainTutorialButton;
    
    [Space(5)]
    [SerializeField] private Animator mainTitleTextAnim;
    [SerializeField] private Animator mainPlayButtonAnim;
    [SerializeField] private Animator mainSettingsButtonAnim;
    [SerializeField] private Animator mainQuitButtonAnim;
    [SerializeField] private Animator mainTutorialButtonAnim;
    [SerializeField] private Animator mainCreditButtonAnim;

    [Header("Settings")]
    [SerializeField] private GameObject settingsParent;
    [SerializeField] private Button videoButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button inputButton;
    [SerializeField] private Button playButton;

    [Space(5)]
    [SerializeField] private Animator settingsTitleTextAnim;
    [SerializeField] private Animator videoButtonAnim;
    [SerializeField] private Animator audioButtonAnim;
    [SerializeField] private Animator inputButtonAnim;
    [SerializeField] private Animator playButtonAnim;
    [SerializeField] private Animator settingsBackButtonAnim;

    [Header("Video Settings")]
    [SerializeField] private GameObject videoSettingsParent;
    [SerializeField] private Button noteSpeedSettingButton;
    [SerializeField] private Text speedValueText;

    [Space(5)]
    [SerializeField] private Animator videoTitleTextAnim;
    [SerializeField] private Animator noteSpeedSettingAnim;
    [SerializeField] private Animator videoBackButtonAnim;

    [Header("Audio Settings")]
    [SerializeField] private GameObject audioSettingsParent;
    [SerializeField] private Button offsetSettingButton;
    [SerializeField] private Button musicVolumeSettingButton;
    [SerializeField] private Button sfxVolumeSettingButton;
    [SerializeField] private Text offsetValueText;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Text sfxVolumeText;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Space(5)]
    [SerializeField] private Animator audioTitleTextAnim;
    [SerializeField] private Animator offsetSettingAnim;
    [SerializeField] private Animator musicVolumeSettingAnim;
    [SerializeField] private Animator sfxVolumeSettingAnim;
    [SerializeField] private Animator audioBackButtonAnim;

    [Header("Input Settings")]
    [SerializeField] private GameObject inputSettingsParent;
    [SerializeField] private Button normalButton1;
    [SerializeField] private Button normalButton2;
    [SerializeField] private Button dashButton1;
    [SerializeField] private Button dashButton2;
    [SerializeField] private Text[] keyTexts;
    private int currentConfiguringKeyIndex;
    
    [Space(5)]
    [SerializeField] private Animator inputTitleTextAnim;
    [SerializeField] private Animator normalButton1Anim;
    [SerializeField] private Animator normalButton2Anim;
    [SerializeField] private Animator dashButton1Anim;
    [SerializeField] private Animator dashButton2Anim;
    [SerializeField] private Animator inputBackButtonAnim;

    [Header("Play Settings")]
    [SerializeField] private GameObject playSettingsParent;

    [Header("Credits")]
    [SerializeField] private GameObject creditsParent;
    [SerializeField] private RectTransform creditsContentRect;
    [SerializeField] private ScrollRect creditsScrollRect;
    
    [Space(5)]
    [SerializeField] private Animator creditsTitleTextAnim;
    [SerializeField] private Animator creditsScrollViewAnim;
    [SerializeField] private Animator creditsBackButtonAnim;

    [Header("Debug")]
    [SerializeField] private Text currentCursorIndexText;
    [SerializeField] private Text scrollRectPositionText;

    public MainState currentState;
    public int currentCursorIndex;

    private void Start()
    {
        GameManager.myManager.LoadSettings();
        
        GameManager.myManager.sm.StartMainBgm();

        loadingParent.SetActive(true);
        mainParent.SetActive(false);
        settingsParent.SetActive(false);
        videoSettingsParent.SetActive(false);
        audioSettingsParent.SetActive(false);
        inputSettingsParent.SetActive(false);
        playSettingsParent.SetActive(false);
        creditsParent.SetActive(false);

        // Keyboard control values
        currentState = MainState.Main;
        currentCursorIndex = 0;

        // Key Initialization
        currentConfiguringKeyIndex = 0;

        StartCoroutine(MainShowAnimation());
    }

    private void Update()
    {
        //currentCursorIndexText.text = $"Current Cursor Index: {currentCursorIndex}";
        //scrollRectPositionText.text = $"Scroll Rect Position: {creditsScrollRect.verticalNormalizedPosition}";
    }

    private void UpdateCursor()
    {
        switch (currentState)
        {
            case MainState.Main:
                switch (currentCursorIndex)
                {
                    case 0:
                        mainPlayButton.Select();
                        break;
                    case 1:
                        mainSettingsButton.Select();
                        break;
                    case 2:
                        mainQuitButton.Select();
                        break;
                    case 3:
                        mainCreditButton.Select();
                        break;
                    case 4:
                        mainTutorialButton.Select();
                        break;
                }

                break;
            
            case MainState.Settings:
                switch (currentCursorIndex)
                {
                    case 0:
                        videoButton.Select();
                        break;
                    case 1:
                        audioButton.Select();
                        break;
                    case 2:
                        inputButton.Select();
                        break;
                    case 3:
                        playButton.Select();
                        break;
                }

                break;
            
            case MainState.VideoSettings:
                switch (currentCursorIndex)
                {
                    case 0:
                        noteSpeedSettingButton.Select();
                        break;
                }

                break;
            
            case MainState.AudioSettings:
                switch (currentCursorIndex)
                {
                    case 0:
                        offsetSettingButton.Select();
                        break;
                    case 1:
                        musicVolumeSettingButton.Select();
                        break;
                    case 2:
                        sfxVolumeSettingButton.Select();
                        break;
                }

                break;
            
            case MainState.InputSettings:
                switch (currentCursorIndex)
                {
                    case 0:
                        normalButton1.Select();
                        break;
                    case 1:
                        normalButton2.Select();
                        break;
                    case 2:
                        dashButton1.Select();
                        break;
                    case 3:
                        dashButton2.Select();
                        break;
                }

                break;
            
            case MainState.PlaySettings:
            case MainState.Credits:
                break;
            
            default:
                throw new ArgumentException();
        }
    }

    #region Keyboard Control

    public void OnMoveCursor(InputValue inputValue)
    {
        var input = (int)Mathf.Sign(inputValue.Get<float>());

        if (input == 0) return;

        if (currentState == MainState.Credits)
        {
            creditsContentRect.anchoredPosition += new Vector2(0, 180 * input);
            return;
        }
        
        currentCursorIndex = (currentCursorIndex + input);
        
        if (currentCursorIndex < 0) currentCursorIndex = CursorMaxIndex[(int)currentState] - 1;
        else if (currentCursorIndex >= CursorMaxIndex[(int)currentState]) currentCursorIndex = 0;
        
        UpdateCursor();
    }

    public void OnAdjustValue(InputValue inputValue)
    {
        var input = (int)Mathf.Sign(inputValue.Get<float>());

        if (input == 0) return;

        switch (currentState, currentCursorIndex)
        {
            case (MainState.VideoSettings, 0):
                if (input > 0) OnClickNoteSpeedUpButton();
                else OnClickNoteSpeedDownButton();
                break;
            case (MainState.AudioSettings, 0):
                if (input > 0) OnClickAudioOffsetUpButton();
                else OnClickAudioOffsetDownButton();
                break;
            case (MainState.AudioSettings, 1):
                musicVolumeSlider.value = Mathf.Clamp01(musicVolumeSlider.value + 0.1f * input);
                break;
            case (MainState.AudioSettings, 2):
                sfxVolumeSlider.value = Mathf.Clamp01(sfxVolumeSlider.value + 0.1f * input);
                break;
        }
    }

    public void OnSelectCurrent()
    {
        switch (currentState)
        {
            case MainState.Main:
                switch (currentCursorIndex)
                {
                    case 0:
                        OnClickMainPlayButton();
                        break;
                    case 1:
                        OnClickMainSettingsButton();
                        break;
                    case 2:
                        OnClickMainQuitButton();
                        break;
                    case 3:
                        OnClickMainCreditsButton();
                        break;
                    case 4:
                        OnClickTutorialButton();
                        break;
                }
                break;
            
            case MainState.Settings:
                switch (currentCursorIndex)
                {
                    case 0:
                        OnClickSettingsVideoButton();
                        break;
                    case 1:
                        OnClickSettingsAudioButton();
                        break;
                    case 2:
                        OnClickSettingsInputButton();
                        break;
                    case 3:
                        OnClickSettingsPlayButton();
                        break;
                }
                break;

            case MainState.VideoSettings:
                break;
            
            case MainState.AudioSettings:
                break;
            
            case MainState.InputSettings:
                OnKeySettingButton(currentCursorIndex + 1);
                break;
            
            case MainState.PlaySettings:
                break;
            
            default:
                throw new ArgumentException();
        }
    }

    public void OnEscape()
    {
        switch (currentState)
        {
            case MainState.Main:
                break;
            
            case MainState.Settings:
                OnClickSettingsBackButton();
                break;
            
            case MainState.VideoSettings:
                OnClickVideoSettingsBackButton();
                break;
            
            case MainState.AudioSettings:
                OnClickAudioSettingsBackButton();
                break;
            
            case MainState.InputSettings:
                OnClickInputSettingsBackButton();
                break;
            
            case MainState.PlaySettings:
                OnClickPlaySettingsBackButton();
                break;
            
            case MainState.Credits:
                OnClickCreditsBackButton();
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    #endregion

    #region Main Animation
    
    private IEnumerator MainShowAnimation()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => GameManager.myManager.isAudioClipLoaded);
        
        loadingParent.SetActive(false);
        mainParent.SetActive(true);

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
        
        currentState = MainState.Settings;
        currentCursorIndex = 0;
        
        yield return StartCoroutine(SettingsShowAnimation());

        mainParent.SetActive(false);
    }

    private IEnumerator ExitSettings()
    {
        mainParent.SetActive(true);

        yield return StartCoroutine(SettingsHideAnimation());
        
        currentState = MainState.Main;
        currentCursorIndex = 1;
        
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
        
        currentState = MainState.VideoSettings;
        currentCursorIndex = 0;
        
        yield return StartCoroutine(VideoSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitVideoSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(VideoSettingHideAnimation());
        
        currentState = MainState.Settings;
        currentCursorIndex = 0;
        
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
        
        currentState = MainState.AudioSettings;
        currentCursorIndex = 0;
        
        yield return StartCoroutine(AudioSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitAudioSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(AudioSettingHideAnimation());
        
        currentState = MainState.Settings;
        currentCursorIndex = 1;
        
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
        
        currentState = MainState.InputSettings;
        currentCursorIndex = 0;
        
        yield return StartCoroutine(InputSettingShowAnimation());
        
        settingsParent.SetActive(false);
    }

    private IEnumerator ExitInputSetting()
    {
        settingsParent.SetActive(true);

        yield return StartCoroutine(InputSettingHideAnimation());
        
        currentState = MainState.Settings;
        currentCursorIndex = 2;
        
        yield return StartCoroutine(SettingsShowAnimation());

        inputSettingsParent.SetActive(false);
    }
    
    #endregion
    
    #region Credits Animation

    private IEnumerator CreditsShowAnimation()
    {
        yield return new WaitForEndOfFrame();

        creditsTitleTextAnim.SetTrigger(AnimShowHash);
        creditsScrollViewAnim.SetTrigger(AnimShowHash);
        creditsBackButtonAnim.SetTrigger(AnimShowHash);
    }
    
    private IEnumerator CreditsHideAnimation()
    {
        yield return new WaitForEndOfFrame();

        creditsTitleTextAnim.SetTrigger(AnimHideHash);
        creditsScrollViewAnim.SetTrigger(AnimHideHash);
        creditsBackButtonAnim.SetTrigger(AnimHideHash);

        yield return new WaitUntil(() => creditsTitleTextAnim.GetCurrentAnimatorStateInfo(0).IsName("Hidden"));
    }

    private IEnumerator EnterCredits()
    {
        creditsParent.SetActive(true);
        
        yield return StartCoroutine(MainHideAnimation());
        
        currentState = MainState.Credits;
        currentCursorIndex = 0;
        creditsScrollRect.verticalNormalizedPosition = 1;
        
        yield return StartCoroutine(CreditsShowAnimation());

        mainParent.SetActive(false);
    }

    private IEnumerator ExitCredits()
    {
        mainParent.SetActive(true);

        yield return StartCoroutine(CreditsHideAnimation());
        
        currentState = MainState.Main;
        currentCursorIndex = 3;
        
        yield return StartCoroutine(MainShowAnimation());

        creditsParent.SetActive(false);
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
        
        UpdateNoteSpeedValue();
    }

    public void OnClickSettingsAudioButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterAudioSetting());
        
        UpdateAudioSettingsValues();
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

    public void OnClickMainCreditsButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(EnterCredits());
    }
    
    public void OnClickCreditsBackButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        StartCoroutine(ExitCredits());
    }

    public void OnClickTutorialButton()
    {
        GameManager.myManager.sm.PlaySFX("Button");
        GameManager.myManager.filepath = Application.streamingAssetsPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar + "Tutorial.txt";
        GameManager.myManager.selectedSongName = "Tutorial";
        GameManager.myManager.selectedComposerName = "Vexatonic Dash";
        GameManager.myManager.selectedDifficulty = Difficulty.Easy;
        GameManager.myManager.selectedLevel = 1;

        GameManager.myManager.isTutorial = true;
        SceneManager.LoadScene("Scenes/LevelTest");
    }

    #region Video Settings

    private void UpdateNoteSpeedValue()
    {
        speedValueText.text = GameManager.myManager.noteSpeed.ToString("#0.0");
        GameManager.myManager.SaveSettings();
    }

    public void OnClickNoteSpeedUpButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed + 0.1f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValue();
    }

    public void OnClickNoteSpeedDownButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed - 0.1f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValue();
    }
    
    public void OnClickNoteSpeedUpMajorButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed + 1.0f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValue();
    }
    
    public void OnClickNoteSpeedDownMajorButton()
    {
        float newNoteSpeed = GameManager.myManager.noteSpeed - 1.0f;
        GameManager.myManager.noteSpeed = Mathf.Clamp(newNoteSpeed, GameManager.MinNoteSpeed, GameManager.MaxNoteSpeed);
        
        UpdateNoteSpeedValue();
    }
    
    #endregion

    #region Audio Settings

    private void UpdateAudioSettingsValues()
    {
        offsetValueText.text = GameManager.myManager.globalOffset.ToString("+##0ms;-##0ms;0ms");
        musicVolumeSlider.value = GameManager.myManager.musicVolume;
        sfxVolumeSlider.value = GameManager.myManager.sfxVolume;
        GameManager.myManager.SaveSettings();
    }

    public void OnClickAudioOffsetUpButton()
    {
        if (GameManager.myManager.globalOffset >= 999) return;
        GameManager.myManager.globalOffset += 1;
        UpdateAudioSettingsValues();
    }

    public void OnClickAudioOffsetDownButton()
    {
        if (GameManager.myManager.globalOffset <= -999) return;
        GameManager.myManager.globalOffset -= 1;
        UpdateAudioSettingsValues();
    }
    
    public void OnClickAudioOffsetUpMajorButton()
    {
        if (GameManager.myManager.globalOffset >= 990) return;
        GameManager.myManager.globalOffset += 10;
        UpdateAudioSettingsValues();
    }

    public void OnClickAudioOffsetDownMajorButton()
    {
        if (GameManager.myManager.globalOffset <= -990) return;
        GameManager.myManager.globalOffset -= 10;
        UpdateAudioSettingsValues();
    }

    private void UpdateMusicVolumeText()
    {
        int percentageVolume = Mathf.RoundToInt(GameManager.myManager.musicVolume * 100);
        musicVolumeText.text = percentageVolume.ToString();
        GameManager.myManager.SaveSettings();
    }
    
    public void OnMusicVolumeSliderChanged(Slider slider)
    {
        GameManager.myManager.musicVolume = slider.value;
        GameManager.myManager.sm.SetBgmVolume();
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
