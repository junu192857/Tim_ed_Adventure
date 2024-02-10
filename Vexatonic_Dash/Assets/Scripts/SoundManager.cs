using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private AudioClip mainBgm;
    private AudioSource song;



    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        song = GetComponent<AudioSource>();
    }

    //For Main Scene
    private void Start()
    {
        switch (SceneManager.GetActiveScene().name) {
            case "Main":
                song.clip = mainBgm;
                song.Play();
                break;
            case "Select":
                StartCoroutine(PlaySelectedSong(0));
                break;
        }
    }

    // Not Implemented. play song for selected music in select scene.
    public IEnumerator PlaySelectedSong(int songIndex) {
        string fullpath = "file://" + MetaReader.SongMetaList[songIndex].AudioFilePath;
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(fullpath, AudioType.MPEG);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            AudioClip myClip = DownloadHandlerAudioClip.GetContent(uwr);
            song.clip = myClip;
            song.Play();
        }
    }

}
