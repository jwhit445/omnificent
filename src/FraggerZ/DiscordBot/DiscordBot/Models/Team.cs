using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class Team
    {
        [JsonIgnore]
        public List<ulong> MemberDiscordIds => ConvertStringListToUlong(MemberIds);
        [JsonIgnore]
        public ulong CaptainDiscordId => ulong.Parse(CaptainId);

        public string CaptainId { get; set; }
        public string TeamName { get; set; }
        public string Id => TeamName;
        public string GameName { get; set; }
        public List<string> MatchStatIds { get; set; }
        public List<string> MemberIds { get; set; }

        public Team() { }
        public Team(string name, string gameName)
        {
            TeamName = name;
            GameName = gameName;
            MemberIds = new List<string>();
            MatchStatIds = new List<string>();
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
}
