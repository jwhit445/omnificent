using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Discord;
using DiscordBot.Models;
using DiscordBot.Services;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DiscordBotTests {

	[TestClass]
    public class PugMatchTests {

        private const ulong _guildId = 1;
        List<IUser> _listAllDiscordUsers;
        List<User> _listAllUsers;
        Mock<IUserService> _userService;
        Mock<IEmbedService> _embedService;
        Mock<IMatchService> _matchService;
        Mock<IOptions<ChannelSettings>> _channelSettings;
        Mock<IOptions<EmoteSettings>> _emoteSettings;

        [TestInitialize]
        public void Setup() {
            _listAllDiscordUsers = new List<IUser>();
            _listAllUsers = new List<User>();

            _userService = new Mock<IUserService>();
            _userService.Setup(x => x.GetById(It.IsAny<ulong>())).ReturnsAsync((ulong id) => _listAllUsers.FirstOrDefault(x => x.DiscordId == id));
            _embedService = new Mock<IEmbedService>();
            _matchService = new Mock<IMatchService>();
            _matchService.Setup(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>())).Returns((string gameName, PlayerQueue queue) => {
                return Task.CompletedTask;
            });

            _channelSettings = new Mock<IOptions<ChannelSettings>>();
            _channelSettings.SetupGet(x => x.Value).Returns(SettingsHelper.GetChannelSettings);
            _emoteSettings = new Mock<IOptions<EmoteSettings>>();
            _emoteSettings.SetupGet(x => x.Value).Returns(SettingsHelper.GetEmoteSettings);
        }

        [TestMethod]
        public async Task Test_Users_Joined_Queue_Success() {
            // Arrange
            AddUsersToCache(10);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listTasks = new List<Func<Task>> {
                async () => await pugService.Join("NA", _listAllDiscordUsers[0], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[1], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[2], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[3], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[4], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[5], naMainQueueChannel),
                async () => await pugService.Join("NA", _listAllDiscordUsers[6], naMainQueueChannel),
            };
            await TaskUtils.WhenAll(listTasks);
            // Assert
            Assert.AreEqual(7, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        [TestMethod]
        public async Task Test_Single_Match_Created_Success() {
            // Arrange
            AddUsersToCache(10);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listTasks = new List<Func<Task>>();
            foreach(var user in _listAllDiscordUsers) {
                listTasks.Add(async () => await pugService.Join("NA", user, naMainQueueChannel));
            }
            await TaskUtils.WhenAll(listTasks);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(_listAllDiscordUsers.Count / 8));
            Assert.AreEqual(_listAllDiscordUsers.Count % 8, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        [TestMethod]
        public async Task Test_Multiple_Matches_Created_Success() {
            // Arrange
            AddUsersToCache(30);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listTasks = new List<Func<Task>>();
            foreach (var user in _listAllDiscordUsers) {
                listTasks.Add(async () => await pugService.Join("NA", user, naMainQueueChannel));
            }
            await TaskUtils.WhenAll(listTasks);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(_listAllDiscordUsers.Count / 8));
            Assert.AreEqual(_listAllDiscordUsers.Count % 8, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        [TestMethod]
        public async Task Test_Add_Duo_To_Queue_Success() {
            // Arrange
            AddUsersToCache(2);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listTasks = new List<Task>();
            (IUser p1, IUser p2) = (_listAllDiscordUsers[0], _listAllDiscordUsers[1]);
            await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
            await pugService.InviteDuoPartner(p1, p2);
            await pugService.FinalizeDuoPartners(p2.Id);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(0));
            Assert.AreEqual(2, pugService.NAMainQueue.PlayersInQueue.Count);
            Assert.AreEqual(1, pugService.NAMainQueue.DuoPlayers.Count);
        }

        [TestMethod]
        public async Task Test_Single_Match_With_Duo_Created_Success() {
            // Arrange
            AddUsersToCache(30);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listFuncTasks = new List<Func<Task>>();
            (IUser p1, IUser p2) = (_listAllDiscordUsers[0], _listAllDiscordUsers[1]);
            await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
            await pugService.InviteDuoPartner(p1, p2);
            await pugService.FinalizeDuoPartners(p2.Id);
            listFuncTasks.Add(async () => {
                await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
                await pugService.InviteDuoPartner(p1, p2);
                await pugService.FinalizeDuoPartners(p2.Id);
            });
            foreach (var user in _listAllDiscordUsers) {
                if(user.Id == p1.Id || user.Id == p2.Id) {
                    continue;
                }
                listFuncTasks.Add(async () => await pugService.Join("NA", user, naMainQueueChannel));
            }
            await TaskUtils.WhenAll(listFuncTasks);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(_listAllDiscordUsers.Count / 8));
            Assert.AreEqual(_listAllDiscordUsers.Count % 8, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        [TestMethod]
        public async Task Test_Multiple_Matches_With_Duos_Created_Success() {
            // Arrange
            AddUsersToCache(30);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listFuncTasks = new List<Func<Task>>();
            (IUser p1, IUser p2) = (_listAllDiscordUsers[0], _listAllDiscordUsers[1]);
            await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
            await pugService.InviteDuoPartner(p1, p2);
            await pugService.FinalizeDuoPartners(p2.Id);
            listFuncTasks.Add(async () => {
                await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
                await pugService.InviteDuoPartner(p1, p2);
                await pugService.FinalizeDuoPartners(p2.Id);
            });
            for(int i = 1;i <= _listAllDiscordUsers.Count; i++) {
                var user = _listAllDiscordUsers[i - 1];
                if (new List<int>(new int[] { 1, 4, 9, 11, 14, 17, 19, 21}).Contains(i)) {
                    var userDuo = _listAllDiscordUsers[i];
                    listFuncTasks.Add(async () => {
                        await pugService.StartDuoQueueAttempt(QueueType.NAMain, user);
                        await pugService.InviteDuoPartner(user, userDuo);
                        await pugService.FinalizeDuoPartners(userDuo.Id);
                    });
                    i++;
                    continue;
                }
                listFuncTasks.Add(async () => await pugService.Join("NA", user, naMainQueueChannel));
            }
            await TaskUtils.WhenAll(listFuncTasks);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(_listAllDiscordUsers.Count / 8));
            Assert.AreEqual(_listAllDiscordUsers.Count % 8, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        [TestMethod]
        public async Task Test_Duo_Joining_at_7_Disallowed_Success() {
            // Arrange
            AddUsersToCache(9);
            RoCoPugService pugService = await GetPugService();
            var naMainQueueChannel = GetTextChannel(_channelSettings.Object.Value.RoCoNAQueueChannelId);
            // Act
            var listFuncTasks = new List<Func<Task>>();
            (IUser p1, IUser p2) = (_listAllDiscordUsers[0], _listAllDiscordUsers[1]);
            foreach (var user in _listAllDiscordUsers) {
                if (user.Id == p1.Id || user.Id == p2.Id) {
                    continue;
                }
                listFuncTasks.Add(async () => await pugService.Join("NA", user, naMainQueueChannel));
            }
            listFuncTasks.Add(async () => {
                await pugService.StartDuoQueueAttempt(QueueType.NAMain, p1);
                await pugService.InviteDuoPartner(p1, p2);
                await pugService.FinalizeDuoPartners(p2.Id);
            });
            await TaskUtils.WhenAll(listFuncTasks);
            // Assert
            _matchService.Verify(x => x.GeneratePUG(It.IsAny<string>(), It.IsAny<PlayerQueue>()), Times.Exactly(0));
            Assert.AreEqual(7, pugService.NAMainQueue.PlayersInQueue.Count);
        }

        private async Task<RoCoPugService> GetPugService() {
            RoCoPugService pugService = new RoCoPugService(_userService.Object, _embedService.Object, _matchService.Object, _channelSettings.Object, _emoteSettings.Object, null);
            var discordClient = new Mock<IDiscordClient>();
            discordClient.Setup(x => x.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((ulong id, CacheMode mode, RequestOptions options) => {
                return GetGuild();
            });
            discordClient.Setup(x => x.GetChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((ulong channelId, CacheMode mode, RequestOptions options) => {
                return GetTextChannel(channelId);
            });
            await pugService.Init(discordClient.Object);
            return pugService;
        }

        private void AddUsersToCache(int numUsers) {
            for(int i = 1; i <= numUsers; i++) {
                CreateAndAddUserToCache((ulong)i);
            }
        }

        private void CreateAndAddUserToCache(ulong id) {
            var userDiscord = CreateDiscordUser(id);
            var user1 = new User($"User{id}", id);
            _listAllDiscordUsers.Add(userDiscord);
            _listAllUsers.Add(user1);
        }

        private IGuildUser CreateDiscordUser(ulong id) {
            var userDiscord = new Mock<IGuildUser>();
            userDiscord.Setup(x => x.Id).Returns(id);
            userDiscord.Setup(x => x.GetOrCreateDMChannelAsync(It.IsAny<RequestOptions>())).ReturnsAsync((RequestOptions options) => {
                var dmChannel = new Mock<IDMChannel>();
                dmChannel.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>())).ReturnsAsync((string text, bool isTTS, Embed embed, RequestOptions options) => {
                    return new Mock<IUserMessage>().Object;
                });
                return dmChannel.Object;
            });
            return userDiscord.Object;
        }

        private ITextChannel GetTextChannel(ulong id) {
            var channelMock = new Mock<ITextChannel>();
            channelMock.Setup(x => x.Id).Returns(id);
            channelMock.Setup(x => x.Name).Returns(id.ToString());
            channelMock.Setup(x => x.Guild).Returns(GetGuild);
            channelMock.Setup(x => x.GetUserAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync((ulong userId, CacheMode mode, RequestOptions options) => {
                return _listAllDiscordUsers.FirstOrDefault(x => x.Id == userId) as IGuildUser;
            });
            channelMock.Setup(x => x.GetMessagesAsync(It.IsNotNull<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(GetMessages);
            return channelMock.Object;
        }

        private IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessages() {
            IReadOnlyCollection<IMessage> messages = new ReadOnlyCollection<IMessage>(new List<IMessage> {
                GetMessage()
            });
            IList<IReadOnlyCollection<IMessage>> retVal = new List<IReadOnlyCollection<IMessage>>() {
                messages
            };
            var asyncEnumerable = retVal.ToAsyncEnumerable();
            return asyncEnumerable;
        }

        private IMessage GetMessage() {
            var messageMock = new Mock<IMessage>();
            return messageMock.Object;
        }

        private IGuild GetGuild() {
            var guildMock = new Mock<IGuild>();
            guildMock.Setup(x => x.Id).Returns(_guildId);
            return guildMock.Object;
        }
    }
}
