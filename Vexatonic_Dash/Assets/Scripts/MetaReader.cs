using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum Difficulty
{
    Easy = 0,
    Hard = 1,
    Vex  = 2
}

public struct SongData
{
    public SongData(string songName, string composerName, IEnumerable<int> levels, string audioFilePath, IEnumerable<string> patternFilePath, bool isEvent = false)
    {
        SongName = songName;
        ComposerName = composerName;
        Levels = levels as int[] ?? levels.ToArray();
        AudioFilePath = audioFilePath;
        PatternFilePath = patternFilePath as string[] ?? patternFilePath.ToArray();
        IsEvent = isEvent;
    }

    public string SongName;
    public string ComposerName;
    public int[] Levels;
    public string AudioFilePath;
    public string[] PatternFilePath;
    public bool IsEvent;
}

public static class MetaReader
{
    private const string MetaFileName = "meta.txt";

    /// <summary>
    /// Stores song meta data while game is running.
    /// </summary>
    public static List<SongData> SongMetaList { get; private set; }

    /// <summary>
    /// Read song meta from Levels folder and stores to <see cref="SongMetaList"/>. Called only once.
    /// </summary>
    public static void GetSongMeta()
    {
        if (SongMetaList is null) SongMetaList = new List<SongData>();
        else SongMetaList.Clear();

        foreach (var d in Directory.GetDirectories(GameManager.SongsDirectory))
        {
            var dir = d.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Debug.Log($"Searching directory {dir}");
            
            var metaDir = dir + Path.DirectorySeparatorChar + MetaFileName;

            if (!File.Exists(metaDir))
            {
                Debug.LogWarning($"Meta file not found in {dir}");
                continue;
            }
            
            var currentSongData = new SongData();
            var fileStream = new FileStream(metaDir, FileMode.Open);
            var streamReader = new StreamReader(fileStream);
            
            currentSongData.PatternFilePath = new[] { "", "", "" };

            while (!streamReader.EndOfStream)
            {
                var lineSplit = streamReader.ReadLine()?.Split('^', 2);

                if (lineSplit is null) continue;

                switch (lineSplit[0])
                {
                    case "SONG":
                        currentSongData.SongName = lineSplit[1];
                        Debug.Log("Song name: " + lineSplit[1]);
                        break;
                    case "COMPOSER":
                        currentSongData.ComposerName = lineSplit[1];
                        Debug.Log("Composer name: " + lineSplit[1]);
                        break;
                    case "LEVEL":
                        currentSongData.Levels = lineSplit[1].Split('^').Select(int.Parse).ToArray();
                        Debug.Log("Levels: " + lineSplit[1]);
                        break;
                    case "AUDIO":
                        Debug.Log("Hello?");
                        currentSongData.AudioFilePath = dir + Path.DirectorySeparatorChar + lineSplit[1];
                        Debug.Log("Audio file path: " + lineSplit[1]);
                        break;
                    case "EASY":
                        currentSongData.PatternFilePath[0] = dir + Path.DirectorySeparatorChar + lineSplit[1];
                        Debug.Log("Easy level: " + lineSplit[1]);
                        break;
                    case "HARD":
                        currentSongData.PatternFilePath[1] = dir + Path.DirectorySeparatorChar + lineSplit[1];
                        Debug.Log("Hard level: " + lineSplit[1]);
                        break;
                    case "VEX":
                        currentSongData.PatternFilePath[2] = dir + Path.DirectorySeparatorChar + lineSplit[1];
                        Debug.Log("Vex level: " + lineSplit[1]);
                        break;
                    case "EVENT":
                        currentSongData.IsEvent = true;
                        break;
                }
            }
            
            Debug.Log($"End searching directory {dir}");

            streamReader.Close();
            SongMetaList.Add(currentSongData);
        }
        
        Debug.Log($"End searching all {SongMetaList.Count} directories");
        GameManager.myManager.isMetaLoaded = true;
    }
}
