using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Services;
using Discord.WebSocket;
using System;
using DiscordBot.Models;
using Discord;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using DiscordBot.Settings;
using DiscordBot.Util;
using System.Linq;
using Core.Async;
using DiscordBot.Caches;

public class MatchModule : ModuleBase<SocketCommandContext> {
    private IMatchService _matchService { get; }
    private IEmbedService _embedService { get; }
    private IUserService _userService { get; }
    private ChannelSettings _channelSettings { get; }
    private RoleSettings _roleSettings { get; }
    private IRoCoPugService _rocoService { get; }
    private EmoteSettings _emoteSettings { get; }
    private IDiscordUserCache _discordUserCache { get; }

    private AsyncLock _lock = new AsyncLock();

    public MatchModule(IUserService userService, IMatchService matchService, IEmbedService embedService, IRoCoPugService rocoService,
        IOptions<ChannelSettings> channelSettings, IOptions<RoleSettings> roleSettings, IOptions<EmoteSettings> emoteSettings,
        IDiscordUserCache discordUserCache)
    {
        _userService = userService;
        _matchService = matchService;
        _channelSettings = channelSettings.Value;
        _embedService = embedService;
        _roleSettings = roleSettings.Value;
        _rocoService = rocoService;
        _emoteSettings = emoteSettings.Value;
        _discordUserCache = discordUserCache;
    }

