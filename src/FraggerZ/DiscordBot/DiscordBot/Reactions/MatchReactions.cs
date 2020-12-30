using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Services;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Reactions
{
    public class MatchReactions
    {
        private readonly MatchService _matchService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly TeamService _teamService;
        private readonly UserService _userService;
        private readonly EmbedService _embedService;
        private readonly RoCoPugService _rocoPugService;

        public MatchReactions(IOptions<ChannelSettings> channelSettings,
            IOptions<EmoteSettings> emoteSettings, MatchService matchService,
            TeamService teamService, UserService userService, EmbedService embedService, RoCoPugService rocoPugService)
        {
            _matchService = matchService;
            _userService = userService;
            _teamService = teamService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _embedService = embedService;
            _rocoPugService = rocoPugService;
        }

        public async Task HandleDMReactionAddedAsync(Cacheable<IUserMessage,
           ulong> message, SocketDMChannel channel, SocketReaction reaction)
        {
            var user = channel.GetUser(reaction.UserId);
            if (reaction.Emote.Name == _emoteSettings.CheckEmoteName)
            {
                if(_rocoPugService.DuoPartners.ContainsKey(user.Id))
                {
                    var queue = _rocoPugService.DuoPartners[user.Id].queue;
                    await _rocoPugService.FinalizeDuoPartners(queue, user.Id, _rocoPugService.DuoPartners[user.Id].player2);
                }
            }
            else if(reaction.Emote.Name == _emoteSettings.XEmoteName)
            {
                if(_rocoPugService.DuoPartners.ContainsKey(user.Id))
                {
                    _rocoPugService.DuoPartners.Remove(user.Id);
                }
            }
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
           ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            if (!channel.Name.Contains("match")) return;

            try
            {
                string endOfChannelName = channel.Name.Substring(9);
                int matchNumber = 0;
                int.TryParse(endOfChannelName, out matchNumber);
                Match match = await _matchService.GetByNumber(matchNumber);
                if (match != null)
                {
                    var embedMessage = await channel.GetMessageAsync(message.Id);

                    //check message to make sure it's a vote embed
                    bool isTeamChangeMessage = false;
                    foreach(var embed in embedMessage.Embeds)
                    {
                        if (embed.Title.Contains("Vote to scramble teams!")) { isTeamChangeMessage = true; break; }
                    }
                    if (!isTeamChangeMessage) return;

                    //check if reaction userId is on a team
                    if (!match.Team1DiscordIds.Contains(reaction.UserId) && !match.Team2DiscordIds.Contains(reaction.UserId))
                    {
                        await embedMessage.RemoveReactionAsync(new Emoji(_emoteSettings.FireEmoteUnicode), reaction.UserId);
                        return;
                    }

                    // count votes
                    int count = 0;
                    foreach(var key in embedMessage.Reactions.Keys)
                    {
                        if (key.Name == _emoteSettings.FireEmoteName)
                        {
                            count = embedMessage.Reactions[key].ReactionCount;
                            break;
                        }      
                    }

                    // count starts at 1
                    if(count >= 5)
                    {
                        //scramble teams

                        List<ulong> allDiscordIds = new List<ulong>();
                        foreach (ulong discordId in match.Team1DiscordIds) allDiscordIds.Add(discordId);
                        foreach (ulong discordId in match.Team2DiscordIds) allDiscordIds.Add(discordId);

                        // shuffle list (fisher-yates)
                        Random rng = new Random();
                        int n = allDiscordIds.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = rng.Next(n + 1);
                            ulong value = allDiscordIds[k];
                            allDiscordIds[k] = allDiscordIds[n];
                            allDiscordIds[n] = value;
                        }

                        //set teams
                        for(int i = 0; i < allDiscordIds.Count / 2; i++)
                        {
                            match.Team1Ids[i] = allDiscordIds[i].ToString();
                        }
                        for (int i = allDiscordIds.Count / 2; i < allDiscordIds.Count; i++)
                        {
                            match.Team2Ids[i] = allDiscordIds[i].ToString();
                        }

                        await _matchService.Update(match);

                        await channel.SendMessageAsync($"The team scramble vote has passed! Please refer to the new teams for setting up your lobby!");

                        await channel.SendMessageAsync(null, false, await _embedService.Match(match, channel));

                        // move users to their voice channels
                        SocketVoiceChannel voice1 = null;
                        SocketVoiceChannel voice2 = null;
                        foreach(var voiceChannel in channel.Guild.VoiceChannels)
                        {
                            if (voiceChannel.Name.Contains("M") &&
                                voiceChannel.Name.Contains(match.MatchNumber.ToString()) &&
                                voiceChannel.Name.Contains("Team Alpha")) voice1 = voiceChannel;
                            else if (voiceChannel.Name.Contains("M") &&
                                voiceChannel.Name.Contains(match.MatchNumber.ToString()) &&
                                voiceChannel.Name.Contains("Team Bravo")) voice1 = voiceChannel;
                        }

                        List<SocketGuildUser> listAllUsers = new List<SocketGuildUser>();
                        foreach(ulong id in allDiscordIds)
                        {
                            listAllUsers.Add(channel.GetUser(id));
                        }
                        var team1Users = listAllUsers.FindAll(user => match.Team1DiscordIds.Contains(user.Id));
                        var team2Users = listAllUsers.FindAll(user => match.Team2DiscordIds.Contains(user.Id));
                        foreach (SocketGuildUser user in team1Users)
                        {
                            try
                            {
                                await user.ModifyAsync(x => x.Channel = voice1);
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        foreach (SocketGuildUser user in team2Users)
                        {
                            try
                            {
                                await user.ModifyAsync(x => x.Channel = voice2);
                            }
                            catch (Exception e)
                            {
                            }
                        }

                        //update channel
                        await _matchService.UpdateMatchLog(match, null, channel);
                    }
                }
                else
                {
                    await channel.SendMessageAsync("A match could not be found.");
                }
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.Message);
            }
        }
    }
}
