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

    private void EnterMain()
    {
        
    }

    private void ExitMain()
    {
        
    }

    private void EnterSettings()
    {
        
    }

    private void ExitSettings()
    {
        
    }

    private void EnterPlay()
    {
        
    }

    private void ExitPlay()
    {
        
    }
}