    [Command("votescramble")]
    public async Task VoteScrambleTeams() {
        if (!Context.Channel.Name.Contains("match")) {
            await Context.Message.DeleteAsync();
            var mess = await Context.Channel.SendMessageAsync("You may only vote to scramble teams inside match text channels.");
            await mess.DeleteAfter(10000);
            return;
        }

        bool foundRole = false;
        if (Context.User is SocketGuildUser sgUser) {
            foundRole = _userService.IsUserPremium(sgUser);
        }

        if (!foundRole) { await Context.Channel.SendMessageAsync($"You do not have the Premium role."); return; }

        User user = await _userService.GetById(Context.User.Id);
        if (user != null) {
            // check last scrambled date
            if ((DateTime.Now - user.DateTimeLastScrambleVote).TotalDays < 7) {
                await Context.Channel.SendMessageAsync("You may only vote to scramble teams once per week.");
                return;
            }
        }
        else {
            await Context.Channel.SendMessageAsync("User not found in DB.");
        }

        try {
            string endOfChannelName = Context.Channel.Name.Substring(9);
            int matchNumber = 0;
            int.TryParse(endOfChannelName, out matchNumber);
            Match match = await _matchService.GetByNumber(matchNumber);
            if (match != null) {
                if (Context.Channel is SocketTextChannel channel) {
                    await _embedService.SendTeamScrambleEmbed(match, channel);
                    user.DateTimeLastScrambleVote = DateTime.Now;
                    await _userService.Update(user);
                }
            }
            else {
                await ReplyAsync("A match could not be found.");
            }
        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

    [Command("ban")]
    [Summary("Ban a rogue.")]
    public async Task BanRogue([Remainder] string rogue) {
        try {
            string endOfChannelName = Context.Channel.Name.Substring(9);
            int matchNumber = 0;
            int.TryParse(endOfChannelName, out matchNumber);
            Match match = await _matchService.GetByNumber(matchNumber);
            if (match != null) {
                if (Context.User is SocketGuildUser user && Context.Channel is SocketTextChannel channel)
                    await _embedService.SendBanEmbed(match, user, channel, rogue);
            }
            else {
                await ReplyAsync("A match could not be found.");
            }
        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

    [Command("duotest")]
    [Summary("test...")]
    public async Task DuoTest(SocketUser u1 = null, SocketUser u2 = null, SocketUser u3 = null, SocketUser u4 = null, SocketUser u5 = null, SocketUser u6 = null, SocketUser u7 = null, SocketUser u8 = null) {
        try {
            List<SocketUser> mentions = Context.Message.MentionedUsers.ToList();
            if (mentions.Count < 8) {
                return;
            }
            Match match = new Match() {

            };
            PlayerQueue queue = new PlayerQueue(null, null, "NA", QueueType.NAMain);
            queue.PlayersInQueue.AddRange(mentions);
            queue.DuoPlayers.Add((mentions[0], mentions[1]));
            queue.DuoPlayers.Add((mentions[6], mentions[7]));
            var list = await _matchService.SetupAutomaticTeams(match, "Rogue Company", queue);
            var team1 = list.FindAll(x => match.Team1DiscordIds.Contains(x.DiscordId)).Select(x => x.Username).ToList();
            var team2 = list.FindAll(x => match.Team2DiscordIds.Contains(x.DiscordId)).Select(x => x.Username).ToList();
            var ttt = "hi";
        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

    [Command("duoinvite")]
    [Summary("Invite a duo partner")]
    public async Task DuoInvite(SocketGuildUser user) {
        await _discordUserCache.SetValueAsync(Context.User.Id, Context.User);
        await _discordUserCache.SetValueAsync(user.Id, user);
        if (Context.Channel.Id != _channelSettings.RoCoDuoInviteChannelId) {
            var responseMessage = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoDuoInviteChannelId}>");
            await responseMessage.DeleteAfter(5000);
            await Context.Message.DeleteAfter(5000);
            return;
        }
        await _rocoService.InviteDuoPartner(Context.User, user);
    }

    [Command("igns")]
    [Summary("Get all igns in a match")]
    public async Task GetIGNs() {
        await _discordUserCache.SetValueAsync(Context.User.Id, Context.User);
        if (!Context.Channel.Name.Contains("match")) {
            await Context.Message.DeleteAsync();
            var deleteMe = await ReplyAsync("Please use this command in a match channel.");
            await deleteMe.DeleteAfter(5000);
            await Context.Message.DeleteAfter(5000);
            return;
        }

        try {
            string endOfChannelName = Context.Channel.Name.Substring(9);
            int matchNumber = 0;
            int.TryParse(endOfChannelName, out matchNumber);
            Match match = await _matchService.GetByNumber(matchNumber);
            if (match == null) {
                await Context.Message.DeleteAsync();
                var deleteMe = await ReplyAsync($"Could not retrieve the match by number: {matchNumber}. Make sure you are only using this" +
                    $" command in a match channel.");
                await deleteMe.DeleteAfter(5000);
                await Context.Message.DeleteAfter(5000);
                return;
            }

            string description = $"{match.GameName} IGNs for this match:\n\n";
            foreach (ulong idCurr in match.Team1DiscordIds) {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.IGN != null)
                    description += $"`{userCurr.IGN}`\n";
                else {
                    IUser iUser = await Context.Channel.GetUserAsync(userCurr.DiscordId);
                    description += $"No IGN set for: {iUser.Mention}\n";
                }
            }
            foreach (ulong idCurr in match.Team2DiscordIds) {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.IGN != null)
                    description += $"`{userCurr.IGN}`\n";
                else {
                    IUser iUser = await Context.Channel.GetUserAsync(userCurr.DiscordId);
                    description += $"No IGN set for: {iUser.Mention}\n";
                }
            }

            EmbedBuilder builder = new EmbedBuilder() { Description = description };
            builder.WithColor(Color.Green);
            builder.WithFooter(new EmbedFooterBuilder() { Text = "Invite these people to a lobby!" });
            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

    [Command("match")]
    [Summary("Get the match information.")]
    public async Task MatchInfo() {
        await _discordUserCache.SetValueAsync(Context.User.Id, Context.User);
        if (!Context.Channel.Name.Contains("match")) {
            await Context.Message.DeleteAsync();
            var deleteMe = await ReplyAsync("Please use this command in a match channel.");
            await deleteMe.DeleteAfter(5000);
            await Context.Message.DeleteAfter(5000);
            return;
        }

        try {
            string endOfChannelName = Context.Channel.Name.Substring(9);
            int matchNumber = 0;
            int.TryParse(endOfChannelName, out matchNumber);
            Match match = await _matchService.GetByNumber(matchNumber);
            if (match == null) {
                await Context.Message.DeleteAsync();
                var deleteMe = await ReplyAsync($"Could not retrieve the match by number: {matchNumber}");
                await Task.Delay(10000);
                await deleteMe.DeleteAsync();
                return;
            }

            if (Context.Channel is SocketTextChannel chan)
                await Context.Channel.SendMessageAsync(null, false, await _embedService.Match(match));
        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

    // Setup the pug queue embed.
    [Command("pick")]
    [Summary("Choose a player from player pool.")]
    public async Task PickUser(SocketGuildUser pickedUser = null) {
        await _discordUserCache.SetValueAsync(Context.User.Id, Context.User);
        if (pickedUser == null) {
            await Context.Message.DeleteAsync();
            var deleteMe = await ReplyAsync("Please @mention a user to pick them.");
            await deleteMe.DeleteAfter(5000);
            await Context.Message.DeleteAfter(5000);
            return;
        }

        await _discordUserCache.SetValueAsync(pickedUser.Id, pickedUser);

        if (!Context.Channel.Name.Contains("match")) {
            await Context.Message.DeleteAsync();
            var deleteMe = await ReplyAsync("Please use this command in a match channel.");
            await deleteMe.DeleteAfter(5000);
            await Context.Message.DeleteAfter(5000);
            return;
        }

        try {
            string endOfChannelName = Context.Channel.Name.Substring(9);
            int matchNumber = 0;
            int.TryParse(endOfChannelName, out matchNumber);
            Match match = await _matchService.GetByNumber(matchNumber);
            Console.WriteLine("Map Name : " + match.MapName);
            if (match == null) {
                await Context.Message.DeleteAsync();
                var deleteMe = await ReplyAsync($"Could not retrieve the match by number: {matchNumber}");
                await deleteMe.DeleteAfter(5000);
                await Context.Message.DeleteAfter(5000);
                return;
            }
            User user = await _userService.GetById(Context.User.Id);
            User picked = await _userService.GetById(pickedUser.Id);
            Console.WriteLine("Users found.");
            if (match.PickingCaptainDiscordId != user.DiscordId) {
                await Context.Message.DeleteAsync();
                var deleteMe = await ReplyAsync("It is not your turn to pick.");
                await deleteMe.DeleteAfter(5000);
                await Context.Message.DeleteAfter(5000);
                return;
            }

            // check player pool
            if (picked == null || !match.PlayerIdsPool.Contains(picked.Id)) {
                await Context.Message.DeleteAsync();
                var deleteMe = await ReplyAsync("That player is not in the player pool for this match.");
                await deleteMe.DeleteAfter(5000);
                await Context.Message.DeleteAfter(5000);
                return;
            }
            Console.WriteLine("Adding player to team.");

            //add to team 1
            if (match.Team1DiscordIds.Contains(match.PickingCaptainDiscordId)) {
                match.Team1Ids.Add(picked.Id);
                match.PickingCaptainId = match.Captain2Id;
            }
            else if (match.Team2DiscordIds.Contains(match.PickingCaptainDiscordId)) {
                //add to team 2
                match.Team2Ids.Add(picked.Id);
                match.PickingCaptainId = match.Captain1Id;
            }

            int index = match.PlayerIdsPool.IndexOf(picked.Id);

            Console.WriteLine("Removing last player from pool.");
            match.PlayerIdsPool.RemoveAt(index);

            // auto pick last guy
            if (match.PlayerIdsPool.Count == 1) {
                // match picking captain was swapped before
                if (match.PickingCaptainId == match.Captain1Id) {
                    match.Team1Ids.Add(match.PlayerIdsPool[0]);
                    match.PlayerIdsPool.RemoveAt(0);
                }
                else {
                    match.Team2Ids.Add(match.PlayerIdsPool[0]);
                    match.PlayerIdsPool.RemoveAt(0);
                }

                Console.WriteLine("Setting team iUsers.");
                List<IUser> team1iUsers = new List<IUser>();
                foreach (string idCurr in match.Team1Ids) {
                    team1iUsers.Add(await Context.Channel.GetUserAsync(ulong.Parse(idCurr)));
                }
                List<IUser> team2iUsers = new List<IUser>();
                foreach (string idCurr in match.Team2Ids) {
                    team2iUsers.Add(await Context.Channel.GetUserAsync(ulong.Parse(idCurr)));
                }

                Console.WriteLine("Setting match channels.");
                SocketVoiceChannel team1Voice = null;
                SocketVoiceChannel team2Voice = null;
                //SocketVoiceChannel preMatchLobby = null;
                foreach (SocketVoiceChannel channel in Context.Guild.VoiceChannels) {
                    if (channel.Name.Contains(match.MatchNumber.ToString() + " Team Alpha"))
                        team1Voice = channel;
                    else if (channel.Name.Contains(match.MatchNumber.ToString() + " Team Bravo"))
                        team2Voice = channel;
                    /*
					else if (channel.Name.Contains(match.MatchNumber.ToString()) && channel.Name.Contains("Prematch"))
						preMatchLobby = channel;
						*/
                }

                try {
                    Console.WriteLine("Moving members to team channels.");
                    foreach (IUser iUserCurr in team1iUsers)
                        if (iUserCurr is SocketGuildUser userCurr)
                            if (team1Voice != null)
                                try {
                                    await userCurr.ModifyAsync(x => x.Channel = team1Voice);
                                }
                                catch (Exception) {
                                }
                    foreach (IUser iUserCurr in team2iUsers)
                        if (iUserCurr is SocketGuildUser userCurr)
                            if (team2Voice != null) {
                                try {
                                    await userCurr.ModifyAsync(x => x.Channel = team2Voice);
                                }
                                catch (Exception) {
                                }
                            }
                }
                catch {
                    Console.WriteLine("Moving user failed.");
                }


                //await preMatchLobby.DeleteAsync();

                Console.WriteLine("Sending match embed.");
                foreach (SocketTextChannel channelCurr in Context.Guild.TextChannels) {
                    if (match.GameName == "Rogue Company")
                        if (channelCurr.Id == _channelSettings.RoCoMatchLogsChannelId) {
                            await _matchService.SendMatchLog(match);
                            break;
                        }
                }
            }


            if (match.PlayerIdsPool.Count <= 0) match.MatchStatus = MatchStatus.Playing;

            await _matchService.Update(match);

            if (Context.Channel is SocketTextChannel chan) {
                await Context.Channel.SendMessageAsync(null, false, await _embedService.Match(match));
                await chan.SendMessageAsync(null, false, _embedService.GetSendPickBansEmbed(chan.GetUser(match.Team1DiscordIds[0]), chan.GetUser(match.Team2DiscordIds[0])));
            }

        }
        catch (Exception e) {
            await ReplyAsync(e.Message);
        }
    }

}