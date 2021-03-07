using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class Match
    {
        [JsonIgnore]
        public ulong Captain1DiscordId => ulong.Parse(Captain1Id);
        [JsonIgnore]
        public ulong Captain2DiscordId => ulong.Parse(Captain2Id);
        [JsonIgnore]
        public List<ulong> PlayerDiscordIdsPool => ConvertStringListToUlong(PlayerIdsPool);
        [JsonIgnore]
        public List<ulong> Team1DiscordIds => ConvertStringListToUlong(Team1Ids);
        [JsonIgnore]
        public List<ulong> Team2DiscordIds => ConvertStringListToUlong(Team2Ids);
        [JsonIgnore]
        public ulong PickingCaptainDiscordId => ulong.Parse(PickingCaptainId);

        public string Id { get; set; }
        public int MatchNumber { get; set; }
        public string PickingCaptainId { get; set; }
        public string Captain1Id { get; set; }
        public string Captain2Id { get; set; }
        public DateTime DateTimeStarted { get; set; }
        public DateTime DateTimeEnded { get; set; }
        public string MapName { get; set; }
        public string GameName { get; set; }
        public string MatchRegion { get; set; }
        public string MapImageURL { get; set; }
        public MatchStatus MatchStatus { get; set; }
        public MatchType MatchType { get; set; }
        public int WinningTeam { get; set; }
        public List<string> PlayerIdsPool { get; set; }
        public List<string> Team1Ids { get; set; }
        public List<string> Team2Ids { get; set; }
        public List<string> ReadyUserIds { get; set; }

        public Match()
        {
            PlayerIdsPool = new List<string>();
            Team1Ids = new List<string>();
            Team2Ids = new List<string>();
            ReadyUserIds = new List<string>();
            MatchStatus = MatchStatus.Picking;
            WinningTeam = -1;
        }

        public List<ulong> ConvertStringListToUlong(List<string> list)
        {
            List<ulong> returnVal = new List<ulong>();
            foreach (string id in list)
            {
                returnVal.Add(ulong.Parse(id));
            }
            return returnVal;
        }
    }

    public enum MatchType
    {
        Unknown,
        PUGCaptains,
        Scrim,
        PUGAuto,
    }

    public enum MatchStatus
    {
        Unknown,
        Cancelled,
        Reported,
        Reversed,
        Picking,
        Playing
    }
}
