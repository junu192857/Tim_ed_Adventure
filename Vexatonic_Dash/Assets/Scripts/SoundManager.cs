using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{

    [SerializeField] private AudioClip mainBgm;
    [SerializeField] private Sound[] sfx;

    [SerializeField] private AudioSource song;
    [SerializeField] private AudioSource[] sfxPlayer;

    private float MusicVolume => GameManager.myManager.musicVolume;
    private float SFXVolume => GameManager.myManager.sfxVolume;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            DontDestroyOnLoad(gameObject);
            //song = GetComponent<AudioSource>();
        }
        if (GameManager.myManager.sm != null && GameManager.myManager.sm != this) {
            Destroy(gameObject);
        }
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
            default:
                break;
        }
    }

    // Not Implemented. play song for selected music in select scene.
    public IEnumerator PlaySelectedSong(int songIndex) {
        song.Stop();
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
            song.volume = MusicVolume;
            song.Play();
        }
    }


    public void PlaySFX(string p_sfxName)
    {
        for (int i = 0; i < sfx.Length; i++)
        {
            if (p_sfxName == sfx[i].name)
            {
                for (int j = 0; j < sfxPlayer.Length; j++)
                {
                    // SFXPlayer에서 재생 중이지 않은 Audio Source를 발견했다면 
                    if (!sfxPlayer[j].isPlaying)
                    {
                        sfxPlayer[j].PlayOneShot(sfx[i].clip, SFXVolume);
                        return;
                    }
                }
                Debug.Log("모든 오디오 플레이어가 재생중입니다.");
                return;
            }
        }
        Debug.Log(p_sfxName + " 이름의 효과음이 없습니다.");
        return;
    }
}