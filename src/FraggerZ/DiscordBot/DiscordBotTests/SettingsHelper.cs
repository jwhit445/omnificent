using DiscordBot.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotTests {
    public class SettingsHelper {

        public static ChannelSettings GetChannelSettings() {
            return new ChannelSettings {
                GameChannelId = 1,
                OtherChannelId = 2,
                RegisterChannelId = 3,
                RoCoCategoryId = 4,
                RoCoDuoInviteChannelId = 5,
                RoCoEUQueueChannelId = 6,
                RoCoMatchLogsChannelId = 7,
                RoCoNAQueueChannelId = 8,
                RoCoNAQueueCPlusUpChannelId = 9,
                RoCoOpenLobbyChannelId = 10,
                RoCoStatsChannelId = 11,
                RoCoTeamChannelId = 12,
                RoleAssignmentCategoryId = 13,
                WelcomeChannelId = 14,
            };
        }

        public static EmoteSettings GetEmoteSettings() {
            return new EmoteSettings {
                CheckEmoteName = "CheckEmoteName",
                CheckEmoteUnicode = "CheckEmoteUnicode",
                CrossFireGameEmoteName = "CrossFireGameEmoteName",
                EUEmoteName = "EUEmoteName",
                FireEmoteName = "FireEmoteName",
                FireEmoteUnicode = "FireEmoteUnicode",
                FraggerZEmoteName = "FraggerZEmoteName",
                FraggerZEmoteUnicode = "FraggerZEmoteUnicode",
                GamerEmoteName = "GamerEmoteName",
                GamerEmoteUnicode = "GamerEmoteUnicode",
                IronSightGameEmoteName = "IronSightGameEmoteName",
                NAEmoteName = "NAEmoteName",
                OneEmoteName = "OneEmoteName",
                OneEmoteUnicode = "OneEmoteUnicode",
                PlayDuoEmoteName = "PlayDuoEmoteName",
                PlayDuoEmoteUnicode = "PlayDuoEmoteUnicode",
                PlayEmoteName = "PlayEmoteName",
                PlayEmoteUnicode = "PlayEmoteUnicode",
                PreMatchEmoteName = "PreMatchEmoteName",
                PreMatchEmoteUnicode = "PreMatchEmoteUnicode",
                PUGPingerEmoteName = "PUGPingerEmoteName",
                RoCoGameEmoteName = "RoCoGameEmoteName",
                SpeakerEmoteUnicode = "SpeakerEmoteUnicode",
                TrophyEmoteUnicode = "TrophyEmoteUnicode",
                TwoEmoteName = "TwoEmoteName",
                TwoEmoteUnicode = "TwoEmoteUnicode",
                XEmoteName = "XEmoteName",
                XEmoteUnicode = "XEmoteUnicode"
            };
        }

        public static RoleSettings GetRoleSettings() {
            return new RoleSettings {
                AcceptedRulesRoleId = 1,
                CrossFireRoleId = 2,
                EURoleId = 3,
                EveryoneRoleId = 4,
                IronSightRoleId = 5,
                NARoleId = 6,
                PremiumRoleId = 7,
                PUGPingerRoleId = 8,
                RoCoRoleId = 9,
                ScoreConfirmRoleId = 10,
            };
        }
    }
}
