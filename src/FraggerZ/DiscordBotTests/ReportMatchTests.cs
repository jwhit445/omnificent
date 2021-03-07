using Core;
using Discord;
using DiscordBot.Caches;
using DiscordBot.Models;
using DiscordBot.Reactions;
using DiscordBot.Services;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTests {

    [TestClass]
    public class ReportMatchTests {

        Mock<IOptions<ChannelSettings>> _channelSettings;
        Mock<IDiscordUserCache> _discordUserCache;
        Mock<IOptions<EmoteSettings>> _emoteSettings;
        Mock<IMatchService> _matchService;
        Mock<ITeamService> _teamService;
        Mock<IUserService> _userService;
        Mock<IOptions<RoleSettings>> _roleSettings;

        [TestInitialize]
        public void Setup() {
            _discordUserCache = new Mock<IDiscordUserCache>();
            _matchService = new Mock<IMatchService>();
            _teamService = new Mock<ITeamService>();
            _userService = new Mock<IUserService>();

            _channelSettings = new Mock<IOptions<ChannelSettings>>();
            _channelSettings.SetupGet(x => x.Value).Returns(SettingsHelper.GetChannelSettings);
            _emoteSettings = new Mock<IOptions<EmoteSettings>>();
            _emoteSettings.SetupGet(x => x.Value).Returns(SettingsHelper.GetEmoteSettings);
            _roleSettings = new Mock<IOptions<RoleSettings>>();
            _roleSettings.SetupGet(x => x.Value).Returns(SettingsHelper.GetRoleSettings);
        }

        [TestMethod]
        public async Task Test_Report_Same_Match_Once_Success() {
            // Arrange
            int matchNumber = 111;
            var match = GetMatch(matchNumber, MatchStatus.Playing, -1);
            List<string> listFailures = new List<string>();
            EmbedBuilder builder = new EmbedBuilder() { Title = $"{ReportReactions.MATCH_LOG_PREFIX}{match.MatchNumber}", Description = "", Color = Color.Red };
            IEmbed matchEmbed = builder.Build();
            Mock<IMessage> messageMock = new Mock<IMessage>();
            messageMock.Setup(x => x.Embeds).Returns(new ReadOnlyCollection<IEmbed>(new List<IEmbed>() { matchEmbed }));
            Mock<ITextChannel> channelMock = new Mock<ITextChannel>();
            channelMock.Setup(x => x.Guild).Returns(GetGuild);
            Mock<IGuildUser> userMock = new Mock<IGuildUser>();
            userMock.Setup(x => x.Id).Returns(1);
            userMock.Setup(x => x.RoleIds).Returns(new ReadOnlyCollection<ulong>(new List<ulong>() { _roleSettings.Object.Value.ScoreConfirmRoleId }));
            channelMock.Setup(x => x.GetUserAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((ulong id, CacheMode mode, RequestOptions options) => {
                if(id != userMock.Object.Id) {
                    listFailures.Add("Incorrect user id given.");
                    throw new Exception();

                }
                return userMock.Object;
            });
            _matchService.Setup(x => x.GetByNumber(It.IsAny<int>())).ReturnsAsync((int matchNumber) => {
                if(matchNumber != match.MatchNumber) {
                    listFailures.Add("Incorrect match number given");
                    throw new Exception();
                }
                // Get a deep copy of the current match to emulate a local cached vs server updated version of the match.
                return GetMatch(matchNumber, match.MatchStatus, match.WinningTeam);
            });
            int matchReportCount = 0;
            bool isReporting = false;
            _matchService.Setup(x => x.ReportWin(It.IsAny<DiscordBot.Models.Match>())).Returns(async (DiscordBot.Models.Match matchToReport) => {
                if (match.MatchNumber != matchToReport.MatchNumber) {
                    listFailures.Add("Incorrect match number given");
                    throw new Exception();
                }
                if(isReporting) {
                    listFailures.Add("Multiple attempts were concurrently made to report the same match.");
                    throw new Exception();
                }
                isReporting = true;
                matchReportCount += 1;
                if(matchReportCount == 1) {
                    await Task.Delay(100);
                }
                match.WinningTeam = matchToReport.WinningTeam;
                match.MatchStatus = DiscordBot.Models.MatchStatus.Reported;
                isReporting = false;
            });
            string emoteName = _emoteSettings.Object.Value.OneEmoteName;
            ReportReactions reportReactions = new ReportReactions(_channelSettings.Object, _discordUserCache.Object, _emoteSettings.Object, _matchService.Object, _teamService.Object, _userService.Object, _roleSettings.Object);
            // Act
            List<Func<Task>> listTasks = new List<Func<Task>>();
            for(int i = 0; i < 10; i++) {
                listTasks.Add(async () => await reportReactions.ReportMatchForMessage(messageMock.Object, channelMock.Object, userMock.Object.Id, emoteName));
            }
            await TaskUtils.WhenAll(listTasks);
            // Assert
            if(listFailures.Count > 0) {
                Assert.Fail(string.Join("\n", listFailures));
            }
            _matchService.Verify(x => x.ReportWin(It.IsAny<DiscordBot.Models.Match>()), Times.Once);
            Assert.AreEqual(1, matchReportCount);
        }

        private DiscordBot.Models.Match GetMatch(int matchNumber, MatchStatus status, int winningTeam) {
            return new DiscordBot.Models.Match {
                MatchNumber = matchNumber,
                WinningTeam = winningTeam,
                MatchStatus = status,
            };
        }

        private IGuild GetGuild() {
            Mock<IGuild> guildMock = new Mock<IGuild>();
            guildMock.Setup(x => x.GetTextChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((CacheMode mode, RequestOptions options) => {
                return new ReadOnlyCollection<ITextChannel>(new List<ITextChannel>());
            });
            guildMock.Setup(x => x.GetVoiceChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((CacheMode mode, RequestOptions options) => {
                Mock<IVoiceChannel> openLobbyVCMock = new Mock<IVoiceChannel>();
                openLobbyVCMock.Setup(x => x.Name).Returns("Open Lobby 1");
                openLobbyVCMock.Setup(x => x.Id).Returns(_channelSettings.Object.Value.RoCoOpenLobbyChannelId);
                return new ReadOnlyCollection<IVoiceChannel>(new List<IVoiceChannel>() { openLobbyVCMock.Object });
            });
            return guildMock.Object;
        }
    }
}
