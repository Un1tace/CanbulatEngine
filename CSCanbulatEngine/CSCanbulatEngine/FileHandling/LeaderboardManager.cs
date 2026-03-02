using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling;

public class LeaderboardEntry
{
    public string ID { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }

    public LeaderboardEntry(string PlayerName, int score)
    {
        this.PlayerName = PlayerName;
        this.Score = score;
        ID = System.Guid.NewGuid().ToString();
    }
}

public class LeaderboardManager
{
    /// <summary>
    /// Gets the file path to the leaderboard
    /// </summary>
    /// <returns></returns>
    public static string GetLeaderboardPath()
    {
        return Path.Combine(Engine.currentProject.ProjectFolderPath, "leaderboard.cbl");
    }

    /// <summary>
    /// Adds a new score to leaderboard
    /// </summary>
    /// <param name="playerName">The players name</param>
    /// <param name="Score">Score they achieved</param>
    public static void AddScore(string playerName, int Score)
    {
        // Loads leaderboard
        List<LeaderboardEntry> board = LoadLeaderboard();
        
        // Adds entry
        board.Add(new LeaderboardEntry(playerName, Score));

        // Sorts leaderboard
        board = InsertionSortLeaderboard(board);
        
        // Overwrites file
        string json = JsonConvert.SerializeObject(board, Formatting.Indented);
        File.WriteAllText(GetLeaderboardPath(), json);
    }

    /// <summary>
    /// Removes score from leaderboard
    /// </summary>
    /// <param name="index">Index to remove</param>
    public static void RemoveScore(int index)
    {
        List<LeaderboardEntry> board = LoadLeaderboard();
        
        board = InsertionSortLeaderboard(board);
        
        board.RemoveAt(index);
        
        string json = JsonConvert.SerializeObject(board, Formatting.Indented);
        File.WriteAllText(GetLeaderboardPath(), json);
    }

    /// <summary>
    /// Loads leaderboard from leaderboard.cbl
    /// </summary>
    /// <returns>Leaderboard entry list</returns>
    public static List<LeaderboardEntry> LoadLeaderboard()
    {
        string path = GetLeaderboardPath();

        // Checks file exists
        if (!File.Exists(path)) return new List<LeaderboardEntry>();

        // Reads information and deserialises data.
        string json = File.ReadAllText(path);
        
        if (string.IsNullOrWhiteSpace(json)) return new List<LeaderboardEntry>();
        
        return JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
    }
    
    /// <summary>
    /// Sorts leaderboard using insertion sort
    /// </summary>
    /// <param name="entries">Unsorted Leaderboard</param>
    /// <returns>Sorted leaderboard</returns>
    private static List<LeaderboardEntry> InsertionSortLeaderboard(List<LeaderboardEntry> entries)
    {
        // Go through each leaderboard entry
        for (int i = 1; i < entries.Count; i++)
        {
            LeaderboardEntry currentEntry = entries[i];
            int j = i - 1;

            // Score behind has a higher score than the current one then switch the values
            while (j >= 0 && entries[j].Score < currentEntry.Score)
            {
                entries[j + 1] = entries[j];
                j--;
            }

            entries[j + 1] = currentEntry;
        }

        return entries;
    }
}