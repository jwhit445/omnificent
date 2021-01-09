using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = DiscordBot.Models.Game;

namespace DiscordBot.Services
{
    public class EmbedService
    {
        private readonly BotSettings _botSettings;
        private readonly UserService _userService;
        private readonly EmoteSettings _emoteSettings;
        private readonly TeamService _teamService;
        private readonly ChannelSettings _channelSettings;
        private readonly RoleSettings _roleSettings;

        public EmbedService(UserService userService, IOptions<BotSettings> botSettings,
            IOptions<EmoteSettings> emoteSettings, TeamService teamService, IOptions<ChannelSettings> channelSettings,
            IOptions<RoleSettings> roleSettings)
        {
            _userService = userService;
            _botSettings = botSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _teamService = teamService;
            _channelSettings = channelSettings.Value;
            _roleSettings = roleSettings.Value;
        }

        public async Task SendTeamScrambleEmbed(Match match, SocketTextChannel matchTextChannel)
        {
            string title = "Vote to scramble teams!";
            string description = $"A <@&{_roleSettings.PremiumRoleId}> user has triggered a team scramble vote. " +
                $"\n\nClick the reaction if you would like the teams to be scrambled `at complete random`.\n\n" +
                "This requires `50%` user votes from this match to pass. (ie: 4 player reactions).";
            EmbedBuilder builder = new EmbedBuilder()
            {
                Description = description,
                Color = Color.LightOrange,
                Title = title
            };

            var message = await matchTextChannel.SendMessageAsync(null, false, builder.Build());
            await message.AddReactionAsync(new Emoji(_emoteSettings.FireEmoteUnicode));
        }

        public async Task SendBanEmbed(Match match, SocketGuildUser user, SocketTextChannel channel, string rogue)
        {
            if(match.MatchType == MatchType.PUGCaptains)
            {
                if (user.Id != match.Captain1DiscordId && user.Id != match.Captain2DiscordId)
                {
                    await channel.SendMessageAsync("You are not a team captain.");
                    return;
                }
            }
            else if(match.MatchType == MatchType.PUGAuto)
            {
                if (user.Id != match.Team1DiscordIds[0] && user.Id != match.Team2DiscordIds[0])
                {
                    await channel.SendMessageAsync("Only the first person listed in each team may ban a rogue. One ban per team.");
                    return;
                }
            }
            string description = $"{rogue} has been banned from usage this match by {user.Mention}.";
            string title = "Rogue Ban";
            EmbedBuilder builder = new EmbedBuilder() { Title = title, Description = description, Color = Color.Red };
            await channel.SendMessageAsync(null, false, builder.Build());
        }

        public async Task SendPlayersNeedReadyUpMessage(Match match, SocketTextChannel anySTChannel)
        {
            string description = "All players need to ready up. Please use the command `!ready` - **now**, in this match channel. " +
                "You have 120 seconds to ready up or the match will be cancelled.";
            foreach (SocketTextChannel chan in anySTChannel.Guild.TextChannels)
            {
                if (chan.Name.Contains("match") && chan.Name.Contains(match.MatchNumber.ToString()))
                {
                    await chan.SendMessageAsync(description);
                    return;
                }
            }
        }

        public async Task SendAllPlayersReadyEmbed(Match match, SocketTextChannel anyChannel)
        {
            // get match channel based on guild text channel query
            SocketTextChannel channel = null;
            foreach(SocketTextChannel chan in anyChannel.Guild.TextChannels)
            {
                if(chan.Name.Contains("match") && chan.Name.Contains(match.MatchNumber.ToString()))
                {
                    channel = chan;
                    break;
                }
            }

            Embed embed = new EmbedBuilder()
            {
                Title = "All players are ready to play!",
                Description = "LETS ROCK!",
                Color = Color.Green
            }.Build();

            if(channel != null)
            {
                try
                {
                    await channel.SendMessageAsync(null, false, embed);
                }
                catch
                {
                }            
            }
        }

        public Embed GetSendPickBansEmbed(IMentionable team1BanUser, IMentionable team2BanUser)
        {
            string description = $"Match has started. {team1BanUser.Mention} has first ban, then {team2BanUser.Mention}\r\n\r\nTo ban, type `!ban characterNameHere`";
            string title = "Character Bans";
            EmbedBuilder builder = new EmbedBuilder() { Title = title, Description = description, Color = Color.Red };
            return builder.Build();
        }

