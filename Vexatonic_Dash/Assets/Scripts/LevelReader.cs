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

    public static int noteCount; // 채보의 전체 노트수

    //맵 파일을 읽어 노트 정보에 관한 Queue로 반환하는 함수.
    [Obsolete("Gravity data added. Please use the newer one.")]
    public List<NoteSpawnInfo> ParseFile(string filepath)
    {
        return ParseFile(filepath, out List<GravityData> dummy);
    }
    
    public List<NoteSpawnInfo> ParseFile(string filepath, out List<GravityData> gravityDataList) {
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
            if (line.StartsWith("GRAVITY")) // GRAVITY (시간) (중력 방향: 0이 아래, 90이 오른쪽)
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
                            return null;
                        }
                    }
                }
                list.Push(cur);
            }
        }
        Debug.LogError("This level does not have an END");
        return null;
    }

    private bool ValidateJump(double noteLastingTime, float jumpHeight) {
        return 0.5 * g * noteLastingTime * noteLastingTime < jumpHeight;
    }

    private NoteSpawnInfo GenerateNoteSpawnInfo(string[] infoList)
    {
        // Local Variable Declaration
        NoteType noteType;
        int legacyLength = 0;
        double spawnTime = 0;
        
        NoteSpawnInfo generated;

        // Initialization by type
        switch (infoList[0])
        {
            case "A":
                noteType = NoteType.Normal;
                legacyLength = 2;
                break;
            case "B":
                noteType = NoteType.Dash;
                legacyLength = 3;
                break;
            case "C":
                noteType = NoteType.Jump;
                legacyLength = 3;
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

        if (infoList.Length == legacyLength)
        {
            // Legacy format check
            Debug.LogWarning("Note information in the map file is in obsolete format. Please use the new style.");
            return generated;
        }

        if (noteType != NoteType.Normal)
            generated.noteSubType = infoList[3] switch
            {
                "G" => NoteSubType.Ground,
                "A" => NoteSubType.Air,
                "W" => NoteSubType.Wall,
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
}
