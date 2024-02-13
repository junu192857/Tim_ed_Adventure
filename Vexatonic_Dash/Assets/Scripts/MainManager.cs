using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    private static readonly int AnimHideHash = Animator.StringToHash("Hide");
    
    [Header("Main")]
    [SerializeField] private GameObject mainParent;
    [SerializeField] private Animator mainTitleTextAnim;
    [SerializeField] private Animator mainPlayButtonAnim;
    [SerializeField] private Animator mainSettingsButtonAnim;
    [SerializeField] private Animator mainQuitButtonAnim;
    
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
        StartCoroutine(MainPlay());
    }
    
    public void OnClickMainSettingsButton()
    {
        mainParent.SetActive(false);
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
        settingsParent.SetActive(false);
        // TODO: Add animations
    }
    
    public void OnClickSettingsAudioButton()
    {
        audioSettingsParent.SetActive(true);
        settingsParent.SetActive(false);
        
        UpdateAudioOffsetValueText();
        // TODO: Add animations
    }

    public void OnClickSettingsInputButton()
    {
        inputSettingsParent.SetActive(true);
        settingsParent.SetActive(false);
        // TODO: Add animations
    }

    public void OnClickSettingsPlayButton()
    {
        playSettingsParent.SetActive(true);
        settingsParent.SetActive(false);
        // TODO: Add animations
    }

    public void OnClickSettingsBackButton()
    {
        settingsParent.SetActive(false);
        mainParent.SetActive(true);
        StartCoroutine(MainShowAnimation());
    }

    public void OnClickVideoSettingsBackButton()
    {
        videoSettingsParent.SetActive(false);
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickAudioSettingsBackButton()
    {
        audioSettingsParent.SetActive(false);
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickInputSettingsBackButton()
    {
        inputSettingsParent.SetActive(false);
        settingsParent.SetActive(true);
        // TODO: Add animations
    }
    
    public void OnClickPlaySettingsBackButton()
    {
        playSettingsParent.SetActive(false);
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
