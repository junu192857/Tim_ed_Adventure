using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class LevelReader
{
    private float g => GameManager.g;

    private FileStream reader;
    private StreamReader sr;

    public static int noteCount; // 채보의 전체 노트수

    //맵 파일을 읽어 노트 정보에 관한 Queue로 반환하는 함수.
    [Obsolete("Gravity data added. Please use the newer one.")]
    public List<NoteSpawnInfo> ParseFile(string filepath)
    {
        return ParseFile(filepath, out List<GravityData> dummy, out List<CameraControlInfo> dummy2);
    }
    
    public List<NoteSpawnInfo> ParseFile(string filepath, out List<GravityData> gravityDataList, out List<CameraControlInfo> cameraControlList) {
        noteCount = 0;

        NoteSpawnInfo cur;
        NoteSpawnInfo prev;


        Stack<NoteSpawnInfo> list = new Stack<NoteSpawnInfo>();
        Stack<GravityData> gravityInfoStack = new Stack<GravityData>();
        Stack<CameraControlInfo> cameraInfoStack = new Stack<CameraControlInfo>();
        gravityDataList = new List<GravityData>();
        cameraControlList = new List<CameraControlInfo>();

        string line;

        reader = new FileStream(filepath, FileMode.Open);
        sr = new StreamReader(reader);
        while (!sr.EndOfStream) { 
            line = sr.ReadLine();
            Debug.Log(line);

            var myList = line.Split(' ');
            /*
            if (line.StartsWith("NAME")) {
                GameManager.myManager.um.songName = myList[1];
            }
            else if (line.StartsWith("COMPOSER")) { 
                GameManager.myManager.um.composerName = myList[1];
            } 
            if (line.StartsWith("BPM")) // Ex) BPM 1 180: 1번째 마디부터 180 BPM.
            {
                accTime += _1bitTime * (int.Parse(myList[1]) - latestBPMChange);
                latestBPMChange = int.Parse(myList[1]);
                _1bitTime = 240 / double.Parse(line.Split(' ')[2]);
            }
            */
            if (line.StartsWith("OFFSET")) // OFFSET (시간)
            {
                GameManager.myManager.levelOffset = double.Parse(myList[1]);
            }
            else if (line.StartsWith("GRAVITY")) // GRAVITY (시간) (중력 방향: 0이 아래, 90이 오른쪽)
            {
                GravityData prevGravityData;
                double gravityTime = double.Parse(myList[1]);
                int gravityAngle = int.Parse(myList[2]);

                if (gravityInfoStack.TryPeek(out prevGravityData) && (prevGravityData.time > gravityTime))
                {
                    sr.Close();
                    throw new ArgumentException("Gravity data does not entered in right order");
                }

                gravityInfoStack.Push(new GravityData(gravityTime, gravityAngle));
            }
            else if (line.StartsWith("CAM"))
            {
                CameraControlInfo camInfo = generateCamControlInfo(myList);

                if (cameraInfoStack.TryPeek(out CameraControlInfo prevCamData) && prevCamData.time > camInfo.time)
                {
                    sr.Close();
                    throw new ArgumentException("Camera control data does not entered in right order");
                }

                cameraInfoStack.Push(camInfo);
            }
            else {
                if (myList[0].Length != 1) {
                    Debug.LogError("Parse Error: Length of Note type letter is not 1");
                    sr.Close();
                    return null;
                }
                        
                // A (스폰시간) 0 (종류) (경사도) (진행방향)
                // B (스폰시간) (대쉬 계수) (종류) (경사도) (진행방향)
                // C (스폰시간) (높이변화) (종류) (경사도) (진행방향)
                cur = GenerateNoteSpawnInfo(myList);
                
                if (list.TryPeek(out prev))
                {
                    prev.noteLastingTime = cur.spawnTime - prev.spawnTime;
                    if (prev.noteType == NoteType.Jump)
                    {
                        JumpNoteSpawnInfo jump = prev as JumpNoteSpawnInfo;
                        if (!ValidateJump(jump.noteLastingTime, jump.jumpHeight))
                        {
                            Debug.LogError("Invalid jump note");
                            sr.Close();
                            return null;
                        }
                    }
                }
                list.Push(cur);

                if (cur.noteSubType == NoteSubType.End) {
                    cur.noteLastingTime = 1f;
                    List<NoteSpawnInfo> returnList = list.ToList();
                    returnList.Reverse();

                    gravityDataList = gravityInfoStack.ToList();
                    gravityDataList.Reverse();

                    cameraControlList = cameraInfoStack.ToList();
                    cameraControlList.Reverse();

                    sr.Close();
                    return returnList;
                }
            }
        }
        Debug.LogError("This level does not have an END Note");
        sr.Close();
        return null;
    }

    private bool ValidateJump(double noteLastingTime, float jumpHeight) {
        return 0.5 * g * noteLastingTime * noteLastingTime < jumpHeight;
    }

    private NoteSpawnInfo GenerateNoteSpawnInfo(string[] infoList)
    {
        // Local Variable Declaration
        NoteType noteType;
        // int legacyLength = 0;
        double spawnTime = 0;
        
        NoteSpawnInfo generated;

        // Initialization by type
        switch (infoList[0])
        {
            case "A":
                noteType = NoteType.Normal;
                // legacyLength = 2;
                break;
            case "B":
                noteType = NoteType.Dash;
                // legacyLength = 3;
                break;
            case "C":
                noteType = NoteType.Jump;
                // legacyLength = 3;
                break;
            default:
                throw new ArgumentException("Invalid note information provided in map file");
        }
        
        // Note Generation
        noteCount++;
        spawnTime = double.Parse(infoList[1]);
        
        generated = noteType switch
        {
            NoteType.Normal => new NoteSpawnInfo(spawnTime, NoteType.Normal),
            NoteType.Dash => new DashNoteSpawnInfo(spawnTime, NoteType.Dash, float.Parse(infoList[2])),
            NoteType.Jump => new JumpNoteSpawnInfo(spawnTime, NoteType.Jump, float.Parse(infoList[2])),
            _ => throw new ArgumentException("Unknown or unimplemented note type")
        };

        // Legacy format을 더 이상 이용할 일이 없음.
        /*if (infoList.Length == legacyLength)
        {
            // Legacy format check
            Debug.LogWarning("Note information in the map file is in obsolete format. Please use the new style.");
            return generated;
        }*/

        // if (noteType != NoteType.Normal) 
        // Comment: 이제 Normal이어도 E라는 특수한 NoteSubType을 가질 수 있음. 그 외의 모든 NormalNote는 Ground로 처리
            generated.noteSubType = infoList[3] switch
            {
                "G" => NoteSubType.Ground,
                "A" => NoteSubType.Air,
                "W" => NoteSubType.Wall,
                "E" => NoteSubType.End,
                _ => throw new ArgumentException("Invalid note subtype provided")
            };

        generated.angle = int.Parse(infoList[4]);
        generated.direction = infoList[5] switch
        {
            "L" => CharacterDirection.Left,
            "R" => CharacterDirection.Right,
            _ => throw new ArgumentException("Invalid character direction provided")
        };

        return generated;
    }

    private CameraControlInfo generateCamControlInfo(string[] infoList)
    {
        // Local variable declaration & definition
        string subCommand = infoList[1];
        double time = double.Parse(infoList[2]);
        double term = double.Parse(infoList[3]);
        CameraControlInfo info;

        // Subcommand Process
        // CAM ZOOM (time) (term) (scale)
        // CAM VELOCITY (time) 0 (x) (y)
        // CAM ROTATE (time) (term) (angle)
        // CAM FIX (time) (term) (x) (y)
        // CAM RETURN (time) (term)
        switch (subCommand)
        {
            case "ZOOM":
                double zoomScale = double.Parse(infoList[4]);
                info = new CameraZoomInfo(time, term, zoomScale);
                break;
            case "VELOCITY":
                info = new CameraVelocityInfo(time, false, new Vector2(
                    float.Parse(infoList[4]),
                    float.Parse(infoList[5])
                ));
                break;
            case "ROTATE":
                int rotateAngle = int.Parse(infoList[4]);
                info = new CameraRotateInfo(time, term, rotateAngle);
                break;
            case "FIX":
                info = new CameraFixInfo(time, term, new Vector2(
                    float.Parse(infoList[5]),
                    float.Parse(infoList[6])
                ));
                break;
            case "RETURN":
                info = new CameraReturnInfo(time, term);
                break;
            default:
                throw new ArgumentException("Unknown camera control subcommand");
        }
        
        return info;
    }
}
