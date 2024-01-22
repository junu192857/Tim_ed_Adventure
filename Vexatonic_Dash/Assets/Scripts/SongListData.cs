using System.Collections.Generic;
using System.Linq;

/*
 * Temporary data structure, will be modified later
 */

public enum PatternType
{
    Easy = 0,
    Hard = 1,
    Vex  = 2
}

public struct SongData
{
    public SongData(string songName, string composerName, IEnumerable<int> difficulty, IEnumerable<string> filePath)
    {
        SongName = songName;
        ComposerName = composerName;
        Difficulty = difficulty as int[] ?? difficulty.ToArray();
        FilePath = filePath as string[] ?? filePath.ToArray();
    }
    
    public readonly string SongName;
    public readonly string ComposerName;
    public readonly int[] Difficulty;
    public readonly string[] FilePath;
}

public class SongListData
{
    public static readonly SongData[] SongList =
    {
        new ("Song1", "Various Artist", new []{1, 4, 7}, new []{"Assets/Maps/Song1/Easy.txt", "Assets/Maps/Song1/Hard.txt", "Assets/Maps/Song1/Vex.txt"}),
        new ("Song2", "Various Artist", new []{2, 6, 8}, new []{"Assets/Maps/Song2/Easy.txt", "Assets/Maps/Song2/Hard.txt", "Assets/Maps/Song2/Vex.txt"}),
        new ("Song3", "Various Artist", new []{4, 6, 9}, new []{"Assets/Maps/Song3/Easy.txt", "Assets/Maps/Song3/Hard.txt", "Assets/Maps/Song3/Vex.txt"}),
        new ("Song4", "Various Artist", new []{4, 7, 9}, new []{"Assets/Maps/Song4/Easy.txt", "Assets/Maps/Song4/Hard.txt", "Assets/Maps/Song4/Vex.txt"}),
        new ("Song5", "Various Artist", new []{5, 8, 10}, new []{"Assets/Maps/Song5/Easy.txt", "Assets/Maps/Song5/Hard.txt", "Assets/Maps/Song5/Vex.txt"}),
        new ("Song6", "Various Artist", new []{2, 7, 9}, new []{"Assets/Maps/Song6/Easy.txt", "Assets/Maps/Song6/Hard.txt", "Assets/Maps/Song6/Vex.txt"}),
        new ("Song7", "Various Artist", new []{3, 6, 8}, new []{"Assets/Maps/Song7/Easy.txt", "Assets/Maps/Song7/Hard.txt", "Assets/Maps/Song7/Vex.txt"})
    };
}
