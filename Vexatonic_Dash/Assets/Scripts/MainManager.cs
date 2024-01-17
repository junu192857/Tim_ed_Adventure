using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainParent;
    
    [Header("Settings")]
    [SerializeField] private GameObject settingsParent;

    [Header("Play")]
    [SerializeField] private GameObject playParent;

    private void Start()
    {
        mainParent.SetActive(true);
    }

    public void OnClickMainPlayButton()
    {
        mainParent.SetActive(false);
        playParent.SetActive(true);
        // TODO: Add animations
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
        
    }
    
    public void OnClickSettingsAudioButton()
    {
        
    }

    public void OnClickSettingsInputButton()
    {
        
    }

    public void OnClickSettingsPlayButton()
    {
        
    }

    public void OnClickSettingsBackButton()
    {
        settingsParent.SetActive(false);
        mainParent.SetActive(true);
        // TODO: Add animations
    }

    public void OnClickPlayBackButton()
    {
        playParent.SetActive(false);
        mainParent.SetActive(true);
        // TODO: Add animations
    }
}
