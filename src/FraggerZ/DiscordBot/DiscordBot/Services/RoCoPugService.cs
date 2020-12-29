﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordBot.Services {
    public class RoCoPugService {
        // To stop double queueing
        public List<SocketGuildUser> FrozenUsers { get; } = new List<SocketGuildUser>();
        public Dictionary<ulong, DateTime> dictUserQueueTimes { get; } = new Dictionary<ulong, DateTime>();

        // Ready system - caches socketguild users of matches who are NOT ready
        private List<Tuple<string, List<SocketGuildUser>>> UnreadyLists { get; set; }

        public Dictionary<IUser, (PlayerQueue queue, IUser player2)> DuoPartners
            = new Dictionary<IUser, (PlayerQueue queue, IUser player2)>();

        public Dictionary<IUser, (PlayerQueue queue, QueueType queueType)> DuoInviteStarted
            = new Dictionary<IUser, (PlayerQueue queue, QueueType queueType)>();

        public PlayerQueue NAQueue { get; set; }
        public PlayerQueue NACPlusQueue { get; set; }
        public PlayerQueue EUQueue { get; set; }

        private readonly MatchService _matchService;
        private readonly EmbedService _embedService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly UserService _userService;

        public RoCoPugService(UserService userService, EmbedService embedService, MatchService matchService, IOptions<ChannelSettings> channelSettings, IOptions<EmoteSettings> emoteSettings) {
            _matchService = matchService;
            _userService = userService;
            _embedService = embedService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;

            UnreadyLists = new List<Tuple<string, List<SocketGuildUser>>>();
        }

        public void Init(DiscordSocketClient client) {
            NAQueue = new PlayerQueue(client.GetChannel(_channelSettings.RoCoNAQueueChannelId) as SocketTextChannel);
            NACPlusQueue = new PlayerQueue(client.GetChannel(_channelSettings.RoCoNAQueueCPlusUpChannelId) as SocketTextChannel);
            EUQueue = new PlayerQueue(client.GetChannel(_channelSettings.RoCoEUQueueChannelId) as SocketTextChannel);
        }

        public int GetUnreadyUserTotal(Match match) {
            foreach (var tuple in UnreadyLists) {
                if (tuple.Item1 == match.Id) {
                    return tuple.Item2.Count;
                }
            }
            return -1;
        }

        /* COMMENTING OUT READY SYSTEM */
        /*

        // actually removes the user from the "UNREADY" list
        public void ReadyUpUser(Match match, SocketGuildUser user)
        {
            foreach(var tuple in UnreadyLists)
            {
                if(tuple.Item1 == match.Id)
                {
                    int removeIndex = -1;
                    for(int k = 0; k < tuple.Item2.Count; k++)
                    {
                        if(tuple.Item2[k].Id == user.Id)
                        {
                            removeIndex = k;
                            break;
                        }
                    }
                    if (removeIndex >= 0) tuple.Item2.RemoveAt(removeIndex);
                }
            }
        }

        public async Task RemoveUsersForAFK(List<SocketGuildUser> queue, SocketTextChannel channel)
        {
            List<SocketGuildUser> users = new List<SocketGuildUser>();
            foreach (SocketGuildUser userCurr in queue)
            {
                if (dictUserQueueTimes.ContainsKey(userCurr.Id) && (DateTime.UtcNow - dictUserQueueTimes[userCurr.Id]).TotalMinutes >= 15) 
                {
                    users.Add(userCurr);
                    await userCurr.SendMessageAsync("You have been removed from the queue after being in it for 15 minutes." +
                        " Please rejoin the queue if you want to play.");
                }
            }
            queue.RemoveAll(x => users.Contains(x));
        }

        public async Task StartReadySystemForMatch(Match match, SocketTextChannel anySocketTextChannel)
        {
            await _embedService.SendPlayersNeedReadyUpMessage(match, anySocketTextChannel);

            List<SocketGuildUser> allUsersInMatch = new List<SocketGuildUser>();
            foreach (ulong discordId in match.Team1DiscordIds)
            {
                allUsersInMatch.Add(anySocketTextChannel.GetUser(discordId));
            }
            foreach (ulong discordId in match.Team2DiscordIds)
            {
                allUsersInMatch.Add(anySocketTextChannel.GetUser(discordId));
            }
            UnreadyLists.Add(new Tuple<string, List<SocketGuildUser>>(match.Id, allUsersInMatch));

            void RemoveUnreadyList()
            {
                int removeIndex = -1;
                for (int i = 0; i < UnreadyLists.Count; i++)
                {
                    if (UnreadyLists[i].Item1 == match.Id)
                    {
                        removeIndex = i;
                        break;
                    }
                }
                if (removeIndex >= 0) UnreadyLists.RemoveAt(removeIndex);
            }

            async Task ApplyCooldown(SocketGuildUser user)
            {
                User dbUser = await _userService.GetById(user.Id);
                dbUser.SuspensionReturnDate = DateTime.UtcNow.AddMinutes(10);
                await _userService.Update(dbUser);
            }

            bool MatchIsReady()
            {
                foreach (var unReadyList in UnreadyLists)
                {
                    if (unReadyList.Item1 == match.Id)
                    {
                        if (unReadyList.Item2.Count <= 0) return true;
                        else return false;
                    }
                }
                return false;
            }

            async Task SendCancellationDMs()
            {
                foreach (SocketGuildUser user in allUsersInMatch)
                {
                    try
                    {
                        await user.SendMessageAsync($"Match #{match.MatchNumber} has been cancelled. " +
                            $"You were not ready in time and received a small cooldown to join the queue (10m).");
                        await ApplyCooldown(user);
                    }
                    catch
                    {
                    }
                }
            }

            async Task UpdateReadiedUsers()
            {
                List<ulong> ReadyUserIds = new List<ulong>();

                //BUILD THE READY USERIDS
                foreach(var tuple in UnreadyLists)
                {
                    if(tuple.Item1 == match.Id)
                    {
                        // add to the ready users if not in the unready list
                        foreach(SocketGuildUser sguCurr in allUsersInMatch)
                        {
                            bool found = false;
                            for(int k = 0; k < tuple.Item2.Count; k++)
                            {
                                if(sguCurr.Id == tuple.Item2[k].Id)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (found) ReadyUserIds.Add(sguCurr.Id);
                        }
                    }
                }

                // for all readied users -> DM Saying match was cancelled and try to join the queue again
                foreach(ulong discordId in ReadyUserIds)
                {
                    try
                    {
                        var usr = anySocketTextChannel.GetUser(discordId);
                        await  usr.SendMessageAsync($"Our apologies, but your match has been cancelled due to not all players being ready after 120s." +
                            $" (You were fine and we threw you back into the queue!) `:)`");
                        await Join(match.MatchRegion, usr, anySocketTextChannel);
                    }
                    catch
                    {

                    }
                }
            }

            async Task DeleteOldMatchChannels()
            {
                foreach(SocketTextChannel chanCurr in anySocketTextChannel.Guild.TextChannels)
                {
                    if (chanCurr.Name.Contains("match") && chanCurr.Name.Contains(match.MatchNumber.ToString()))
                    {
                        await chanCurr.DeleteAsync();
                        break;
                    }
                }
                foreach (SocketVoiceChannel chanCurr in anySocketTextChannel.Guild.VoiceChannels)
                {
                    if (chanCurr.Name.Contains("M") && chanCurr.Name.Contains(match.MatchNumber.ToString()))
                    {
                        await chanCurr.DeleteAsync();
                    }
                }
            }
            // add all users from match to the list

            // now we have all the unready users
            // set a timer for 60s at the end of the timer we check to see if the list of users.count <= 0
            var timerTask = Task.Factory.StartNew(() =>
            {
                Timer timer = new Timer();
                timer.Elapsed += ReadyTimerElapsed;
                timer.Enabled = true;
                timer.Start();

                async void ReadyTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
                {
                    if (!MatchIsReady())
                    {
                        // match not ready
                        // send cancellation DMs to users that they have been requeued.
                        await SendCancellationDMs();

                        // put all users who were ready into the queue
                        await UpdateReadiedUsers();

                        // Delete the old match channels
                        await DeleteOldMatchChannels();
                    }
                    else
                    {
                        // match is ready to be played
                        // Send confirmation embed to channel
                        await _embedService.SendAllPlayersReadyEmbed(match, anySocketTextChannel);
                    }
                    //Remove the cached list of unready users.
                    RemoveUnreadyList();
                    timer.Stop();
                    timer.Dispose();
                }
            });
        }
        */

        /// <summary>
        /// Join the NA or EU pug queue.
        /// </summary>
        /// <param name="region">NA or EU</param>
        public async Task Join(string region, SocketGuildUser socketGuildUser, SocketTextChannel socketTextChannel) {

            if (socketTextChannel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                await JoinToQueue(NACPlusQueue, socketGuildUser, socketTextChannel, "NA");
            }
            else if (region == "NA") {
                await JoinToQueue(NAQueue, socketGuildUser, socketTextChannel, "NA");
            }
            else if (region == "EU") {
                await JoinToQueue(EUQueue, socketGuildUser, socketTextChannel, "EU");
            }
        }

        async Task JoinToQueue(PlayerQueue queue, SocketGuildUser user, SocketTextChannel channel, string region) {
            foreach (SocketGuildUser userCurr in queue.PlayersInQueue) {
                if (userCurr.Id == user.Id) {
                    return;
                }
            }

            var messages = await channel.GetMessagesAsync(1).FlattenAsync();
            var message = messages.FirstOrDefault();
            if (message == null) {
                message = channel.GetCachedMessages(1).FirstOrDefault();
            }
            if (!await CanUserJoinQueue(user, message, channel.Id)) {
                return;
            }
            if (!dictUserQueueTimes.ContainsKey(user.Id)) {
                dictUserQueueTimes.Add(user.Id, DateTime.UtcNow);
            }
            dictUserQueueTimes[user.Id] = DateTime.UtcNow;
            queue.PlayersInQueue.Add(user);
            await TryStartMatch(queue, message, channel, region);
        }

        async Task JoinDuoToQueue(PlayerQueue queue, (IUser p1, IUser p2) duo, SocketTextChannel channel, string region) {
            foreach (SocketGuildUser userCurr in queue.PlayersInQueue) {
                if (userCurr.Id == duo.p1.Id || userCurr.Id == duo.p2.Id) {
                    return;
                }
            }

            var messages = await channel.GetMessagesAsync(1).FlattenAsync();
            var message = messages.FirstOrDefault();
            if (message == null) {
                message = channel.GetCachedMessages(1).FirstOrDefault();
            }
            if (!await CanUserJoinQueue(duo.p1, message, channel.Id) || !await CanUserJoinQueue(duo.p2, message, channel.Id)) {
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
            queue.PlayersInQueue.Add(duo.p1);
            queue.PlayersInQueue.Add(duo.p2);
            queue.DuoPlayers.Add(duo);
            await TryStartMatch(queue, message, channel, region);
        }

        private async Task TryStartMatch(PlayerQueue queue, IMessage queueMessage, SocketTextChannel channel, string region) {
            if (queue.PlayersInQueue.Count >= 8) {
                try {
                    await queueMessage.RemoveAllReactionsAsync();
                    await _embedService.UpdateQueueEmbed(new List<IUser>(), channel, true);
                    await _matchService.GeneratePUG("Rogue Company", channel, queue, region);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
                finally {
                    queue.PlayersInQueue.Clear();
                    await queueMessage.AddReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode));
                    await queueMessage.AddReactionAsync(new Emoji(_emoteSettings.PlayDuoEmoteUnicode));
                }
            }
            else {
                await _embedService.UpdateQueueEmbed(queue.PlayersInQueue, channel);
            }
        }

        private async Task<bool> CanUserJoinQueue(IUser user, IMessage queueMessage, ulong channelId) {
            if (await IsFrozenFromJoining(user, queueMessage)) {
                return false;
            }
            // check null user
            User dbUser = await _userService.GetById(user.Id);
            if (dbUser == null) {
                await user.SendMessageAsync($"Can't find your user in the database. Please go to #register and toggle the reaction and try again.");
                await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                return false;
            }
            if (dbUser.SuspensionReturnDate.Year > 2000) {
                if (dbUser.SuspensionReturnDate > DateTime.UtcNow) {
                    await user.SendMessageAsync($"Sorry, you are suspended from joining pugs.");
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
                    await user.SendMessageAsync($"Sorry, this lobby requires a rank of C+ or higher.");
                    await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> IsFrozenFromJoining(IUser user, IMessage queueMessage) {
            // check frozen
            foreach (var keyVal in dictUserQueueTimes) {
                if (keyVal.Key == user.Id && (DateTime.UtcNow - keyVal.Value).TotalSeconds < 15) {
                    await user.SendMessageAsync("You are frozen from joining the queue for 15 seconds. This is because your recently joined it.");
                    await queueMessage.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), user);
                    return true;
                }
            }
            return false;
        }

        public async Task StartDuoQueueAttempt(QueueType queueType, IUser user) {
            string queueName;
            switch (queueType) {
                case QueueType.NAMain:
                    DuoInviteStarted[user] = (NAQueue, queueType);
                    queueName = "NA - Main";
                    break;
                case QueueType.NACPlus:
                    DuoInviteStarted[user] = (NACPlusQueue, queueType);
                    queueName = "NA - C+ and up";
                    break;
                case QueueType.EUMain:
                    DuoInviteStarted[user] = (EUQueue, queueType);
                    queueName = "EU - Main";
                    break;
                default:
                    throw new ArgumentException($"Invalid queueType: {queueType}");
            }
            await user.SendMessageAsync(null, false, _embedService.StartDuoMessage(queueName));
        }

        public async Task InviteDuoPartner(QueueType queueType, IUser user) {
            string queueName = "";
            switch (queueType) {
                case QueueType.NAMain:
                    DuoPartners[user] = (NAQueue, null);
                    queueName = "NA - Main";
                    break;
                case QueueType.NACPlus:
                    DuoPartners[user] = (NACPlusQueue, null);
                    queueName = "NA - C+ and up";
                    break;
                case QueueType.EUMain:
                    DuoPartners[user] = (EUQueue, null);
                    queueName = "EU - Main";
                    break;
                default:
                    throw new ArgumentException($"Invalid queueType: {queueType}");
            }
            var message = await user.SendMessageAsync(null, false, _embedService.InviteDuo(queueName, user));
            await message.AddReactionsAsync(new IEmote[] { new Emoji(_emoteSettings.CheckEmoteUnicode), new Emoji(_emoteSettings.XEmoteUnicode) });
        }

        public async Task FinalizeDuoPartners(PlayerQueue queue, IUser player1, IUser player2) {
            //TODO: create a rocoPugService.JoinDuo method that code shares all of the if queue popped code, and handles the 9/8 potential queue size situation
            //queue.PlayersInQueue.AddRange(new List<IUser>() { player1, player2 });
            //queue.DuoPlayers.Add((player1, player2));

            await player1.SendMessageAsync(embed: _embedService.DuoQueueJoined(queue.Channel.Name, player2));
            await player2.SendMessageAsync(embed: _embedService.DuoQueueJoined(queue.Channel.Name, player1));
        }



        /// <summary>
        /// Leave the NA or EU pug queue.
        /// </summary>
        /// <param name="region">NA or EU</param>
        /// <param name="socketGuildUser"></param>
        /// <param name="socketTextChannel"></param>
        public async Task Leave(string region, SocketGuildUser socketGuildUser, SocketTextChannel socketTextChannel) {
            async Task LeaveFromQueue(PlayerQueue queue, SocketGuildUser user, SocketTextChannel channel) {
                for (int i = 0; i < queue.PlayersInQueue.Count; i++) {
                    if (queue.PlayersInQueue[i].Id == user.Id) {
                        queue.PlayersInQueue.RemoveAt(i);

                        await _embedService.UpdateQueueEmbed(queue.PlayersInQueue, channel);
                        return;
                    }
                }
            }

            if (socketTextChannel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId) {
                await LeaveFromQueue(NACPlusQueue, socketGuildUser, socketTextChannel);
            }
            if (region == "NA") {
                await LeaveFromQueue(NAQueue, socketGuildUser, socketTextChannel);
            }
            else if (region == "EU") {
                await LeaveFromQueue(EUQueue, socketGuildUser, socketTextChannel);
            }
        }

    }

    public class PlayerQueue {
        public SocketTextChannel Channel { get; }
        public List<IUser> PlayersInQueue { get; } = new List<IUser>();
        public List<(IUser, IUser)> DuoPlayers { get; } = new List<(IUser, IUser)>();

        public PlayerQueue(SocketTextChannel channel) => Channel = channel;
    }

    public enum QueueType {
        NAMain,
        NACPlus,
        EUMain,
    }
}
