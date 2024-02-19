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
    private void Start() => StartMainBgm();

    public void StartMainBgm() {
        song.Stop();
        if (mainBgm != null)
        {
            song.clip = mainBgm;
            song.loop = true;
            song.Play();
        }

        for (int i = 0; i < sfx.Length; i++)
        {
            sfxPlayer[i].clip = sfx[i].clip;
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
                sfxPlayer[i].time = 0f;
                Debug.Log("sound " + i + "start"+p_sfxName);
                double curDspTime = AudioSettings.dspTime;
                sfxPlayer[i].volume = SFXVolume;
                sfxPlayer[i].PlayScheduled(curDspTime);
                return;
            }
        }
        Debug.Log(p_sfxName + " 이름의 효과음이 없습니다.");
        return;
    }

    public void PlayNote()
    {//5가 notouch, 6이 touch
        sfxPlayer[6].Stop();
        sfxPlayer[5].volume = SFXVolume;
        sfxPlayer[6].volume = SFXVolume;
        if (!sfxPlayer[5].isPlaying)
        {
            sfxPlayer[6].Play();
            return;
        }

        sfxPlayer[6].timeSamples = Math.Min(sfxPlayer[5].timeSamples + 300, 2000);
        sfxPlayer[6].Play();
    }
}