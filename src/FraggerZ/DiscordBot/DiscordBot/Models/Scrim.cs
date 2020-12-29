using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Models
{
    public class Scrim
    {
        public string Id { get; set; }
        public ulong ChallengeMessageId { get; set; }
        public string Team1Id { get; set; }
        public string Team2Id { get; set; }
        public ulong TeamsChannelId { get; set; }
        public ScrimStatus Status { get; set; }
    }

    public enum ScrimStatus
    {
        Open,
        Active,
        Closed
    }
}
