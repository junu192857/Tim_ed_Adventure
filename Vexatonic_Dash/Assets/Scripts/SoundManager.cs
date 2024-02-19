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
    [SerializeField] private AudioClip _note, _unote;
    [SerializeField] private Sound[] sfx;

    [SerializeField] private AudioSource song, note, unote;
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

        note.clip = _note;
        unote.clip = _unote;

        note.volume = SFXVolume;
        unote.volume = SFXVolume;
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
    }

    public void PlaySelectedSong(AudioClip clip)
    {
        song.Stop();
        song.clip = clip;
        song.volume = MusicVolume;
        song.Play();
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

    public void PlayUTouch()
    {
        if (unote.isPlaying) return;
        double curdspTime = AudioSettings.dspTime;
        unote.PlayScheduled(curdspTime);
    }

    public void PlayTouch()
    {
        Debug.Log("touch!");
        if (unote.isPlaying) note.timeSamples = Math.Min(unote.timeSamples + 300, 1400);
        else note.timeSamples = 900;
        note.Play();
    }

    public void SetBgmVolume() => song.volume = MusicVolume;
}