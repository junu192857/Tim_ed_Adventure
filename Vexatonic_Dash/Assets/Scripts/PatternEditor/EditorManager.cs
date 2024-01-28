using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class EditorManager : MonoBehaviour
{
    private AudioSource song;

    private Camera mainCamera;

    [Header("InitialWindowUI")]
    public Text AudioNameText;
    public Button PlaySongFromInitialButton;
    public Button StartEditorButton;
    public GameObject OuterPanel;
    public GameObject InnerPanel;
    public Text bpmInputText;
    public Text warningText;

    [Header("BeatIndicator")]
    public GameObject measureLinePrefab;
    public GameObject bitLinePrefab;
    public Canvas canvas;

    private List<GameObject> lines;
    private int bit; // 4, 6, 8, 12, 16, 24, 32만 지원함
    private bool indicatorEnabled;

    private void Start()
    {
        song = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        mainCamera.GetComponent<CameraInputAction>().enabled = false;
        PlaySongFromInitialButton.enabled = false;
        StartEditorButton.enabled = false;
    }

    //TODO: 특정 부분부터 bpm 바꾸는 옵션. 일단은 매우귀찮다
    private double bpm;


    // =========================== Initial Window for Editor ===============================

    public void StartEditor() {
        if (double.TryParse(bpmInputText.text, out bpm))
        {
            OuterPanel.SetActive(false);
            InnerPanel.SetActive(false);
            song.Stop();
            lines = new List<GameObject>();
            bit = 4;
            indicatorEnabled = true;
            Camera.main.transform.position = -5 * Vector3.forward;
            mainCamera.GetComponent<CameraInputAction>().enabled = true;
            ReloadMeasureCountLine();
        }
        else warningText.text = "Please Enter valid BPM!";
        
    }
    public void PlaySong() => song.Play();
    public void BrowseSong()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Songs", ".mp3", ".wav"));
        FileBrowser.SetDefaultFilter(".mp3");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        FileBrowser.AddQuickLink("Users", "C:/Users", null);
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);

            StartCoroutine(GetAudioClip(destinationPath));
        }
    }

    IEnumerator GetAudioClip(string audiopath) {
        string fullpath = "file://" + audiopath;
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(fullpath, AudioType.MPEG);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(uwr.error);
        }
        else {
            AudioClip myClip = DownloadHandlerAudioClip.GetContent(uwr);
            song.clip = myClip;
            // TODO: 여기 LastIndexOf의 인수로 '/'와 '\\'를 넣었을 때 다르게 작동하는 문제 해결
            song.clip.name = audiopath.Substring(audiopath.LastIndexOf('\\') + 1);
            AudioNameText.text = "Audio: " + song.clip.name;
            PlaySongFromInitialButton.enabled = true;
            StartEditorButton.enabled = true;
        }
    }

    // =========================== Beat Indicator ===============================

    //CameraInputAction에서 카메라를 조작할 때마다 실행시켜야 한다!!
    public void ReloadMeasureCountLine() {
        if (!indicatorEnabled) return;

        foreach (GameObject _line in lines) {
            Destroy(_line);
        }
        lines.Clear();
        GameObject line;
        //BPM이 바뀌는 곡은 고려하지 않음
        //플레이어의 이동 방향이 반대가 되는 기믹(벽점프 등)을 넣을 예정이지만 에디터에서는 그걸 감안하지 않음
        double lineGap = GameManager.myManager.scrollSpeed * 2 * 240 / (bpm * bit); //n비트 1개의 시간 240/(bpm*bit)지만 노트의 길이 = 시간 * 2이기 때문(변속이 없는 경우) 아 하드코딩 언젠간 업보받을것같음
        int bitCount = (int)Math.Truncate((mainCamera.transform.position.x - mainCamera.orthographicSize * 16 / 9) / lineGap); // 0부터 시작
        double linePosition = bitCount * lineGap;
        while (linePosition <= mainCamera.transform.position.x + mainCamera.orthographicSize * 16 / 9) {
            if (linePosition < 0f) {
                bitCount++;
                linePosition = bitCount * lineGap;
                continue;
            }

            if (bitCount % bit == 0)
            {
                line = Instantiate(measureLinePrefab, mainCamera.WorldToScreenPoint(new Vector3((float)linePosition, mainCamera.transform.position.y, 0)), Quaternion.identity);
                line.transform.SetParent(canvas.transform, true);
                line.GetComponentInChildren<Text>().text = (bitCount / bit + 1).ToString();
                lines.Add(line);
            }
            else { 
                line = Instantiate(bitLinePrefab, mainCamera.WorldToScreenPoint(new Vector3((float)linePosition, mainCamera.transform.position.y, 0)), Quaternion.identity);
                line.transform.SetParent(canvas.transform, true);
                lines.Add(line);
            }
            bitCount++;
            linePosition = bitCount * lineGap;
        }
    }

    public void ToggleLines(Toggle toggle)
    {
        foreach (GameObject line in lines)
        {
            line.SetActive(toggle.isOn);
        }
        indicatorEnabled = toggle.isOn;
        if (toggle.isOn) ReloadMeasureCountLine();
    }
}
