using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class LevelReader
{
    private FileStream reader;
    private StreamReader sr;

    private double spawnTime; //계산을 통해 얻어내는 노트별 스폰 시간.
    private double _1bitTime; //한 마디의 지속시간. 계산법 = 240/(BPM)

    private double accTime; // 기본적으로 0이고, BPM이 바뀔 때마다 바뀌기 전까지 곡의 진행 시간이 저장된다.
    private int latestBPMChange; // 가장 최근에 BPM이 바뀐 마디수

    public static int noteCount; // 채보의 전체 노트수

    //맵 파일을 읽어 노트 정보에 관한 Queue로 반환하는 함수.
    public List<NoteSpawnInfo> ParseFile(string filepath) {
        accTime = 0;
        latestBPMChange = 1;
        noteCount = 0;


        Stack<NoteSpawnInfo> list = new Stack<NoteSpawnInfo>();

        string line;

        reader = new FileStream(filepath, FileMode.Open);
        sr = new StreamReader(reader);
        while (!sr.EndOfStream) { 
            line = sr.ReadLine();
            Debug.Log(line);

            var myList = line.Split(' ');
            if (line.StartsWith("NAME")) {
                //GameManager.myManager.um.songName = myList[1];
            }
            else if (line.StartsWith("COMPOSER")) { 
                //GameManager.myManager.um.composerName = myList[1];
            }
            else if (line.StartsWith("BPM")) // Ex) BPM 1 180: 1번째 마디부터 180 BPM.
            {
                accTime += _1bitTime * (int.Parse(myList[1]) - latestBPMChange);
                latestBPMChange = int.Parse(myList[1]);
                _1bitTime = 240 / double.Parse(line.Split(' ')[2]);
            }
            else if (line.StartsWith("END")) {
                Debug.Log("Parse End");

                list.Peek().platformScale = 1f;
                List<NoteSpawnInfo> returnList = list.ToList();
                returnList.Reverse();
                return returnList;
            }
            else {
                if (myList[0].Length != 1) {
                    Debug.LogError("Parse Error: Length of Note type letter is not 1");
                    return null;
                }
                char type = char.Parse(myList[0]);
                if ('A' <= type && type <= 'E')
                {
                    noteCount++;
                    double spawnTime = accTime + (int.Parse(myList[1]) - latestBPMChange) * _1bitTime + (_1bitTime / double.Parse(myList[2]) * (int.Parse(myList[3]) - 1));
                    NoteSpawnInfo cur = new NoteSpawnInfo(spawnTime, (NoteType)((int)type - 65));
                    Debug.Log(spawnTime);
                    if (list.TryPeek(out NoteSpawnInfo prev)) prev.platformScale = (float)(2 * (cur.spawnTime - prev.spawnTime));
                    list.Push(cur);
                }
                else {
                    Debug.LogError("Parse Error: Note type letter invalid");
                    return null;
                }
            }
        }
        Debug.LogError("This level does not have an END");
        return null;
    }

}
