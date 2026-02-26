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
    public static string GetLeaderboardPath()
    {
        return Path.Combine(Engine.currentProject.ProjectFolderPath, "leaderboard.cbl");
    }

    public static void AddScore(string playerName, int Score)
    {
        List<LeaderboardEntry> board = LoadLeaderboard();
        
        board.Add(new LeaderboardEntry(playerName, Score));

        board = InsertionSortLeaderboard(board);
        
        string json = JsonConvert.SerializeObject(board, Formatting.Indented);
        File.WriteAllText(GetLeaderboardPath(), json);
    }

    public static void RemoveScore(int index)
    {
        List<LeaderboardEntry> board = LoadLeaderboard();
        
        board = InsertionSortLeaderboard(board);
        
        board.RemoveAt(index);
        
        string json = JsonConvert.SerializeObject(board, Formatting.Indented);
        File.WriteAllText(GetLeaderboardPath(), json);
    }

    public static List<LeaderboardEntry> LoadLeaderboard()
    {
        string path = GetLeaderboardPath();

        if (!File.Exists(path)) return new List<LeaderboardEntry>();

        string json = File.ReadAllText(path);
        
        if (string.IsNullOrWhiteSpace(json)) return new List<LeaderboardEntry>();
        
        return JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
    }
    
    private static List<LeaderboardEntry> InsertionSortLeaderboard(List<LeaderboardEntry> entries)
    {
        for (int i = 1; i < entries.Count; i++)
        {
            LeaderboardEntry currentEntry = entries[i];
            int j = i - 1;

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