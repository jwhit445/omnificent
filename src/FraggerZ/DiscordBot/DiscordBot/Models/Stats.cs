using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class StatSummary
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double RoCoMMR { get; set; }
        public int RankPosition { get; set; }
    }

    /// <summary>
    /// Pulled from API. Not saved to Database.
    /// </summary> 
    public class Stats
    {
        public string GameName { get; set; }

        // If the map stat is for a user
        public ulong UserId { get; set; }
        // If the map stat is for a team
        public string TeamId { get; set; }
        

        public List<MapStat> MapStats { get; set; }

        public Stats()
        {
        }
    }

    // Used inside Stats class
    public class MapStat
    {
        public string MapName { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public long Damage { get; set; }
        public int Assists { get; set; }
    }
}
