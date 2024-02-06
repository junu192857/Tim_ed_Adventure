using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public enum EditorState { 
    EditorInitial,
    EditorMain,
    OnSetting
}

public class EditorManager : MonoBehaviour
{
    private AudioSource song;

    private Camera mainCamera;

    public EditorState editorState;

    [Header("InitialWindowUI")]
    public Text AudioNameText;
    public Button PlaySongFromInitialButton;
    public Button StartEditorButton;
    public GameObject TotalPanel;
    public GameObject InnerPanel;
    public Text bpmInputText;
    public Text warningText;

    [Header("Indicator")]
    public GameObject measureLinePrefab;
    public GameObject bitLinePrefab;
    public GameObject songLinePrefab;
    public InputField bitInputField;
    private IEnumerator songLineMoveCoroutine;
    private GameObject songLine;
    public Canvas canvas;
    private List<GameObject> lines;
    private int bit;
    private bool indicatorEnabled;
    private float musicOffset;

    [Header("EditorMain")]
    private Vector3 noteStartPosition; // 다음에 배치할 노트의 시작점
    private Vector3 noteEndPosition; // 다음에 배치할 노트의 끝점
    private GameObject notePreview; // 노트가 어떻게 생길지 미리 알려줌
    public List<GameObject> notePrefabs;
    public GameObject jumpEndIndicator;
    private GameObject selectedNote;
    private Color c;
    private SpriteRenderer noteSprite;
    private float platformAngle;
    private float dashPlatformAngle;
    private float dashCoeff;
    public enum NoteWriteSetting
    {
        MouseDiscrete,
        MouseContinuous,
        WriteLength
    }
    public NoteWriteSetting noteWriteSetting;
    private Vector3 mousePosition;
    [SerializeField] private GameObject normalNoteSettingPanel;
    [SerializeField] private InputField normalAngleInputField;
    [SerializeField] private GameObject dashNoteSettingPanel;
    [SerializeField] private InputField dashAngleInputField;
    [SerializeField] private InputField dashCoeffInputField;
    [SerializeField] private GameObject jumpNoteSettingPanel;
    [SerializeField] private InputField noteLengthInputField;
    [SerializeField] private GameObject settingBackgroundPanel;

    [Header("LevelWriter")]
    private List<NoteInfoPair> noteStorage;
    [SerializeField] private GameObject mapSavePanel;
    [SerializeField] private InputField mapNameInputField;
    private FileStream writer;
    private StreamWriter sw;
    private string filepath;


    private void Start()
    {
        song = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        mainCamera.GetComponent<CameraInputAction>().enabled = false;
        PlaySongFromInitialButton.enabled = false;
        StartEditorButton.enabled = false;
        editorState = EditorState.EditorInitial;
        //placedNotes = new List<GameObject>();
        //noteSpawnInfos = new List<NoteSpawnInfo>();
        noteStorage = new List<NoteInfoPair>();
    }

    //TODO: 특정 부분부터 bpm 바꾸는 옵션. 일단은 매우귀찮다
    private float bpm;


    // =========================== Initial Window for Editor ===============================

