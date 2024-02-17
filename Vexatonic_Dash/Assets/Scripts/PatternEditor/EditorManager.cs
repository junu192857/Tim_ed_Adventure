using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.InputSystem;
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
    private float musicOffset;
    public InputField offsetInputField;
    private IEnumerator songLineMoveCoroutine;
    private GameObject songLine;
    public Canvas canvas;
    private List<GameObject> lines;
    private int bit;
    private bool indicatorEnabled;


    [Header("EditorMain")]
    private Vector3 noteStartPosition; // 다음에 배치할 노트의 시작점
    private Vector3 noteEndPosition; // 다음에 배치할 노트의 끝점
    private GameObject notePreview; // 노트가 어떻게 생길지 미리 알려줌
    private Vector3 cameraPosition;

    public Button putNoteButton;
    public GameObject cameraSettingPanel;
    public InputField cameraTermInputField;
    public InputField cameraScaleInputField;
    public InputField cameraVxInputField;
    public InputField cameraVyInputField;
    public InputField cameraAngleInputField;
    public InputField cameraPosxInputField;
    public InputField cameraPosyInputField;
    public CameraControlType cct;
    public GameObject termIndicator;

    private CharacterDirection direction;
    public Text directionText;
    public List<GameObject> notePrefabs;
    public List<Sprite> CameraSprites;
    public GameObject jumpEndIndicator;
    private GameObject selectedNote;
    private Color c;
    private SpriteRenderer noteSprite;
    private int[] angleArray = { 0, 30, 45, 60, -30, -45, -60, 0, 30, 45, 60, -30, -45, -60, 0 };
    private int platformAngle;
    private int dashPlatformAngle;
    private float dashCoeff;
    private int gravity;
    private bool hasEnd;
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
    private List<CameraControlInfo> cameraStorage;
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
        cameraStorage = new List<CameraControlInfo>();
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
            musicOffset = 0f;
            offsetInputField.text = $"{musicOffset}";
            direction = CharacterDirection.Right;
            indicatorEnabled = true;
            noteStartPosition = Vector3.zero;
            noteEndPosition = Vector3.zero;
            dashCoeff = 1.5f;
            gravity = 0;
            hasEnd = false;
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

    public void ChangeBit() { if (int.TryParse(bitInputField.text, out bit) && bit > 0) ReloadMeasureCountLine(); }

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
            StartCoroutine(StartSongCoroutine(startPositionX));
        }
        else {
            song.Stop();
            songLineMoveCoroutine = FixSongLineCoroutine(songLine);
        }
        StartCoroutine(songLineMoveCoroutine);
    }

    private IEnumerator StartSongCoroutine(float startX) {
        if (startX < 0) startX = 0;
        float musicTime = (float)GameManager.myManager.CalculateTimeFromInputWidth(startX) + musicOffset / 1000;
        if (musicTime >= 0f) {
            song.time = musicTime;
            song.Play();
            yield return null;
        }
        else {
            yield return new WaitForSeconds(-musicTime);
            song.time = 0f;
            song.Play();
        }
    }
    private IEnumerator MoveSongLineCoroutine(float startX, GameObject line)
    {
        if (startX < 0) startX = 0;
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

    public void ChangeMusicOffset(float value) {
        musicOffset = value < 0 ? musicOffset - 1 : musicOffset + 1;
        offsetInputField.text = musicOffset.ToString();
    }

    public void ChangeMusicOffsetDirectly() => float.TryParse(offsetInputField.text, out musicOffset);

    public void ChangeDirection(float value) {
        if (value < 0)
        {
            direction = CharacterDirection.Left;
            directionText.text = "Direction: Left";
        }
        else {
            direction = CharacterDirection.Right;
            directionText.text = "Direction: Right";
        }
    }

    // =========================== Editor Main ===============================


    private void Update()
    {
        // 아래는 노트의 미리보기를 생성하는 부분
        if (editorState != EditorState.EditorMain || selectedNote == null || hasEnd) return;
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (notePrefabs.IndexOf(selectedNote) == 19) {
            if (noteWriteSetting == NoteWriteSetting.MouseDiscrete) {
                double lineGap = GameManager.myManager.CalculateInputWidthFromTime(240 / (bpm * bit));
                int bitCount = (int)(mousePosition.x / lineGap);
                cameraPosition = new Vector3((float)(lineGap * bitCount), mousePosition.y, 0);
            }
            else cameraPosition = mousePosition;
            ShowNotePreviewForCamera(cameraPosition);
        }
        else
        {
            //NoteEndPosition x좌표 변경하기. NoteWriteSetting이 WriteLength라면 별개의 함수를 통해 변경 << WriteLength는 기술력 문제로 삭제
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
            //Index 30인 위치에 있는 EndNote 이후로 추가 노트는 등장하지 않을 예정이라 대충 구현함
            //if (notePrefabs.IndexOf(selectedNote) >= 15 && notePrefabs.IndexOf(selectedNote) != 30) noteEndPosition.y = mousePosition.y;
            //else noteEndPosition.y = noteStartPosition.y + Mathf.Tan(Mathf.Deg2Rad * angleArray[notePrefabs.IndexOf(selectedNote)]);

            noteEndPosition.y = notePrefabs.IndexOf(selectedNote) switch
            {
                30 => noteStartPosition.y,
                <= 14 => noteStartPosition.y + Mathf.Tan(Mathf.Deg2Rad * angleArray[notePrefabs.IndexOf(selectedNote)]),
                >= 15 and < 30 => mousePosition.y,
                _ => throw new ArgumentOutOfRangeException()
            };


            if (noteStartPosition.x >= noteEndPosition.x) return;
            ShowNotePreview(noteEndPosition);
        }
    }

    private void ShowNotePreview(Vector3 noteEndPosition) {
        if (notePreview != null) Destroy(notePreview);
        if (jumpEndIndicator != null) Destroy(jumpEndIndicator);
        notePreview = Instantiate(selectedNote, noteStartPosition, Quaternion.identity);
        noteSprite = notePreview.GetComponentInChildren<SpriteRenderer>();
        c = noteSprite.color;
        c.a = 0.5f;
        noteSprite.color = c;

        // Air Dash나 모든 종류의 Jump일 때, Jump Indicator를 놓는다. 다른 노트들과 달리 noteEndPosition을 명확하게 볼 수 없기 때문.
        if (notePrefabs.IndexOf(selectedNote) >= 14 && notePrefabs.IndexOf(selectedNote) != 30)
        {
            jumpEndIndicator = Instantiate(notePrefabs[18], noteEndPosition, Quaternion.identity);
            jumpEndIndicator.GetComponent<SpriteRenderer>().color = c;
        }

        switch (notePrefabs.IndexOf(selectedNote))
        {
            case 0:
            case 7:
                noteSprite.size = new Vector2(10 * (noteEndPosition.x - noteStartPosition.x), 2.5f);
                break;
            case 1:
            case 4:
            case 8:
            case 11:
                noteSprite.size = new Vector2(10 * (noteEndPosition.x - noteStartPosition.x), 8.27f);
                break;
            case 2:
            case 5:
            case 9:
            case 12:
                noteSprite.size = new Vector2(10 * (noteEndPosition.x - noteStartPosition.x), 12.5f);
                break;
            case 3:
            case 6:
            case 10:
            case 13:
                noteSprite.size = new Vector2(10 * (noteEndPosition.x - noteStartPosition.x), 19.82f);
                break;
            case 14:
            case 15:
            case 16:
            case 17:
            case 30:
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void ShowNotePreviewForCamera(Vector3 cameraPosition) {
        if (notePreview != null) Destroy(notePreview);
        if (jumpEndIndicator != null) Destroy(jumpEndIndicator);
        notePreview = Instantiate(selectedNote, cameraPosition, Quaternion.identity);
        noteSprite = notePreview.GetComponentInChildren<SpriteRenderer>();
        c = noteSprite.color;
        c.a = 0.5f;
        noteSprite.color = c;
    }

    public void PutNote() {

        if (noteStartPosition.x >= noteEndPosition.x || notePreview == null || hasEnd) return;
        // notePreview를 불투명하게 바꿔서 노트 설치하기.
        GameObject[] previousJumpIndicator = GameObject.FindGameObjectsWithTag("JumpIndicator");
        foreach (GameObject indicator in previousJumpIndicator)
        {
            if (indicator != null && indicator != jumpEndIndicator) Destroy(indicator);
        }
        // TODO: 벽점프 노트면 자동 direction 및 directionText 변화시키기. 벽점프는 무조건 진행방향의 반대이므로, direction이 유저 설정에 의존하지 않고 이전 노트의 direction에 의존한다.
        if (notePrefabs.IndexOf(selectedNote) == 17) {
            switch (noteStorage.Last().info.direction) { 
                case CharacterDirection.Left:
                    direction = CharacterDirection.Right;
                    directionText.text = "Direction: Right";
                    break;
                case CharacterDirection.Right:
                    direction = CharacterDirection.Left;
                    directionText.text = "Direction: Left";
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        c.a = direction == CharacterDirection.Right ? 1f : 0.5f;
        noteSprite.color = c;

        if (jumpEndIndicator != null) jumpEndIndicator.GetComponent<SpriteRenderer>().color = c;

        NoteSpawnInfo info;

        switch (notePrefabs.IndexOf(selectedNote))
        {
            case int i when i <= 6:
                // A (스폰시간) 0 0 (경사도) (진행방향) 
                info = new NoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Normal)
                {
                    angle = platformAngle,
                    direction = direction,
                };
                break;
            case int i when 6 < i && i <= 13:
                // B (스폰시간) (대쉬 계수) G (경사도) (진행방향)
                info = new DashNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Dash, dashCoeff)
                {
                    noteSubType = NoteSubType.Ground,
                    angle = dashPlatformAngle,
                    direction = direction
                };
                break;
            case 14:
                // B (스폰시간) (대쉬 계수) A 0 (진행방향)
                info = new DashNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Dash, dashCoeff)
                {
                    noteSubType = NoteSubType.Air,
                    angle = 0,
                    direction = direction
                };
                break;
            case 15:
                // C (스폰시간) (높이변화) G 0 (진행방향)
                info = new JumpNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Jump, noteEndPosition.y - noteStartPosition.y)
                {
                    noteSubType = NoteSubType.Ground,
                    direction = direction
                };
                break;
            case 16:
                // C (스폰시간) (높이변화) A 0 (진행방향)
                info = new JumpNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Jump, noteEndPosition.y - noteStartPosition.y)
                {
                    noteSubType = NoteSubType.Air,
                    direction = direction
                };
                break;
            case 17:
                // C (스폰시간) (높이변화) W 0 (진행방향)
                info = new JumpNoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Jump, noteEndPosition.y - noteStartPosition.y)
                {
                    noteSubType = NoteSubType.Wall,
                    direction = direction
                };
                break;
            case 30:
                info = new NoteSpawnInfo(GameManager.myManager.CalculateTimeFromInputWidth(noteStartPosition.x), NoteType.Normal)
                {
                    noteSubType = NoteSubType.End,
                    angle = 0,
                    direction = direction,
                };
                hasEnd = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        //placedNotes.Add(notePreview);
        //noteSpawnInfos.Add(info);

        // noteStartPosition 업데이트
        noteStartPosition = noteEndPosition;
        noteStorage.Add(new NoteInfoPair(notePreview, info));
        notePreview = null;
        jumpEndIndicator = null;
        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void PutCameraNote() {
        if (notePreview == null || hasEnd) return;

        GameObject[] previousJumpIndicator = GameObject.FindGameObjectsWithTag("JumpIndicator");
        foreach (GameObject indicator in previousJumpIndicator)
        {
            if (indicator != null && indicator != jumpEndIndicator) Destroy(indicator);
        }
        
        c.a = 1f;
        noteSprite.color = c;
        
        OpenCameraSetting();

        //카메라 조작 종류에 따라 info 알아서 설정하기
    }


    public void DeleteLastNote() {
        if (noteStorage.Count == 0) return;
        NoteInfoPair pair = noteStorage[^1];
        noteStartPosition = pair.note.transform.position;
        if (pair.info.noteSubType == NoteSubType.End) hasEnd = false;
        Destroy(pair.note);

        noteStorage.RemoveAt(noteStorage.Count - 1);
        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void DeleteLastCamera() {
        if (cameraStorage.Count == 0) return;
        CameraControlInfo info = cameraStorage[^1];
        Destroy(info.parent);
        cameraStorage.RemoveAt(cameraStorage.Count - 1);
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
        string inputText = noteLengthInputField.text;
        double time;
        float noteWidth;
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
        noteEndPosition.x = noteStartPosition.x + noteWidth;
    }

    public void SelectNote(int noteIndex) {
        selectedNote = notePrefabs[noteIndex];
        if (noteIndex < 7) platformAngle = angleArray[noteIndex];
        else if (noteIndex < 14) dashPlatformAngle = angleArray[noteIndex];

        putNoteButton.onClick.RemoveAllListeners();
        if (noteIndex == 19) putNoteButton.onClick.AddListener(PutCameraNote);
        else putNoteButton.onClick.AddListener(PutNote);

        if (noteWriteSetting == NoteWriteSetting.WriteLength) SetNotePreviewByWriteLength();
    }

    public void OpenCameraSetting() {
        editorState = EditorState.OnSetting;
        cameraSettingPanel.SetActive(true);
    }

    public void CloseCameraSetting() {
        if (!CheckValidInput()) return;
        double term = double.Parse(cameraTermInputField.text);

        switch (cct)
        {
            case CameraControlType.Zoom:
                CameraZoomInfo zi = new CameraZoomInfo(GameManager.myManager.CalculateTimeFromInputWidth(cameraPosition.x), term, double.Parse(cameraScaleInputField.text))
                {
                    parent = notePreview
                };
                noteSprite.sprite = CameraSprites[0];
                cameraStorage.Add(zi);
                break;
            case CameraControlType.Velocity:
                CameraVelocityInfo vi = new CameraVelocityInfo(GameManager.myManager.CalculateTimeFromInputWidth(cameraPosition.x), new Vector2(float.Parse(cameraVxInputField.text), float.Parse(cameraVyInputField.text))) {
                    parent = notePreview
                };
                cameraStorage.Add(vi);
                noteSprite.sprite = CameraSprites[1];
                break;
            case CameraControlType.Rotate:
                CameraRotateInfo ri = new CameraRotateInfo(GameManager.myManager.CalculateTimeFromInputWidth(cameraPosition.x), term, int.Parse(cameraAngleInputField.text))
                {
                    parent = notePreview
                };
                cameraStorage.Add(ri);
                noteSprite.sprite = CameraSprites[2];
                break;
            case CameraControlType.Fix:
                CameraFixInfo fi = new CameraFixInfo(GameManager.myManager.CalculateTimeFromInputWidth(cameraPosition.x), term, new Vector2(float.Parse(cameraPosxInputField.text), float.Parse(cameraPosyInputField.text)))
                {
                    parent = notePreview
                };
                cameraStorage.Add(fi);
                noteSprite.sprite = CameraSprites[3];
                break;
            case CameraControlType.Return:
                CameraReturnInfo rei = new CameraReturnInfo(GameManager.myManager.CalculateTimeFromInputWidth(cameraPosition.x), term)
                {
                    parent = notePreview
                };
                cameraStorage.Add(rei);
                noteSprite.sprite = CameraSprites[4];
                break;
            default:
                throw new ArgumentOutOfRangeException();        
        }

        if (cct != CameraControlType.Velocity) {
            GameObject termIndicator = Instantiate(this.termIndicator, notePreview.transform);
            termIndicator.transform.localScale = new Vector3(GameManager.myManager.CalculateInputWidthFromTime(term), 1, 1);
        }

        notePreview = null;
        jumpEndIndicator = null;
        cameraSettingPanel.SetActive(false);
        editorState = EditorState.EditorMain;
    }

    private bool CheckValidInput() {
        if (!double.TryParse(cameraTermInputField.text, out double t)) return false;
        switch (cct) {
            case CameraControlType.Zoom:
                if (!double.TryParse(cameraScaleInputField.text, out double s)) return false;
                break;
            case CameraControlType.Velocity:
                if (!float.TryParse(cameraVxInputField.text, out float x) || !float.TryParse(cameraVyInputField.text, out float y)) return false;
                break;
            case CameraControlType.Rotate:
                if (!int.TryParse(cameraAngleInputField.text, out int a)) return false;
                break;
            case CameraControlType.Fix:
                if (!float.TryParse(cameraPosxInputField.text, out float px) || !float.TryParse(cameraPosyInputField.text, out float py)) return false;
                break;
            default:
                break;
        }
        return true;
    }

    public void SetCameraType(int index) {
        cct = index switch
        {
            0 => CameraControlType.Zoom,
            1 => CameraControlType.Velocity,
            2 => CameraControlType.Rotate,
            3 => CameraControlType.Fix,
            4 => CameraControlType.Return,
            _ => throw new ArgumentOutOfRangeException()
        };
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
                normalNoteSettingPanel.SetActive(false);
                settingBackgroundPanel.SetActive(false);
                editorState = EditorState.EditorMain;
                break;
            case 1:
                if (float.TryParse(dashCoeffInputField.text, out dashCoeff) && 0 < dashCoeff)
                {
                    dashNoteSettingPanel.SetActive(false);
                    settingBackgroundPanel.SetActive(false);
                    editorState = EditorState.EditorMain;
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
        if (!hasEnd) {
            Debug.LogWarning("This map do not have an End note");
            return;
        }

        mapSavePanel.SetActive(true);
        settingBackgroundPanel.SetActive(true);
        editorState = EditorState.OnSetting;
    }

    public void SaveEditorToMapFile() {
        filepath = Application.dataPath + "/SavedLevels/" + mapNameInputField.text + ".txt";
        writer = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        sw = new StreamWriter(writer);

        sw.WriteLine("OFFSET " + musicOffset.ToString());

        cameraStorage = cameraStorage.OrderBy(i => i.time).ToList();
        foreach (CameraControlInfo cci in cameraStorage) {
            sw.WriteLine(MakeCameraInfoString(cci));
        }


        foreach (NoteInfoPair pair in noteStorage) {
            sw.WriteLine(MakeNoteInfoString(pair.info));
        }
        sw.WriteLine("END");
        sw.Close();
        mapSavePanel.SetActive(false);
        settingBackgroundPanel.SetActive(false);
        editorState = EditorState.EditorMain;
    }

    private string MakeCameraInfoString(CameraControlInfo cci) {
        string type = cci.type switch {
            CameraControlType.Zoom => "ZOOM ",
            CameraControlType.Velocity => "VELOCITY ",
            CameraControlType.Rotate => "ROTATE ",
            CameraControlType.Fix => "FIX ",
            CameraControlType.Return => "RETURN ",
            _ => throw new ArgumentException()
        };

        string time = cci.time.ToString() + " ";

        string term = cci.type == CameraControlType.Velocity ? "0 " : cci.term.ToString() + " ";

        string another1 = cci.type switch {
            CameraControlType.Zoom => (cci as CameraZoomInfo).scale.ToString(),
            CameraControlType.Velocity => (cci as CameraVelocityInfo).cameraVelocity.x.ToString(),
            CameraControlType.Rotate => (cci as CameraRotateInfo).angle.ToString(),
            CameraControlType.Fix => (cci as CameraFixInfo).fixPivotDelta.x.ToString(),
            CameraControlType.Return => "",
            _ => throw new ArgumentException()
        };
        another1 += " ";

        string another2 = cci.type switch {
            CameraControlType.Zoom => "",
            CameraControlType.Velocity => (cci as CameraVelocityInfo).cameraVelocity.y.ToString(),
            CameraControlType.Rotate => "",
            CameraControlType.Fix => (cci as CameraFixInfo).fixPivotDelta.y.ToString(),
            CameraControlType.Return => "",
            _ => throw new ArgumentException()
        };
        another2 += " ";

        return "CAM " + type + time + term + another1 + another2;
    }

    private string MakeNoteInfoString(NoteSpawnInfo info) {
        string type = info.noteType switch {
            NoteType.Normal => "A ",
            NoteType.Dash => "B ",
            NoteType.Jump => "C ",
            _ => "X "
        };
        string spawnTime = info.spawnTime.ToString() + " ";
        string dashCoeffOrJumpHeight = info.noteType switch {
            NoteType.Normal => "0 ",
            NoteType.Dash => (info as DashNoteSpawnInfo).dashCoeff.ToString() + " ",
            NoteType.Jump => (info as JumpNoteSpawnInfo).jumpHeight.ToString() + " ",
            _ => "X "
        };
        string subType = info.noteSubType switch
        {
            NoteSubType.Air => "A ",
            NoteSubType.Wall => "W ",
            NoteSubType.Ground => "G ",
            NoteSubType.End => "E ",
            _ => "0 "
        };
        string angle = info is JumpNoteSpawnInfo ? "0 " : info.angle.ToString() + " ";
        string direction = info.direction == CharacterDirection.Left ? "L" : "R";

        return type + spawnTime + dashCoeffOrJumpHeight + subType + angle + direction;
    }

    //========================================= Input Actions ===================================================
    public void OnMusicOffset(InputValue value)
    {
        if (editorState != EditorState.EditorMain) return;
        ChangeMusicOffset(value.Get<float>());
    }

    public void OnDirectionToggle(InputValue value)
    {
        if (editorState != EditorState.EditorMain) return;
        ChangeDirection(value.Get<float>());
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

