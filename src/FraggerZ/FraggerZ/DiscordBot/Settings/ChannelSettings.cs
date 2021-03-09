namespace DiscordBot.Settings
{
    public class ChannelSettings
    {
        //Intro
        public ulong WelcomeChannelId { get; set; }
        public ulong RegisterChannelId { get; set; }

        //Role Assignment
        public ulong RoleAssignmentCategoryId { get; set; }
        public ulong GameChannelId { get; set; }
        public ulong OtherChannelId { get; set; }

        //Game Specific
        public ulong RoCoCategoryId { get; set; }
        public ulong RoCoNAQueueChannelId { get; set; }
        public ulong RoCoNAQueueCPlusUpChannelId { get; set; }
        public ulong RoCoEUQueueChannelId { get; set; }
        public ulong RoCoMatchLogsChannelId { get; set; }
        public ulong RoCoStatsChannelId { get; set; }
        public ulong RoCoOpenLobbyChannelId { get; set; }
        public ulong RoCoDuoInviteChannelId { get; set; }

        //Team Specific
        public ulong RoCoTeamChannelId { get; set; }
    }
}