    public void StartEditor() {
        if (float.TryParse(bpmInputText.text, out bpm))
        {
            TotalPanel.SetActive(false);
            InnerPanel.SetActive(false);
            song.Stop();
            lines = new List<GameObject>();
            bit = 4;
            bitInputField.text = "4";
            indicatorEnabled = true;
            noteStartPosition = Vector3.zero;
            noteEndPosition = Vector3.zero;
            dashCoeff = 1.5f;
            editorState = EditorState.EditorMain;
            noteWriteSetting = NoteWriteSetting.MouseDiscrete;
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
        float lineGap = GameManager.myManager.CalculateInputWidthFromTime(240 / (bpm * bit)); //n비트 1개의 시간 240/(bpm*bit)지만 노트의 길이 = 시간 * 2이기 때문(변속이 없는 경우) 아 하드코딩 언젠간 업보받을것같음
        int bitCount = (int)((mainCamera.transform.position.x - mainCamera.orthographicSize * 16 / 9) / lineGap); // 0부터 시작
        float linePosition = bitCount * lineGap;
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

    public void ChangeBit() { if (int.TryParse(bitInputField.text, out bit)) ReloadMeasureCountLine(); }

    public void ToggleLines(Toggle toggle)
    {
        foreach (GameObject line in lines)
        {
            line.SetActive(toggle.isOn);
        }
        indicatorEnabled = toggle.isOn;
        if (toggle.isOn) ReloadMeasureCountLine();
    }

    public void SongToggleIngame(Toggle toggle)
    {
        if (songLineMoveCoroutine != null) StopCoroutine(songLineMoveCoroutine);
        if (toggle.isOn)
        {
            if (songLine != null) Destroy(songLine);
            float startPositionX = mainCamera.transform.position.x - mainCamera.orthographicSize * 16 / 9;
            songLine = Instantiate(songLinePrefab, mainCamera.WorldToScreenPoint(new Vector3(startPositionX, mainCamera.transform.position.y, 0f)), Quaternion.identity);
            songLine.transform.SetParent(canvas.transform, true);
            songLineMoveCoroutine = MoveSongLineCoroutine(startPositionX, songLine);
        }
        else {
            song.Stop();
            songLineMoveCoroutine = FixSongLineCoroutine(songLine);
        }
        StartCoroutine(songLineMoveCoroutine);
    }

    private IEnumerator MoveSongLineCoroutine(float startX, GameObject line)
    {
        if (startX < 0) startX = 0;
        float musicTime = (float)GameManager.myManager.CalculateTimeFromInputWidth(startX);
        song.time = musicTime;
        song.Play();
        while (true) {
            line.GetComponent<RectTransform>().position = mainCamera.WorldToScreenPoint(new Vector3(startX, mainCamera.transform.position.y, 0f));
            startX += GameManager.myManager.CalculateInputWidthFromTime(Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator FixSongLineCoroutine(GameObject line) {
        Vector3 positionDelta = line.GetComponent<RectTransform>().position - mainCamera.WorldToScreenPoint(new Vector3(0, mainCamera.transform.position.y, 0));
        while (true) {
            line.GetComponent<RectTransform>().position = mainCamera.WorldToScreenPoint(new Vector3(0, mainCamera.transform.position.y, 0)) + positionDelta;
            yield return null;
        }
    }

    // =========================== Editor Main ===============================


    private void Update()
    {
        if (editorState != EditorState.EditorMain || selectedNote == null) return;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //노트 x좌표 변경하기. NoteWriteSetting이 WriteLength라면 별개의 함수를 통해 변경
        if (noteWriteSetting != NoteWriteSetting.WriteLength)
        {
            if (noteWriteSetting == NoteWriteSetting.MouseDiscrete)
            {
                double lineGap = GameManager.myManager.CalculateInputWidthFromTime(240 / (bpm * bit));
                int bitCount = (int)(mousePosition.x / lineGap);
                noteEndPosition = new Vector3((float)(lineGap * bitCount), noteStartPosition.y, 0);
            }
            else noteEndPosition = new Vector3(mousePosition.x, noteStartPosition.y, 0);
        }
        //노트 y좌표 변경하기.
        if (notePrefabs.IndexOf(selectedNote) == 2) noteEndPosition.y = mousePosition.y;
        else noteEndPosition.y = noteStartPosition.y;



        if (noteStartPosition.x >= noteEndPosition.x) return;
        ShowNotePreview(noteEndPosition);
    }

    public void ShowNotePreview(Vector3 noteEndPosition) {
        if (notePreview != null) Destroy(notePreview);
        if (jumpEndIndicator != null) Destroy(jumpEndIndicator);
        notePreview = Instantiate(selectedNote, noteStartPosition, Quaternion.identity);
        noteSprite = notePreview.GetComponentInChildren<SpriteRenderer>();
        c = noteSprite.color;
        c.a = 0.5f;
        noteSprite.color = c;

        if (notePrefabs.IndexOf(selectedNote) == 2)
        {
            jumpEndIndicator = Instantiate(notePrefabs[10], noteEndPosition, Quaternion.identity);
            jumpEndIndicator.GetComponent<SpriteRenderer>().color = c;
        }

        switch (notePrefabs.IndexOf(selectedNote))
        {
            case 0:
            case 1:
                noteSprite.size = new Vector2(10 * (noteEndPosition.x - noteStartPosition.x), 2.5f);
                break;
            case 2:
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void PutNote() {
        if (noteStartPosition.x >= noteEndPosition.x || notePreview == null) return;
        // notePreview를 불투명하게 바꿔서 노트 설치하기.
        GameObject[] previousJumpIndicator = GameObject.FindGameObjectsWithTag("JumpIndicator");
        foreach (GameObject indicator in previousJumpIndicator)
        {
            if (indicator != null && indicator != jumpEndIndicator) Destroy(indicator);
        }
        c.a = 1f;
        noteSprite.color = c;
        if (jumpEndIndicator != null) jumpEndIndicator.GetComponent<SpriteRenderer>().color = c;
        // noteStartPosition 업데이트
        noteStartPosition = noteEndPosition;
        NoteSpawnInfo info;
        switch (notePrefabs.IndexOf(selectedNote))
        {
            case 0:
                info = new NoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Normal);
                Debug.Log("Hello from normalNote");
                break;
            case 1:
                // TODO: dashCoeff를 인겜에서 조정할 수 있게 하기
                info = new DashNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Dash, dashCoeff);
                Debug.Log("Hello from dashNote");
                break;
            case 2:
                // TODO: jumpHeight를 인겜에서 조정할 수 있게 하기
                info = new JumpNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Jump, noteEndPosition.y - noteStartPosition.y);
                Debug.Log("Hello from jumpNote");
                break;
            default:
                info = null;
                break;
        }
        //placedNotes.Add(notePreview);
        //noteSpawnInfos.Add(info);
        noteStorage.Add(new NoteInfoPair(notePreview, info));
        notePreview = null;
        jumpEndIndicator = null;
        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void DeleteLastNote() {
        if (noteStorage.Count == 0) return;
        NoteInfoPair pair = noteStorage[^1];
        noteStartPosition = pair.note.transform.position;
        Destroy(pair.note);

        noteStorage.RemoveAt(noteStorage.Count - 1);
        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void ChangeNoteWriteSetting(int setting) {
        noteWriteSetting = setting switch
        {
            0 => NoteWriteSetting.MouseDiscrete,
            1 => NoteWriteSetting.MouseContinuous,
            2 => NoteWriteSetting.WriteLength,
            _ => throw new NotImplementedException()
        };
    }

    public void ShowWriteLengthSetting() {
        noteLengthInputField.gameObject.SetActive(true);
        SetNotePreviewByWriteLength();
    }
    public void HideWriteLengthSetting() => noteLengthInputField.gameObject.SetActive(false);
    public void SetNotePreviewByWriteLength() {
        Debug.Log("Hello from start");
        string inputText = noteLengthInputField.text;
        double time;
        float noteWidth;
        Debug.Log(inputText);
        if (inputText.EndsWith('s') && double.TryParse(inputText.Substring(0, inputText.Length - 1), out time))
        {
            noteWidth = GameManager.myManager.CalculateInputWidthFromTime(time);
        }
        else {
            string[] bitExpression = inputText.Split('/');
            if (bitExpression.Length != 2) return;
            if (int.TryParse(bitExpression[0], out int multiply) && int.TryParse(bitExpression[1], out int bit))
            {
                noteWidth = GameManager.myManager.CalculateInputWidthFromTime(240 * multiply / (bpm * bit));
            }
            else return;
        }
        Debug.Log("Hello?");
        noteEndPosition.x = noteStartPosition.x + noteWidth;
    }

    public void SelectNote(int noteIndex) {
        selectedNote = notePrefabs[noteIndex];
        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void OpenNoteSetting(int noteIndex) {
        editorState = EditorState.OnSetting;
        switch (noteIndex) {
            case 0:
                normalNoteSettingPanel.SetActive(true);
                break;
            case 1:
                dashNoteSettingPanel.SetActive(true);
                break;
            case 2:
                jumpNoteSettingPanel.SetActive(true);
                break;
            default:
                break;
        }
        settingBackgroundPanel.SetActive(true);
    }

    public void QuitNoteSetting(int noteIndex) {
        switch (noteIndex)
        {
            case 0:
                if (/* float.TryParse(normalAngleInputField.text, out platformAngle)*/ true) {
                    if (/* 0 <= platformAngle && platformAngle < 90f */ true)
                    {
                        normalNoteSettingPanel.SetActive(false);
                        settingBackgroundPanel.SetActive(false);
                        editorState = EditorState.EditorMain;
                    }
                }
                break;
            case 1:
                if (/* float.TryParse(dashAngleInputField.text, out dashPlatformAngle) && */ float.TryParse(dashCoeffInputField.text, out dashCoeff))
                {
                    if (/* 0 <= dashPlatformAngle && dashPlatformAngle < 90f && */ 0 < dashCoeff)
                    {
                        dashNoteSettingPanel.SetActive(false);
                        settingBackgroundPanel.SetActive(false);
                        editorState = EditorState.EditorMain;
                    }
                }
                break;
            case 2:
                jumpNoteSettingPanel.SetActive(false);
                settingBackgroundPanel.SetActive(false);
                editorState = EditorState.EditorMain;
                break;
            default:
                break;
        }
    }

    

    // =========================== Save Map File ===============================
    public void OpenMapSavePanel() {
        mapSavePanel.SetActive(true);
        settingBackgroundPanel.SetActive(true);
        editorState = EditorState.OnSetting;
    }

    public void SaveEditorToMapFile() {
        filepath = Application.dataPath + "/SavedLevels/" + mapNameInputField.text + ".txt";
        writer = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        sw = new StreamWriter(writer);

        NoteSpawnInfo info;

        foreach (NoteInfoPair pair in noteStorage) {
            info = pair.info;
            switch (pair.info.noteType) {
                case NoteType.Normal:
                    sw.WriteLine("A " + info.spawnTime);
                    break;
                case NoteType.Dash:
                    DashNoteSpawnInfo dashInfo = info as DashNoteSpawnInfo;
                    sw.WriteLine("B " + dashInfo.spawnTime + " " + dashInfo.dashCoeff);
                    break;
                case NoteType.Jump:
                    JumpNoteSpawnInfo jumpInfo = info as JumpNoteSpawnInfo;
                    sw.WriteLine("C " + jumpInfo.spawnTime + " " + jumpInfo.jumpHeight);
                    break;
            }
        }
        sw.WriteLine("END");
        sw.Close();
        mapSavePanel.SetActive(false);
        settingBackgroundPanel.SetActive(false);
        editorState = EditorState.EditorMain;
    }
}

class NoteInfoPair {
    public GameObject note;
    public NoteSpawnInfo info;

    public NoteInfoPair(GameObject note, NoteSpawnInfo info) {
        this.note = note;
        this.info = info;
    }
}