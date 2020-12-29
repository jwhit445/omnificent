using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DiscordBot.Models
{
    public class User
    {
        [JsonIgnore]
        public ulong DiscordId => ulong.Parse(Id);

        public string Id { get; set; }
        public string Username { get; set; }
        public double RoCoMMR { get; set; }
        public double CrossFireMMR { get; set; }
        public double IronSightMMR { get; set; }
        public double RoCoSigma { get; set; }
        public DateTime SuspensionReturnDate { get; set; }
        public List<string> MatchStatIds { get; set; }
        public List<string> TeamIds { get; set; }
        public string StreamURL { get; set; }
        public string IGN { get; set; }
        public List<string> PlacementMatchIds { get; set; }
        public List<string> WinStreakMatchIds { get; set; }
        public DateTime DateTimeLastMapVote { get; set; }
        public DateTime DateTimeLastScrambleVote { get; set; }
        public DateTime DateTimeLastStatReset { get; set; }

        public User(string username, ulong discordId)
        {
            Id = discordId.ToString();
            Username = username;
            MatchStatIds = new List<string>();
            TeamIds = new List<string>();
            PlacementMatchIds = new List<string>();
            WinStreakMatchIds = new List<string>();
            RoCoMMR = 1500;
            CrossFireMMR = 1500;
            IronSightMMR = 1500;
        }
    }
}
