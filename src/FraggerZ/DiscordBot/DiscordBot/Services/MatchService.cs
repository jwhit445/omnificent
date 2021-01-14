using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Game = DiscordBot.Models.Game;

namespace DiscordBot.Services {
    public class MatchService {
        private readonly EmbedService _embedService;
        private readonly RoleSettings _roleSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly UserService _userService;
        private readonly ChannelSettings _channelSettings;
        private readonly APISettings _apiSettings;
        private HttpClient httpClient;
        Random _rnd = new Random();

        public MatchService(UserService userService, EmbedService embedService,
            IOptions<RoleSettings> roleSettings, IOptions<EmoteSettings> emoteSettings,
            IOptions<ChannelSettings> channelSettings, IOptions<APISettings> apiSettings) {
            _userService = userService;
            _embedService = embedService;
            _roleSettings = roleSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _channelSettings = channelSettings.Value;
            _apiSettings = apiSettings.Value;
            httpClient = new HttpClient();
        }

        public async Task ReportWin(Match match, IUserMessage message) {
            var JSON = JsonConvert.SerializeObject(match);
            var response = await httpClient.PutAsync(_apiSettings.BaseURL + $"/match/{match.Id}/report", new StringContent(JSON, System.Text.Encoding.UTF8));
            Console.WriteLine($"Attempt to report win for match {match.MatchNumber} - Status: " + response.StatusCode);
        }

