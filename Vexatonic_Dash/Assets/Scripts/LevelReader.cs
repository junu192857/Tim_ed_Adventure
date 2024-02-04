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

    private double spawnTime; //계산을 통해 얻어내는 노트별 스폰 시간.
    private double _1bitTime; //한 마디의 지속시간. 계산법 = 240/(BPM)

    private double accTime; // 기본적으로 0이고, BPM이 바뀔 때마다 바뀌기 전까지 곡의 진행 시간이 저장된다.
    private int latestBPMChange; // 가장 최근에 BPM이 바뀐 마디수

    public static int noteCount; // 채보의 전체 노트수

    //맵 파일을 읽어 노트 정보에 관한 Queue로 반환하는 함수.
    [Obsolete("Gravity data added. Please use the newer one.")]
    public List<NoteSpawnInfo> ParseFile(string filepath)
    {
        return ParseFile(filepath, out List<GravityData> dummy);
    }
    
    public List<NoteSpawnInfo> ParseFile(string filepath, out List<GravityData> gravityDataList) {
        accTime = 0;
        latestBPMChange = 1;
        noteCount = 0;

        NoteSpawnInfo cur;
        NoteSpawnInfo prev;


        Stack<NoteSpawnInfo> list = new Stack<NoteSpawnInfo>();
        Stack<GravityData> gravityInfoStack = new Stack<GravityData>();
        gravityDataList = new List<GravityData>();

        string line;

        reader = new FileStream(filepath, FileMode.Open);
        sr = new StreamReader(reader);
        while (!sr.EndOfStream) { 
            line = sr.ReadLine();
            Debug.Log(line);

            var myList = line.Split(' ');
            if (line.StartsWith("NAME")) {
                GameManager.myManager.um.songName = myList[1];
            }
            else if (line.StartsWith("COMPOSER")) { 
                GameManager.myManager.um.composerName = myList[1];
            }
            else if (line.StartsWith("BPM")) // Ex) BPM 1 180: 1번째 마디부터 180 BPM.
            {
                accTime += _1bitTime * (int.Parse(myList[1]) - latestBPMChange);
                latestBPMChange = int.Parse(myList[1]);
                _1bitTime = 240 / double.Parse(line.Split(' ')[2]);
            }
            else if (line.StartsWith("GRAVITY")) // GRAVITY (시간) (중력 방향: 0이 아래, 90이 오른쪽)
            {
                GravityData prevGravityData;
                double gravityTime = double.Parse(myList[1]);
                int gravityAngle = int.Parse(myList[2]);

                if (gravityInfoStack.TryPeek(out prevGravityData) && (prevGravityData.time > gravityTime))
                    throw new ArgumentException("Gravity data does not entered in right order");
                
                gravityInfoStack.Push(new GravityData(gravityTime, gravityAngle));
            }
            else if (line.StartsWith("END")) {

                list.Peek().noteLastingTime = 1f;
                List<NoteSpawnInfo> returnList = list.ToList();
                returnList.Reverse();

                gravityDataList = gravityInfoStack.ToList();
                gravityDataList.Reverse();
                
                return returnList;
            }
            else {
                if (myList[0].Length != 1) {
                    Debug.LogError("Parse Error: Length of Note type letter is not 1");
                    return null;
                }
                char type = char.Parse(myList[0]);
                
                switch (type) {
                    case 'A':
                        // A (마디수) (n비트) (m번째) (경사도) (진행방향)
                        cur = GenerateNoteSpawnInfo(myList);
                        
                        if (list.TryPeek(out prev)) prev.noteLastingTime = cur.spawnTime - prev.spawnTime;
                        list.Push(cur);
                        break;
                    case 'B':
                        // B (마디수) (n비트) (m번째) (대쉬 계수) (종류) (경사도) (진행방향)
                        cur = GenerateNoteSpawnInfo(myList);
                        
                        if (list.TryPeek(out prev)) prev.noteLastingTime = cur.spawnTime - prev.spawnTime;
                        list.Push(cur);
                        break;
                    case 'C':
                        // C (마디수) (n비트) (m번째) (높이변화) (종류) (경사도) (진행방향)
                        cur = GenerateNoteSpawnInfo(myList);
                        
                        if (list.TryPeek(out prev)) prev.noteLastingTime = cur.spawnTime - prev.spawnTime;
                        if (!ValidateJump(prev.noteLastingTime, float.Parse(myList[4]))) {
                            Debug.LogError("Invalid jump note");
                            return null;
                        }
                        list.Push(cur);
                        break;
                    default:
                        Debug.LogError("Parse Error: Note type letter invalid");
                        return null;
                }
            }
        }
        Debug.LogError("This level does not have an END");
        return null;
    }

    private double CalculateSpawnTime(string[] myList) { 
        return accTime + (int.Parse(myList[1]) - latestBPMChange) * _1bitTime + (_1bitTime / double.Parse(myList[2]) * (int.Parse(myList[3]) - 1));
    }

    private bool ValidateJump(double noteLastingTime, float heightDelta) {
        return 0.5 * g * noteLastingTime * noteLastingTime < heightDelta;
    }

    private NoteSpawnInfo GenerateNoteSpawnInfo(string[] infoList)
    {
        // Local Variable Declaration
        NoteType noteType;
        int subTypeIndex = 0;
        int angleIndex = 0;
        int directionIndex = 0;
        int legacyLength = 0;
        double spawnTime = 0;
        
        NoteSpawnInfo generated;

        // Initialization by type
        switch (infoList[0])
        {
            case "A":
                noteType = NoteType.Normal;
                angleIndex = 4;
                directionIndex = 5;
                legacyLength = 4;
                break;
            case "B":
                noteType = NoteType.Dash;
                subTypeIndex = 5;
                angleIndex = 6;
                directionIndex = 7;
                legacyLength = 5;
                break;
            case "C":
                noteType = NoteType.Jump;
                subTypeIndex = 5;
                angleIndex = 6;
                directionIndex = 7;
                legacyLength = 5;
                break;
            default:
                throw new ArgumentException("Invalid note information provided in map file");
        }
        
        // Note Generation
        noteCount++;
        spawnTime = CalculateSpawnTime(infoList);
        
        generated = noteType switch
        {
            NoteType.Normal => new NoteSpawnInfo(spawnTime, NoteType.Normal),
            NoteType.Dash => new DashNoteSpawnInfo(spawnTime, NoteType.Dash, float.Parse(infoList[4])),
            NoteType.Jump => new JumpNoteSpawnInfo(spawnTime, NoteType.Jump, float.Parse(infoList[4])),
            _ => throw new ArgumentException("Unknown or unimplemented note type")
        };

        if (infoList.Length == legacyLength)
        {
            // Legacy format check
            Debug.LogWarning("Note information in the map file is in obsolete format. Please use the new style.");
            return generated;
        }

        if (noteType != NoteType.Normal)
            generated.noteSubType = infoList[subTypeIndex] switch
            {
                "G" => NoteSubType.Ground,
                "A" => NoteSubType.Air,
                "W" => NoteSubType.Wall,
                _ => throw new ArgumentException("Invalid note subtype provided")
            };

        generated.angle = int.Parse(infoList[angleIndex]);
        generated.direction = infoList[directionIndex] switch
        {
            "L" => CharacterDirection.Left,
            "R" => CharacterDirection.Right,
            _ => throw new ArgumentException("Invalid character direction provided")
        };

        return generated;
    }
}
