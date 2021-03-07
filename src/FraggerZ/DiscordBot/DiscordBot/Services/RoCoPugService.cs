using Core;
using Core.Async;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Caches;
using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordBot.Services {
    public class RoCoPugService : IRoCoPugService {
        public Dictionary<ulong, DateTime> dictUserQueueTimes { get; } = new Dictionary<ulong, DateTime>();

        public Dictionary<ulong, (PlayerQueue queue, ulong player2)> DuoPartners { get; }
            = new Dictionary<ulong, (PlayerQueue queue, ulong player2)>();

        public Dictionary<ulong, (PlayerQueue queue, QueueType queueType)> DuoInviteStarted { get; }
            = new Dictionary<ulong, (PlayerQueue queue, QueueType queueType)>();

        public PlayerQueue NAMainQueue { get; set; }
        public PlayerQueue NACPlusQueue { get; set; }
        public PlayerQueue EUQueue { get; set; }
        private AsyncLock _lock = new AsyncLock();

        public Dictionary<ulong, PlayerQueue> DictQueueForChannel { get; set; } = new Dictionary<ulong, PlayerQueue>();

        private readonly IMatchService _matchService;
        private readonly IEmbedService _embedService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly IChannelCache _channelCache;
        private readonly IUserService _userService;

        public RoCoPugService(IUserService userService, IEmbedService embedService, IMatchService matchService, IOptions<ChannelSettings> channelSettings, IOptions<EmoteSettings> emoteSettings,
            IChannelCache channelCache) {
            _matchService = matchService;
            _userService = userService;
            _embedService = embedService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _channelCache = channelCache;
        }

        public async Task Init(IDiscordClient client) {
            var naMainQueueChannel = await client.GetChannelAsync(_channelSettings.RoCoNAQueueChannelId) as ITextChannel;
            var naCPlusQueueChannel = await client.GetChannelAsync(_channelSettings.RoCoNAQueueCPlusUpChannelId) as ITextChannel;
            var euQueueChannel = await client.GetChannelAsync(_channelSettings.RoCoEUQueueChannelId) as ITextChannel;
            var guild = await client.GetGuildAsync(naMainQueueChannel.GuildId);
            NAMainQueue = new PlayerQueue(guild, naMainQueueChannel, "NA", QueueType.NAMain);
            NACPlusQueue = new PlayerQueue(guild, naCPlusQueueChannel, "NA", QueueType.NACPlus);
            EUQueue = new PlayerQueue(guild, euQueueChannel, "EU", QueueType.EUMain);
            DictQueueForChannel.Add(_channelSettings.RoCoNAQueueChannelId, NAMainQueue);
            DictQueueForChannel.Add(_channelSettings.RoCoNAQueueCPlusUpChannelId, NACPlusQueue);
            DictQueueForChannel.Add(_channelSettings.RoCoEUQueueChannelId, EUQueue);
            await _channelCache.InitCache(client);
        }

        /// <summary>
        /// Join the NA or EU pug queue.
        /// </summary>
        /// <param name="region">NA or EU</param>
        public async Task Join(string region, IUser user, ITextChannel textChannel) {
            if(user == null) {
                return;
            }
            if (textChannel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                await JoinToQueue(NACPlusQueue, user, NACPlusQueue.Channel, "NA");
            }
            else if (region == "NA") {
                await JoinToQueue(NAMainQueue, user, NAMainQueue.Channel, "NA");
            }
            else if (region == "EU") {
                await JoinToQueue(EUQueue, user, EUQueue.Channel, "EU");
            }
        }

        private async Task JoinToQueue(PlayerQueue queue, IUser user, ITextChannel channel, string region) {
            foreach (IUser userCurr in queue.PlayersInQueue) {
                if (userCurr.Id == user.Id) {
                    return;
                }
            }
            try {
                var messages = channel.GetMessagesAsync(1);
                var flattened = await messages.FlattenAsync();
                var message = flattened.FirstOrDefault();
                if (message == null) {
                    return;
                }
                if (!await CanUserJoinQueue(user, message, channel.Id)) {
                    return;
                }
                if (!dictUserQueueTimes.ContainsKey(user.Id)) {
                    dictUserQueueTimes.Add(user.Id, DateTime.UtcNow);
                }
                dictUserQueueTimes[user.Id] = DateTime.UtcNow;
                PlayerQueue poppedQueue = null;
                await _lock.LockAsync(async () => {
                    queue.PlayersInQueue.Add(user);
                    if(queue.PlayersInQueue.Count >= 8) {
                        poppedQueue = PlayerQueue.Copy(queue);
                        queue.Clear();
                        await _embedService.UpdateQueueEmbed(queue, channel);
                        await message.RemoveAllReactionsAsync();
                        await message.AddReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode));
                        if (queue.QueueType == QueueType.NAMain) {
                            await message.AddReactionAsync(new Emoji(_emoteSettings.PlayDuoEmoteUnicode));
                        }
                    }
                });
                if(poppedQueue != null) {
                    await TryStartMatch(poppedQueue);
                }
                else {
                    await _embedService.UpdateQueueEmbed(queue, channel);
                }
            }
            catch (Exception ex) {
                var msg = ex.Message;
                return;
            }
        }

        async Task JoinDuoToQueue(PlayerQueue queue, (IUser p1, IUser p2) duo) {
            try {
                foreach (IUser userCurr in queue.PlayersInQueue) {
                    if (userCurr.Id == duo.p1.Id || userCurr.Id == duo.p2.Id) {
                        return;
                    }
                }
            }
            catch (Exception ex) {
                var msg = ex.Message;
                return;
            }
            try {
                var messages = await queue.Channel.GetMessagesAsync(1).FlattenAsync();
                var message = messages.FirstOrDefault();
                if (message == null) {
                    return;
                }
                if (!await CanUserJoinQueue(duo.p1, message, queue.Channel.Id) || !await CanUserJoinQueue(duo.p2, message, queue.Channel.Id)) {
                    return;
                }
                if (!dictUserQueueTimes.ContainsKey(duo.p1.Id)) {
                    dictUserQueueTimes.Add(duo.p1.Id, DateTime.UtcNow);
                }
                if (!dictUserQueueTimes.ContainsKey(duo.p2.Id)) {
                    dictUserQueueTimes.Add(duo.p2.Id, DateTime.UtcNow);
                }
                dictUserQueueTimes[duo.p1.Id] = DateTime.UtcNow;
                dictUserQueueTimes[duo.p2.Id] = DateTime.UtcNow;

                PlayerQueue poppedQueue = null;
                await _lock.LockAsync(async () => {
                    queue.PlayersInQueue.Add(duo.p1);
                    queue.PlayersInQueue.Add(duo.p2);
                    queue.DuoPlayers.Add(duo);
                    if (queue.PlayersInQueue.Count >= 8) {
                        poppedQueue = PlayerQueue.Copy(queue);
                        queue.Clear();
                        await _embedService.UpdateQueueEmbed(queue, queue.Channel);
                        await message.AddReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode));
                        if (queue.QueueType == QueueType.NAMain) {
                            await message.AddReactionAsync(new Emoji(_emoteSettings.PlayDuoEmoteUnicode));
                        }
                    }
                });
                if (poppedQueue != null) {
                    await message.RemoveAllReactionsAsync();
                    await TryStartMatch(poppedQueue);
                }
                else {
                    await _embedService.UpdateQueueEmbed(queue, queue.Channel);
                }
            }
            catch (Exception ex) {
                var msg = ex.Message;
                return;
            }
        }

        private async Task TryStartMatch(PlayerQueue queue) {
            try {
                await _matchService.GeneratePUG("Rogue Company", queue);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private async Task<bool> CanUserJoinQueue(IUser user, IMessage queueMessage, ulong channelId) {
            if(user == null) {
                return false;
            }
            if (await IsFrozenFromJoining(user, queueMessage)) {
                return false;
            }
            // check null user
            User dbUser = await _userService.GetById(user.Id);
            if (dbUser == null) {
                await _userService.Register(user);
                dbUser = await _userService.GetById(user.Id);
            }
            if (dbUser == null) {
                await SendDirectMessageAsync(user, text: $"Can't find your user in the database. Please go to #register and toggle the reaction and try again.");
                await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                return false;
            }
            if (dbUser.SuspensionReturnDate.Year > 2000) {
                if (dbUser.SuspensionReturnDate > DateTime.UtcNow) {
                    await SendDirectMessageAsync(user, text: $"Sorry, you are suspended from joining pugs.");
                    await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                    return false;
                }
                else {
                    dbUser.SuspensionReturnDate = DateTime.MinValue;
                    await _userService.Update(dbUser);
                }
            }
            if (channelId == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                if (dbUser.RoCoMMR * 100 < 2000 || dbUser.PlacementMatchIds.Count < 10) // less than C+ or unranked
                {
                    await SendDirectMessageAsync(user, text: $"Sorry, this lobby requires a rank of C+ or higher.");
                    await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> IsFrozenFromJoining(IUser user, IMessage queueMessage) {
            // check frozen
            foreach (var keyVal in dictUserQueueTimes) {
                if (keyVal.Key == user.Id && (DateTime.UtcNow - keyVal.Value).TotalSeconds < 3) {
                    await SendDirectMessageAsync(user, text: "You are frozen from joining the queue for 3 seconds. This is because your recently joined it.");
                    await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                    return true;
                }
            }
            return false;
        }

        public async Task StartDuoQueueAttempt(QueueType queueType, IUser user) {
            string queueName = "";
            await _lock.Lock(() => {
                switch (queueType) {
                    case QueueType.NAMain:
                        DuoInviteStarted[user.Id] = (NAMainQueue, queueType);
                        queueName = "NA - Main";
                        break;
                    case QueueType.NACPlus:
                        DuoInviteStarted[user.Id] = (NACPlusQueue, queueType);
                        queueName = "NA - C+ and up";
                        break;
                    case QueueType.EUMain:
                        DuoInviteStarted[user.Id] = (EUQueue, queueType);
                        queueName = "EU - Main";
                        break;
                    default:
                        throw new ArgumentException($"Invalid queueType: {queueType}");
                }
            });
            await SendDirectMessageAsync(user, embed: _embedService.StartDuoMessage(queueName));
        }

        public async Task InviteDuoPartner(IUser requestingUser, IUser invitedUser) {
            await _lock.Lock(() => DuoPartners[invitedUser.Id] = (DuoInviteStarted[requestingUser.Id].queue, requestingUser.Id));
            var message = await SendDirectMessageAsync(invitedUser, embed: _embedService.InviteDuo(DuoInviteStarted[requestingUser.Id].queueType.ToString(), requestingUser));
            await message.AddReactionsAsync(new IEmote[] { new Emoji(_emoteSettings.CheckEmoteUnicode), new Emoji(_emoteSettings.XEmoteUnicode) });
        }

        private async Task<IUserMessage> SendDirectMessageAsync(IUser user, string text = null, Embed embed = null) {
            return await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(text: text, embed: embed);
        }

        public async Task FinalizeDuoPartners(ulong player1) {
            var queue = DuoPartners[player1].queue;
            ulong player2 = DuoPartners[player1].player2;
            IUser p1 = await queue.Channel.GetUserAsync(player1);
            IUser p2 = await queue.Channel.GetUserAsync(player2);
            if(queue.PlayersInQueue.Count >= 7) {
                //Can't fit both players into queue. Block them.
                string errorMsg = "Unable to join queue as a duo, there was no enough room for both players.";
                await SendDirectMessageAsync(p1, text: errorMsg);
                await SendDirectMessageAsync(p2, text: errorMsg);
                DuoInviteStarted.Remove(DuoPartners[player1].player2);
                DuoPartners.Remove(player1);
                return;
            }
            await JoinDuoToQueue(queue, (p1, p2));
            await SendDirectMessageAsync(p1, embed: _embedService.DuoQueueJoined(queue.Channel.Name, p2));
            await SendDirectMessageAsync(p2, embed: _embedService.DuoQueueJoined(queue.Channel.Name, p1));
        }

        /// <summary>
        /// Leave the NA or EU pug queue.
        /// </summary>
        /// <param name="region">NA or EU</param>
        /// <param name="user"></param>
        /// <param name="socketTextChannel"></param>
        public async Task Leave(string region, IUser user, ITextChannel socketTextChannel) {
            if (user == null) {
                return;
            }

            if (socketTextChannel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                await LeaveFromQueue(NACPlusQueue, user, socketTextChannel);
            }
            if (region == "NA") {
                await LeaveFromQueue(NAMainQueue, user, socketTextChannel);
            }
            else if (region == "EU") {
                await LeaveFromQueue(EUQueue, user, socketTextChannel);
            }
        }

        public async Task LeaveDuo(string region, IUser user, ITextChannel socketTextChannel) {

            if (socketTextChannel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                await LeaveFromQueue(NACPlusQueue, user, socketTextChannel);
            }
            if (region == "NA") {
                await LeaveFromQueue(NAMainQueue, user, socketTextChannel);
            }
            else if (region == "EU") {
                await LeaveFromQueue(EUQueue, user, socketTextChannel);
            }
        }

        private async Task LeaveFromQueue(PlayerQueue queue, IUser user, ITextChannel channel) {
            if (user == null) {
                return;
            }
            await _lock.Lock(() => {
                (IUser, IUser)? duo = null;
                for (int i = 0; i < queue.DuoPlayers.Count; i++) {
                    if (queue.DuoPlayers[i].Item1.Id == user.Id || queue.DuoPlayers[i].Item2.Id == user.Id) {
                        duo = queue.DuoPlayers[i];
                        queue.DuoPlayers.RemoveAt(i);
                    }
                }
                for (int i = 0; i < queue.PlayersInQueue.Count; i++) {
                    if (queue.PlayersInQueue[i].Id == user.Id) {
                        queue.PlayersInQueue.RemoveAt(i);
                    }
                    else if (duo.HasValue && (queue.PlayersInQueue[i].Id == duo.Value.Item1.Id || queue.PlayersInQueue[i].Id == duo.Value.Item2.Id)) {
                        queue.PlayersInQueue.RemoveAt(i);
                    }
                }
            });
            await _embedService.UpdateQueueEmbed(queue, channel);
        }

    }

    public class PlayerQueue {
        public ITextChannel Channel { get; }
        public IGuild Guild { get; }
        public QueueType QueueType { get; }
        public string Region { get; }
        public List<IUser> PlayersInQueue { get; private set; } = new List<IUser>();
        public List<(IUser, IUser)> DuoPlayers { get; private set; } = new List<(IUser, IUser)>();

        public PlayerQueue(IGuild guild, ITextChannel channel, string region, QueueType type) => (Guild, Channel, Region, QueueType) = (guild, channel, region, type);

        public void Clear() {
            PlayersInQueue.Clear();
            DuoPlayers.Clear();
        }

        public IUser GetUser(ulong userId) {
            return PlayersInQueue.FirstOrDefault(x => x.Id == userId);
        }

        public static PlayerQueue Copy(PlayerQueue queue) {
            return new PlayerQueue(queue.Guild, queue.Channel, queue.Region, queue.QueueType) {
                PlayersInQueue = new List<IUser>(queue.PlayersInQueue),
                DuoPlayers = new List<(IUser, IUser)>(queue.DuoPlayers),
            };
        }
    }

    public enum QueueType {
        NAMain,
        NACPlus,
        EUMain,
    }
}