        public async Task UpdateMatchLog(Match reportedMatch, IUserMessage iMessage = null, SocketTextChannel anyTextChannel = null) {
            Match match = await Get(reportedMatch.Id);
            EmbedBuilder builder = new EmbedBuilder();
            string title = "";
            if (match.MatchType == MatchType.PUGCaptains || match.MatchType == MatchType.PUGAuto) title = $"Match Log #{match.MatchNumber}";
            else if (match.MatchType == MatchType.Scrim) title = $"Scrim Log #{match.MatchNumber}";
            string description = "Please react to this message with the appropriate winning team number to report the match." +
                $"\n\n*Note*: <@&{_roleSettings.ScoreConfirmRoleId}> role is required to report a match.";
            string team1Mentions = "";

            IUserMessage message = null;
            if (iMessage != null) {
                message = iMessage;
                await FinishUpdating();
                return;
            }
            //search for message else

            else if (anyTextChannel != null) {
                //find channel
                SocketTextChannel channel = anyTextChannel.Guild.GetTextChannel(_channelSettings.RoCoMatchLogsChannelId);

                if (channel != null) {
                    var messages = await channel.GetMessagesAsync(10000).FlattenAsync();
                    foreach (var msg in messages) {
                        foreach (Embed embed in message.Embeds) {
                            if (embed.Title.Contains($"Match Log #{match.MatchNumber}")) {
                                if (msg is IUserMessage iUserMessage) {
                                    message = iUserMessage;
                                    if (message != null)
                                        await FinishUpdating();
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            async Task FinishUpdating() {
                foreach (ulong idCurr in match.Team1DiscordIds) {
                    User userCurr = await _userService.GetById(idCurr);
                    if (userCurr.IGN != null)
                        team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.IGN}\n";
                    else {
                        IUser iUserCurr = await message.Channel.GetUserAsync(userCurr.DiscordId);
                        if (iUserCurr == null) {
                            team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.Username}\n";
                        }
                        else {
                            team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {iUserCurr.Mention}\n";
                        }
                    }
                }

                string team2Mentions = "";
                foreach (ulong idCurr in match.Team2DiscordIds) {
                    User userCurr = await _userService.GetById(idCurr);
                    if (userCurr.IGN != null)
                        team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.IGN}\n";
                    else {
                        IUser iUserCurr = await message.Channel.GetUserAsync(userCurr.DiscordId);
                        if (iUserCurr == null) {
                            team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.Username}\n";
                        }
                        else {
                            team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {iUserCurr.Mention}\n";
                        }
                    }
                }

                builder.AddField("Team Alpha", team1Mentions);
                builder.AddField("Team Bravo", team2Mentions);
                builder.WithDescription(description);
                builder.WithTitle(title);

                string footerText = "";
                Color color = Color.Gold;
                switch (match.MatchStatus) {
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
                builder.WithFooter(new EmbedFooterBuilder() { Text = footerText });
                await message.ModifyAsync(x => x.Embed = builder.Build());
            }


        }

        public async Task SendMatchLog(Match match, SocketTextChannel matchLogChannel) {
            // send an embed to matchlogchannel
            // react to that embed with :1: or :2:
            EmbedBuilder builder = new EmbedBuilder();
            string title = "";
            if (match.MatchType == MatchType.PUGCaptains || match.MatchType == MatchType.PUGAuto) title = $"Match Log #{match.MatchNumber}";
            else if (match.MatchType == MatchType.Scrim) title = $"Scrim Log #{match.MatchNumber}";
            string description = "Please react to this message with the appropriate winning team number to report the match." +
                $"\n\n*Note*: <@&{_roleSettings.ScoreConfirmRoleId}> role is required to report a match.";
            string team1Mentions = "";
            foreach (ulong idCurr in match.Team1DiscordIds) {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.IGN != null)
                    team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.IGN}\n";
                else {
                    IUser iUserCurr = matchLogChannel.GetUser(userCurr.DiscordId);
                    team1Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {iUserCurr.Mention}\n";
                }
            }

            string team2Mentions = "";
            foreach (ulong idCurr in match.Team2DiscordIds) {
                User userCurr = await _userService.GetById(idCurr);
                if (userCurr.IGN != null)
                    team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {userCurr.IGN}\n";
                else {
                    IUser iUserCurr = matchLogChannel.GetUser(userCurr.DiscordId);
                    team2Mentions += $"<{_emoteSettings.GamerEmoteUnicode}> {iUserCurr.Mention}\n";
                }
            }

            builder.AddField("Team Alpha", team1Mentions);
            builder.AddField("Team Bravo", team2Mentions);
            builder.WithDescription(description);
            builder.WithTitle(title);
            var message = await matchLogChannel.SendMessageAsync(null, false, builder.Build());
            await message.AddReactionsAsync(new IEmote[] { new Emoji(_emoteSettings.OneEmoteUnicode), new Emoji(_emoteSettings.TwoEmoteUnicode),
              new Emoji(_emoteSettings.XEmoteUnicode)});

        }

        public async Task GenerateScrim(string gameName, SocketTextChannel channel, Team team1, Team team2) {
            Match match = new Match();

            // TODO : Pull match number from DB
            match.MatchNumber = 1;
            match.Id = "xyz";
            match.GameName = gameName;
            match.MatchType = MatchType.Scrim;

            // choose a random map
            int randIndex = -1;
            Random rnd = new Random();
            foreach (Game game in GameSettings.Games) {
                if (game.Name == gameName) {
                    randIndex = rnd.Next(game.Maps.Count);
                    match.MapName = game.Maps[randIndex];
                    match.MapImageURL = game.MapImageURLs[randIndex];
                    break;
                }
            }

            RestTextChannel textChannel = await channel.Guild.CreateTextChannelAsync($"{_emoteSettings.TrophyEmoteUnicode}-scrim-{match.MatchNumber}", x => x.CategoryId = channel.CategoryId);
            await channel.SyncPermissionsAsync();
            RestVoiceChannel voice1 = await channel.Guild.CreateVoiceChannelAsync($"{_emoteSettings.SpeakerEmoteUnicode} {team1.TeamName}", x => x.CategoryId = channel.CategoryId);
            RestVoiceChannel voice2 = await channel.Guild.CreateVoiceChannelAsync($"{_emoteSettings.SpeakerEmoteUnicode} {team2.TeamName}", x => x.CategoryId = channel.CategoryId);
            await voice1.SyncPermissionsAsync();
            await voice2.SyncPermissionsAsync();

            // deny write and connect privs to everyone
            foreach (SocketRole roleCurr in channel.Guild.Roles) {
                if (roleCurr.Id == _roleSettings.EveryoneRoleId) {
                    await textChannel.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, PermValue.Deny, null, PermValue.Deny));
                    await voice1.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Deny));
                    await voice2.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Deny));
                    break;
                }
            }

            string announcement = $"Scrim #{match.MatchNumber} is ready! ";

            foreach (ulong id in team1.MemberDiscordIds) {
                match.Team1Ids.Add(id.ToString());
                User user = await _userService.GetById(id);
                IUser userCurr = channel.GetUser(user.DiscordId);
                announcement += (userCurr.Mention + " ");
                await textChannel.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, PermValue.Allow, null, PermValue.Allow));
                await voice1.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Allow));
                await voice2.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Allow));
            }
            foreach (ulong id in team2.MemberDiscordIds) {
                match.Team2Ids.Add(id.ToString());
                User user = await _userService.GetById(id);
                IUser userCurr = channel.GetUser(user.DiscordId);
                announcement += (userCurr.Mention + " ");
                await textChannel.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, PermValue.Allow, null, PermValue.Allow));
                await voice1.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Allow));
                await voice2.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Allow));
            }

            // send match embed and announcement to channel and ping users
            await textChannel.SendMessageAsync(announcement, false, null);
            await textChannel.SendMessageAsync(null, false, await _embedService.Match(match, channel, team1, team2));

            foreach (SocketTextChannel channelCurr in channel.Guild.TextChannels) {
                if (gameName == "Rogue Company") {
                    if (channelCurr.Id == _channelSettings.RoCoMatchLogsChannelId) {
                        await SendMatchLog(match, channelCurr);
                        break;
                    }
                }
            }

            await SendToAPI(match);
        }

        public async Task GeneratePUG(string gameName, SocketTextChannel queueChannel, PlayerQueue queue, string region) {
            Match match = new Match();
            match.GameName = gameName;
            match.MatchRegion = region;
            List<User> listAllUsers;
            if (queueChannel.Id == _channelSettings.RoCoNAQueueChannelId) {
                listAllUsers = await SetupAutomaticTeams(match, gameName, queue);
            }
            else {
                listAllUsers = await SetupCaptainsPick(match, gameName, queue);
            }


            // choose a random map
            foreach (Game game in GameSettings.Games) {
                if (game.Name == gameName) {
                    int randIndex = _rnd.Next(game.Maps.Count);
                    match.MapName = game.Maps[randIndex];
                    match.MapImageURL = game.MapImageURLs[randIndex];
                    break;
                }
            }

            match = await SendToAPI(match);

            // Text Channel
            RestTextChannel channel = await queueChannel.Guild.CreateTextChannelAsync($"{_emoteSettings.TrophyEmoteUnicode}-match-{match.MatchNumber}", x => x.CategoryId = queueChannel.CategoryId);
            await channel.SyncPermissionsAsync();

            // Prematch Voice Channel
            //RestVoiceChannel preMatchVoice = await queueChannel.Guild.CreateVoiceChannelAsync($"{_emoteSettings.PreMatchEmoteUnicode} Prematch #{match.MatchNumber}", x => x.CategoryId = queueChannel.CategoryId);
            //await preMatchVoice.SyncPermissionsAsync();

            // Voice Channels
            RestVoiceChannel voice1 = await queueChannel.Guild.CreateVoiceChannelAsync($"{_emoteSettings.SpeakerEmoteUnicode} M{match.MatchNumber} Team Alpha", x => x.CategoryId = queueChannel.CategoryId);
            RestVoiceChannel voice2 = await queueChannel.Guild.CreateVoiceChannelAsync($"{_emoteSettings.SpeakerEmoteUnicode} M{match.MatchNumber} Team Bravo", x => x.CategoryId = queueChannel.CategoryId);
            await voice1.SyncPermissionsAsync();
            await voice2.SyncPermissionsAsync();

            // deny write and connect privs to everyone
            foreach (SocketRole roleCurr in queueChannel.Guild.Roles) {
                if (roleCurr.Id == _roleSettings.EveryoneRoleId) {
                    await channel.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, PermValue.Deny));
                    //await preMatchVoice.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Deny));
                    await voice1.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Deny));
                    await voice2.AddPermissionOverwriteAsync(roleCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Deny));
                    break;
                }
            }

            // give all users in queue the ability to write to match channel and connect to voice
            foreach (IUser userCurr in queue.PlayersInQueue) {
                await channel.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, PermValue.Allow));
                //await preMatchVoice.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, null, null, null, null, null, null, null, null, null, PermValue.Allow));
                await voice1.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, PermValue.Allow, null, null, null, null, null, null, null, null, PermValue.Allow));
                await voice2.AddPermissionOverwriteAsync(userCurr, new OverwritePermissions().Modify(null, null, null, PermValue.Allow, null, null, null, null, null, null, null, null, PermValue.Allow));
            }

            string announcement = $"Match #{match.MatchNumber} is ready! ";
            // send match embed and announcement to channel and ping users
            foreach (IUser userCurr in queue.PlayersInQueue) {
                announcement += (userCurr.Mention + " ");
            }
            await channel.SendMessageAsync(announcement, false, null);
            await channel.SendMessageAsync(null, false, await _embedService.Match(match, queueChannel, listAllUsers: listAllUsers));
            if (match.MatchType == MatchType.PUGAuto) {
                await channel.SendMessageAsync(null, false, _embedService.GetSendPickBansEmbed(queueChannel.GetUser(match.Team1DiscordIds[0]), queueChannel.GetUser(match.Team2DiscordIds[0])));
            }

            //move players
            if (match.MatchType == MatchType.PUGAuto) {
                var team1Users = listAllUsers.FindAll(user => match.Team1DiscordIds.Contains(user.DiscordId)).Select(x => queueChannel.GetUser(x.DiscordId));
                var team2Users = listAllUsers.FindAll(user => match.Team2DiscordIds.Contains(user.DiscordId)).Select(x => queueChannel.GetUser(x.DiscordId));
                foreach (SocketGuildUser user in team1Users) {
                    try {
                        await user.ModifyAsync(x => x.Channel = voice1);
                    }
                    catch (Exception e) {
                    }
                }
                foreach (SocketGuildUser user in team2Users) {
                    try {
                        await user.ModifyAsync(x => x.Channel = voice2);
                    }
                    catch (Exception e) {
                    }
                }
            }


            if (match.MatchType == MatchType.PUGAuto) {
                await SendMatchLog(match, queueChannel.Guild.TextChannels.First(x => x.Id == _channelSettings.RoCoMatchLogsChannelId));
            }
        }

        public async Task<List<User>> SetupAutomaticTeams(Match match, string gameName, PlayerQueue queue) {
            match.MatchStatus = MatchStatus.Playing;
            match.MatchType = MatchType.PUGAuto;
            match.PlayerIdsPool.Clear();
            List<User> listAllUsers = new List<User>();
            var players = queue.PlayersInQueue;
            if(queue.PlayersInQueue.Count > 8) {
                players = queue.PlayersInQueue.GetRange(0, 8);
            }
            foreach (IUser userCurr in players) {
                User dbUser = await _userService.GetById(userCurr.Id);
                if (dbUser == null || dbUser.DiscordId == 0) {
                    throw new Exception("Invalid player in queue");
                }
                listAllUsers.Add(dbUser);
            }
            listAllUsers.Sort((o1, o2) => {
                (IUser, IUser) duo1 = queue.DuoPlayers.FirstOrDefault(x => x.Item1.Id == o1.DiscordId || x.Item2.Id == o1.DiscordId);
                (IUser, IUser) duo2 = queue.DuoPlayers.FirstOrDefault(x => x.Item1.Id == o2.DiscordId || x.Item2.Id == o2.DiscordId);
                if(duo1.Item1 != null && duo2.Item1 != null) {
                    return o1.RoCoMMR > o2.RoCoMMR ? -1 : 1;
                }
                if(duo1.Item1 != null) {
                    return -1;
                }
                if(duo2.Item2 != null) {
                    return 1;
                }
                return o1.RoCoMMR > o2.RoCoMMR ? -1 : 1;
            });
            List<User> team1 = new List<User>();
            List<User> team2 = new List<User>();
            foreach (var user in listAllUsers) {
                if(team1.Contains(user) || team2.Contains(user)) {
                    continue;
                }
                if (GetTeamMmr(team1) <= GetTeamMmr(team2) && team1.Count < 4) {
                    team1.Add(user);
                    AddTeammateIfDuo(team1, queue, user, listAllUsers);
                }
                else if(team2.Count < 4) {
                    team2.Add(user);
                    AddTeammateIfDuo(team2, queue, user, listAllUsers);
                }
            }
            match.Team1Ids = team1.Select(x => x.Id).ToList();
            match.Team2Ids = team2.Select(x => x.Id).ToList();
            return listAllUsers;
        }

        private void AddTeammateIfDuo(List<User> listTeam, PlayerQueue queue, User user, List<User> listAllUsers) {
            var duo = queue.DuoPlayers.FirstOrDefault(x => x.Item1.Id == user.DiscordId || x.Item2.Id == user.DiscordId);
            if (duo.Item1 != null) {
                if (listTeam.Count >= 4) {
                    throw new Exception("Team is full but has a duo partner");
                }
                var teammateId = (duo.Item1.Id == user.DiscordId ? duo.Item2.Id : duo.Item1.Id);
                listTeam.Add(listAllUsers.First(x => x.DiscordId == teammateId));
            }
        }

        private double GetTeamMmr(List<User> team) {
            return team.Select(x => x.RoCoMMR).Sum();
        }

        private async Task<List<User>> SetupCaptainsPick(Match match, string gameName, PlayerQueue queue) {
            match.MatchType = MatchType.PUGCaptains;

            // Temporary list constructed from queue to build top 4 random captains
            List<User> captainsPool = new List<User>();
            List<User> listAllUsers = new List<User>();

            foreach (SocketGuildUser userCurr in queue.PlayersInQueue) {
                User dbUser = await _userService.GetById(userCurr.Id);
                if (dbUser == null) {
                    throw new Exception("Invalid player in queue");
                }
                captainsPool.Add(dbUser);
                listAllUsers.Add(dbUser);
                match.PlayerIdsPool.Add(dbUser.Id);
            }

            // choose two random captains from top 4 mmr
            List<User> top4MMRs = new List<User>();

            for (int j = 0; j < 3; j++) {
                User maxUser = captainsPool[0];
                int removeIndex = 0;
                for (int i = 0; i < captainsPool.Count; i++) {
                    if (gameName == "Rogue Company") {
                        if (captainsPool[i].RoCoMMR > maxUser.RoCoMMR) {
                            maxUser = captainsPool[i];
                            removeIndex = i;
                        }
                    }
                    else if (gameName == "IronSight") {
                        if (captainsPool[i].IronSightMMR > maxUser.IronSightMMR) {
                            maxUser = captainsPool[i];
                            removeIndex = i;
                        }
                    }
                    else if (gameName == "CrossFire") {
                        if (captainsPool[i].CrossFireMMR > maxUser.CrossFireMMR) {
                            maxUser = captainsPool[i];
                            removeIndex = i;
                        }
                    }
                }
                top4MMRs.Add(maxUser);
                captainsPool.RemoveAt(removeIndex);
            }
            int randIndex = _rnd.Next(0, top4MMRs.Count - 1);
            match.Captain1Id = top4MMRs[randIndex].Id;
            match.Team1Ids.Add(match.Captain1Id);
            top4MMRs.RemoveAt(randIndex);
            randIndex = _rnd.Next(0, top4MMRs.Count - 1);
            match.Captain2Id = top4MMRs[randIndex].Id;

            if (match.PlayerIdsPool.Contains(match.Captain1Id))
                match.PlayerIdsPool.RemoveAt(match.PlayerIdsPool.IndexOf(match.Captain1Id));
            if (match.PlayerIdsPool.Contains(match.Captain2Id))
                match.PlayerIdsPool.RemoveAt(match.PlayerIdsPool.IndexOf(match.Captain2Id));

            match.Team2Ids.Add(match.Captain2Id);
            match.PickingCaptainId = match.Captain1Id;
            return listAllUsers;
        }

        public async Task<Match> SendToAPI(Match match) {
            var JSON = JsonConvert.SerializeObject(match);
            var response = await httpClient.PostAsync(_apiSettings.BaseURL + "/match", new StringContent(JSON, System.Text.Encoding.UTF8));
            var contents = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Match>(contents);
        }

        public async Task Update(Match match) {
            var JSON = JsonConvert.SerializeObject(match);
            var response = await httpClient.PutAsync(_apiSettings.BaseURL + $"/match/{match.Id}", new StringContent(JSON, System.Text.Encoding.UTF8));
            Console.WriteLine($"Attempt to update match {match.MatchNumber} in DB - Status: " + response.StatusCode);
        }

        public async Task<Match> Get(string matchId) {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/match/{matchId}");
            Match match = null;
            if (response != null) {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { match = JsonConvert.DeserializeObject<Match>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return match;
        }

        public async Task<Match> GetByNumber(int matchNumber) {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/match/?matchNumber={matchNumber}");
            Match match = null;
            if (response == null || response.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Error calling GetByNumber API endpoint");
            }
            var jsonString = await response.Content.ReadAsStringAsync();
            try { match = JsonConvert.DeserializeObject<Match>(jsonString); }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return match;
        }
    }
}