        public async Task SendScrimChallenge(Team team1, Team team2, SocketTextChannel channel)
        {
            // Check for duplicate challenges
            if(await _teamService.ScrimExists(team1, team2)) {
                var deleteMe = await channel.SendMessageAsync($"{team1.TeamName} has already challenged {team2.TeamName}...");
                await Task.Delay(10000);
                await deleteMe.DeleteAsync();
                return;
            }

            string team1Mentions = "";
            foreach (ulong idCurr in team1.MemberDiscordIds)
            {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.IGN != null)
                {
                    team1Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + $"`{userCurr.IGN}`");
                }
                else
                {
                    IUser iUserCurr = channel.GetUser(userCurr.DiscordId);
                    team1Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + iUserCurr.Mention);
                }
            }
            string team2Mentions = "";
            string team2CaptainMention = "";
            foreach (ulong idCurr in team2.MemberDiscordIds)
            {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.DiscordId == team2.CaptainDiscordId) 
                {
                    IUser iUserCurr = channel.GetUser(userCurr.DiscordId);
                    team2CaptainMention = iUserCurr.Mention;
                } 

                if (userCurr.IGN != null)
                {
                    team2Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + $"`{userCurr.IGN}`");
                }
                else
                {
                    IUser iUserCurr = channel.GetUser(userCurr.DiscordId);
                    team2Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + iUserCurr.Mention);
                }
                
                
            }
            string description = $"{team1.TeamName} is trying to scrim against your team: {team2.TeamName}\n\n" +
                $"React to this message with <{_emoteSettings.PlayEmoteUnicode}> to accept.\n\n";
            EmbedBuilder builder = new EmbedBuilder() { Description = description, Color = Color.Gold };
            builder.AddField($"{team1.TeamName}", team1Mentions);
            builder.AddField($"{team2.TeamName}", team2Mentions);
            builder.WithFooter(new EmbedFooterBuilder() { Text = "Awaiting confirmation..." });
            await channel.SendMessageAsync($"{team2CaptainMention}..");
            var message = await channel.SendMessageAsync(null, false, builder.Build());
            await message.AddReactionAsync(new Emoji($"{_emoteSettings.PlayEmoteUnicode}"));

            Scrim scrim = new Scrim();
            scrim.ChallengeMessageId = message.Id;
            scrim.Team1Id = team1.Id;
            scrim.Team2Id = team2.Id;
            scrim.TeamsChannelId = channel.Id;

            await _teamService.AddScrim(scrim);
        }

        public async Task<Embed> Leaderboard(string gameName, SocketTextChannel channel, string region = null)
        {
            List<User> users = await _userService.GetLeaderboard(gameName);
            string title = $"{gameName} Leaderboard";
            string description = "";
            int count = 0;
            for(int i = 0; i < users.Count; i++)
            {
                if(count >= 15)
                {
                    break;
                }
                if(_userService.GetTier(users[i]) == "U")
                {
                    //The user likely left the Discord server. We can't show them.
                    continue;
                }
                if(channel.GetUser(users[i].DiscordId) == null) {
                    continue;
                }
                count++;
                if (users[i].IGN == null)
                    description += $"`#{count}` - {channel.GetUser(users[i].DiscordId).Mention} - **({_userService.GetTier(users[i])})**\n";
                else description += $"`#{count}` - {channel.GetUser(users[i].DiscordId).Mention} - `{users[i].IGN}` - **({_userService.GetTier(users[i])})**\n";

            }

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = title,
                Description = description,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Top Players"
                },
                Color = Color.Gold
            };

            return builder.Build();
        }

        /// <summary>
        /// Get match embed.
        /// </summary>
        /// <param name="match"></param>
        /// <param name="channel">Only needed to retrieve discord user mentions.</param>
        /// <returns></returns>
        public async Task<Embed> Match(Match match, SocketTextChannel channel, Team team1 = null, Team team2 = null, List<User> listAllUsers=null)
        {
            if(match.PlayerDiscordIdsPool.Count > 0)
                return await PickingMatchEmbed(match, channel);

            string title = "";
            if (match.MatchType == MatchType.PUGCaptains || match.MatchType == MatchType.PUGAuto) title = $"{match.GameName} Match #{match.MatchNumber}";
            else if (match.MatchType == MatchType.Scrim) title = $"{match.GameName} Scrim #{match.MatchNumber}";

            // Build team mentions
            string description = $"**Map**: `{match.MapName}`\n\n";
            if (match.MatchRegion != null) description += $"**Region**: `{match.MatchRegion}`\n\n";
            string team1Mentions = "";
            foreach (ulong userId in match.Team1DiscordIds)
            {
                User userCurr;
                if(listAllUsers == null) 
                {
                   userCurr = await _userService.GetById(userId);
                }
                else
                {
                    userCurr = listAllUsers.FirstOrDefault(x => x.DiscordId == userId);
                }
   
                // host
                if(userId == ulong.Parse(match.Team1Ids[0]))
                {
                    if (match.GameName == "Rogue Company") description += $"**Host**: {channel.GetUser(ulong.Parse(match.Team1Ids[0])).Mention} - `{userCurr.IGN}`";
                }

                if (userCurr.IGN != null)
                {
                    team1Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + "**(" + _userService.GetTier(userCurr) + ")** " + $"<@{userCurr.DiscordId}> - " + $"`{userCurr.IGN}`");
                    if (userCurr.StreamURL != null) team1Mentions += $" - [Stream]({userCurr.StreamURL})\n";
                    else team1Mentions += "\n";
                }
                else
                {
                    team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> " + "**(" + _userService.GetTier(userCurr) + ")** " + (channel.GetUser(userCurr.DiscordId).Mention);

                    if (userCurr.StreamURL != null) team1Mentions += $" - [Stream]({userCurr.StreamURL})\n";
                    else team1Mentions += "\n";
                }
            }

            string team2Mentions = "";
            foreach (ulong userId in match.Team2DiscordIds)
            {
                User userCurr;
                if (listAllUsers == null)
                {
                    userCurr = await _userService.GetById(userId);
                }
                else
                {
                    userCurr = listAllUsers.FirstOrDefault(x => x.DiscordId == userId);
                }
                if (userCurr.IGN != null)
                {
                    team2Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + "**(" + _userService.GetTier(userCurr) + ")** " + $"<@{userCurr.DiscordId}> - " + $"`{userCurr.IGN}`");
                    if (userCurr.StreamURL != null) team2Mentions += $" - [Stream]({userCurr.StreamURL})\n";
                    else team2Mentions += "\n";
                }
                else
                {
                    team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> " + "**(" + _userService.GetTier(userCurr) + ")** " + (channel.GetUser(userCurr.DiscordId).Mention);

                    if (userCurr.StreamURL != null) team2Mentions += $" - [Stream]({userCurr.StreamURL})\n";
                    else team2Mentions += "\n";
                }
            }

            string footerText = "";
            Color color = Color.Gold;
            switch (match.MatchStatus)
            {
                case MatchStatus.Playing:
                    footerText = $"In Progress...";
                    color = Color.Green;
                    break;
                case MatchStatus.Reported:
                    footerText = $"Team {match.WinningTeam} Victory!";
                    color = Color.Blue;
                    break;
                case MatchStatus.Reversed:
                    footerText = $"Reversed: Team {match.WinningTeam} Victory!";
                    color = Color.LightOrange;
                    break;
                case MatchStatus.Cancelled:
                    footerText = $"Cancelled!";
                    color = Color.Red;
                    break;
                default:
                    footerText = $"Powered by FraggerZ";
                    color = Color.Blue;
                    break;
            }

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = title,
                Description = description,
                Footer = new EmbedFooterBuilder()
                {
                    Text = footerText
                },
                Color = color
            };

            // Get map url
            string mapURL = "";
            foreach(Game game in GameSettings.Games)
            {
                if(game.Name == match.GameName)
                {
                    mapURL = game.MapImageURLs[game.Maps.IndexOf(match.MapName)];
                    break;
                }
            }
            builder.WithThumbnailUrl(mapURL);
            if(match.MatchType == MatchType.PUGCaptains || match.MatchType == MatchType.PUGAuto)
            {
                if(listAllUsers != null)
                {
                    builder.AddField($"Team Alpha - Rank ({_userService.GetTier(listAllUsers.FindAll(x => match.Team1DiscordIds.Contains(x.DiscordId)).Select(x => x.RoCoMMR).Sum() * 100 / 4)}):", team1Mentions);
                    builder.AddField($"Team Bravo - Rank ({_userService.GetTier(listAllUsers.FindAll(x => match.Team2DiscordIds.Contains(x.DiscordId)).Select(x => x.RoCoMMR).Sum() * 100 / 4)}):", team2Mentions);
                }
                else
                {
                    builder.AddField($"Team Alpha:", team1Mentions);
                    builder.AddField($"Team Bravo:", team2Mentions);
                }
            }
            else if(match.MatchType == MatchType.Scrim)
            {
                if(team1 != null)
                {
                    builder.AddField($"{team1.TeamName}", team1Mentions);
                }
                if (team2 != null)
                {
                    builder.AddField($"{team2.TeamName}", team2Mentions);
                }
            }

            return builder.Build();
        }

        public async Task<Embed> PickingMatchEmbed(Match match, SocketTextChannel channel)
        {
            string title = $"Match #{match.MatchNumber}";

            //Get captain info
            User captain1 = await _userService.GetById(match.Captain1DiscordId);
            string captain1Mention = channel.GetUser(captain1.DiscordId).Mention;
            User captain2 = await _userService.GetById(match.Captain2DiscordId);
            string captain2Mention = channel.GetUser(captain2.DiscordId).Mention;
            string pickingCaptainMention = "";
            if (captain1.DiscordId == match.PickingCaptainDiscordId) pickingCaptainMention = captain1Mention;
            else if (captain2.DiscordId == match.PickingCaptainDiscordId) pickingCaptainMention = captain2Mention;

            // Build player pool description
            string description = $"It is {pickingCaptainMention}'s turn to pick. Choose with a player with `{_botSettings.Prefix}pick @user`...\n\n";
            List<string> playerPoolStrings = new List<string>();
            foreach(string userId in match.PlayerIdsPool)
            {
                User user = await _userService.GetById(ulong.Parse(userId));
                string str = $"**({_userService.GetTier(user)})** " + channel.GetUser(ulong.Parse(userId)).Mention;
                playerPoolStrings.Add(str);

            }
            description += "**Player Pool** : \n";
            foreach(string playerMention in playerPoolStrings)
            {
                description += (playerMention + "\n");
            }

            string footerText = "";

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = title,
                Description = description,
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Picking players..."
                },
                Color = Color.Gold
            };

            string team1Mentions = "";
            foreach(ulong userId in match.Team1DiscordIds)
            {
                User userCurr = await _userService.GetById(userId);
                team1Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + channel.GetUser(userCurr.DiscordId).Mention + "\n");
            }
            string team2Mentions = "";
            foreach (ulong userId in match.Team2DiscordIds)
            {
                User userCurr = await _userService.GetById(userId);
                team2Mentions += ($"<{_emoteSettings.GamerEmoteUnicode}> " + channel.GetUser(userCurr.DiscordId).Mention + "\n");
            }
            builder.AddField("Team Alpha", team1Mentions);
            builder.AddField("Team Bravo", team2Mentions);

            return builder.Build();
        }

        public Embed TeamInvite(Team team)
        {
            string title = team.TeamName;
            EmbedBuilder builder = new EmbedBuilder();
            string description = $"<@{team.CaptainDiscordId}> is trying to create a new team: {team.TeamName}\n\n" +
                $"With the following people: <{ string.Join(">, <@", team.MemberDiscordIds)}>" +
                $"React to this message with <{ _emoteSettings.PlayEmoteUnicode }> to accept.\n\n";

            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            builder.WithFooter(new EmbedFooterBuilder() { Text = team.GameName });
            return builder.Build();
        }

        public Embed StartDuoMessage(string queueName)
        {
            string title = $"Duo queue initiated.";
            EmbedBuilder builder = new EmbedBuilder();
            string description = $"You have initiated a request to join {queueName} with a partner. \n\n" +
                $"To invite someone, go to the `#duoinvite` channel and type the following command and we will let you know if they accept or decline: \n" +
                $"`!duoinvite @name`";

            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            return builder.Build();
        }

        public Embed InviteDuo(string queueName, IUser inviter)
        {
            string title = $"Duo invite request sent by {inviter.Username} for the {queueName} queue.";
            EmbedBuilder builder = new EmbedBuilder();
            string description = $"{inviter.Username} has requested that you join the {queueName} queue with them. \n\n"
                +"You are guaranteed to play with this player when a match is started.";

            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            builder.WithFooter(new EmbedFooterBuilder() { Text = "React below to accept or decline this invite." });
            return builder.Build();
        }

        public Embed DuoQueueJoined(string channelName, IUser partner)
        {
            string title = $"Joined a queue as a duo.";
            EmbedBuilder builder = new EmbedBuilder();
            string description = $"You have joined the {channelName} queue with {partner.Mention}.";

            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            return builder.Build();
        }

        public async Task<Embed> Team(Team team)
        {
            string title = team.TeamName;
            string description = "";
            foreach(ulong userId in team.MemberDiscordIds)
            {
                User user = await _userService.GetById(userId);

                description += $"<@{user.DiscordId}> - **({_userService.GetTier(user)})**";
                if (user.StreamURL != null) description += $"\n[Stream]({user.StreamURL})";
                description += "\n\n";
            }

            //
            // STAT SUMMARY WHILE MAP STATS ARE NOT A THING
            //
            StatSummary stats = await _teamService.GetStatSummary(team.Id);

            EmbedBuilder builder = new EmbedBuilder();

            description += $"**Wins**: `{stats.Wins}` - **Losses**: `{stats.Losses}`";
            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            builder.WithFooter(new EmbedFooterBuilder() { Text = team.GameName });
            return builder.Build();

            // UNCOMMENT WHEN MAP STATS ARE READY
            /*
            StatSummary stats = await _teamService.GetStats(team);
            int totalWins = 0;
            int totalLosses = 0;
            EmbedBuilder builder = new EmbedBuilder();

            foreach(MapStat mapStatCurr in stats.MapStats)
            {
                totalWins += mapStatCurr.Wins;
                totalLosses += mapStatCurr.Losses;
                builder.AddField($"{mapStatCurr.MapName}", $"{mapStatCurr.Wins} - {mapStatCurr.Losses}", true);
            }
            description += $"**Wins**: `{totalWins}` - **Losses**: `{totalLosses}`";
            builder.WithColor(Color.Blue);
            builder.WithDescription(description);
            builder.WithTitle(title);
            builder.WithFooter(new EmbedFooterBuilder() { Text = team.GameName });
            return builder.Build();
            */
        }

        public async Task<Embed> User(User user, string gameName)
        {
            string title = user.Username;
            string description = $"`{gameName}`\n\nRank: **({_userService.GetTier(user)})**\n";
            if (user.StreamURL != null) description += $"[Stream]({user.StreamURL})\n\n";

            EmbedBuilder builder = new EmbedBuilder().
                WithColor(Color.Green).
                WithFooter(new EmbedFooterBuilder() { Text = "FraggerZ Profile"}).
                WithTitle(title);

            // COMMENT WHEN MAP STATS ARE AVAILABLE
            StatSummary stats = await _userService.GetStatSummary(user);
            builder.AddField(new EmbedFieldBuilder() { Name = "Wins", Value = stats.Wins.ToString() });
            builder.AddField(new EmbedFieldBuilder() { Name = "Losses", Value = stats.Losses.ToString() });
            builder.AddField(new EmbedFieldBuilder() { Name = "Leaderboard Position", Value = (stats.RankPosition + 1).ToString() });
            builder.WithDescription(description);
            builder.WithFooter("*Coming soon: map stats!*");
            return builder.Build();

            // UNCOMMENT WHEN MAP STATS ARE AVAILABLE
            /*
            Stats stats = await _userService.GetStats(user);
            int totalWins = 0;
            int totalLosses = 0;
            foreach (MapStat mapStat in stats.MapStats)
            {
                totalWins += mapStat.Wins;
                totalLosses += mapStat.Losses;
                builder.AddField($"{mapStat.MapName}", $"{mapStat.Wins} - {mapStat.Losses}", true);
            }
            description += $"**Wins**: `{totalWins}` - **Losses**: `{totalLosses}`";
            builder.WithDescription(description);
            return builder.Build();
            */
        }

        public async Task UpdateQueueEmbed(PlayerQueue queue, SocketTextChannel channel, bool matchWasGenerated = false)
        {
            // Update pug embed in the channel
            var messages = await channel.GetMessagesAsync(1).FlattenAsync();
            var message = messages.FirstOrDefault();
            if (message == null)
                message = channel.GetCachedMessages(1).FirstOrDefault();

            string description = $"Users in queue: `{queue.PlayersInQueue.Count}` / 8\n\n";
            var listDuoIds = queue.DuoPlayers.Select(x => x.Item1.Id).Union(queue.DuoPlayers.Select(x => x.Item2.Id)).ToList();
            foreach (IUser userCurr in queue.PlayersInQueue)
            {
                description += userCurr.Mention;
                if(listDuoIds.Contains(userCurr.Id)) {
                    description += " (duo)";
                }
                description += "\n";
            }
            Embed embed = new EmbedBuilder() { Color = Color.Green, Title = "PUG Queue" + (channel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId ? " For Rank C+ and up" : ""), Description = description }.Build();

            if (message is RestUserMessage restUserMessage)
            {
                try {
                    await restUserMessage.ModifyAsync(x => { x.Embed = embed; });
                }
                catch (Exception ee) {

                    throw;
                }

                // clear all reactions if queue empty ( match was just generated )
                if (matchWasGenerated)
                {
                    if (queue.PlayersInQueue.Count <= 0)
                    {

                        await restUserMessage.RemoveAllReactionsAsync();
                        await restUserMessage.AddReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode));
                        if(queue.QueueType == QueueType.NAMain) {
                            await restUserMessage.AddReactionAsync(new Emoji(_emoteSettings.PlayDuoEmoteUnicode));
                        }
                    }
                }
            }

            
        }
    }
}
