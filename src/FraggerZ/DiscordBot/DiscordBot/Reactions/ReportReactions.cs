using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using DiscordBot.Settings;
using DiscordBot.Services;
using Microsoft.Extensions.Options;
using DiscordBot.Models;
using System;
using System.Linq;

namespace DiscordBot.Reactions
{
    public class ReportReactions
    {
        private readonly MatchService _matchService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly TeamService _teamService;
        private readonly UserService _userService;
        private readonly RoleSettings _roleSettings;

        public ReportReactions(IOptions<ChannelSettings> channelSettings,
            IOptions<EmoteSettings> emoteSettings, MatchService matchService,
            TeamService teamService, UserService userService, IOptions<RoleSettings> roleSettings)
        {
            _matchService = matchService;
            _userService = userService;
            _teamService = teamService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _roleSettings = roleSettings.Value;
        }

        public async Task ReportMatchForMessage(IMessage m, SocketTextChannel channel, ulong userId, string emoteName)
        {
            if (m is IUserMessage message)
                foreach (Embed embed in message.Embeds)
                {
                    if (embed.Title.Contains("Match Log #"))
                    {
                        try
                        {
                            int.TryParse(embed.Title.Substring(11), out int matchNumber);
                            Match match = await _matchService.GetByNumber(matchNumber);
                            if (match != null)
                            {
                                if (match.WinningTeam == 1 || match.WinningTeam == 2 || match.MatchStatus != MatchStatus.Playing) return;
                                var openLobbyVoiceChannel = channel.Guild.VoiceChannels.FirstOrDefault(x => x.Id == _channelSettings.RoCoOpenLobbyChannelId);

                                if (emoteName == _emoteSettings.OneEmoteName)
                                {
                                    IUser user = await message.Channel.GetUserAsync(userId);
                                    if (user is SocketGuildUser guildUser)
                                    {
                                        foreach (SocketRole role in guildUser.Roles)
                                        {
                                            if (role.Id == _roleSettings.ScoreConfirmRoleId)
                                            {
                                                match.WinningTeam = 1;
                                                //match.MatchStatus = MatchStatus.Reported;
                                                match.DateTimeEnded = DateTime.UtcNow;
                                                await _matchService.ReportWin(match, message);
                                                //await _matchService.Update(match);
                                                await _matchService.UpdateMatchLog(match, message);
                                                await CleanupMatchChannels(channel, openLobbyVoiceChannel, match);
                                                return;
                                            }
                                        }
                                        await message.RemoveReactionAsync(new Emoji(_emoteSettings.OneEmoteUnicode), guildUser);
                                    }

                                }

                                else if (emoteName == _emoteSettings.TwoEmoteName)
                                {
                                    IUser user = await message.Channel.GetUserAsync(userId);
                                    if (user is SocketGuildUser guildUser)
                                    {
                                        foreach (SocketRole role in guildUser.Roles)
                                        {
                                            if (role.Id == _roleSettings.ScoreConfirmRoleId)
                                            {
                                                match.WinningTeam = 2;
                                                //match.MatchStatus = MatchStatus.Reported;
                                                match.DateTimeEnded = DateTime.UtcNow;
                                                await _matchService.ReportWin(match, message);
                                                //await _matchService.Update(match);
                                                await _matchService.UpdateMatchLog(match, message);
                                                await CleanupMatchChannels(channel, openLobbyVoiceChannel, match);
                                                return;
                                            }
                                        }
                                        await message.RemoveReactionAsync(new Emoji(_emoteSettings.TwoEmoteUnicode), guildUser);
                                    }
                                }

                                else if (emoteName == _emoteSettings.XEmoteName)
                                {
                                    IUser user = await message.Channel.GetUserAsync(userId);
                                    if (user is SocketGuildUser guildUser)
                                    {
                                        foreach (SocketRole role in guildUser.Roles)
                                        {
                                            if (role.Id == _roleSettings.ScoreConfirmRoleId)
                                            {
                                                match.MatchStatus = MatchStatus.Cancelled;
                                                match.DateTimeEnded = DateTime.UtcNow;
                                                await _matchService.Update(match);
                                                await _matchService.UpdateMatchLog(match, message);
                                                await CleanupMatchChannels(channel, openLobbyVoiceChannel, match);
                                                return;
                                            }
                                        }
                                        await message.RemoveReactionAsync(new Emoji(_emoteSettings.TwoEmoteUnicode), guildUser);
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Match could not be found through api.");
                            }
                        }
                        catch (Exception e)
                        {
                            await channel.SendMessageAsync(e.Message);
                        }
                    }
                }
        }

        private async Task CleanupMatchChannels(SocketTextChannel socketChannel, SocketVoiceChannel openLobbyVoiceChannel, Match match)
        {
            foreach (var channelCurr in socketChannel.Guild.Channels)
            {
                if ((channelCurr.Name.Contains("Team Alpha") || channelCurr.Name.Contains("Team Bravo"))
                    && channelCurr.Name.Contains(match.MatchNumber.ToString()))
                {
                    await DeleteVoiceChannelAndMoveUsers(channelCurr, openLobbyVoiceChannel);
                }

                if (channelCurr.Name.Contains($"match-{match.MatchNumber}"))
                {
                    await channelCurr.DeleteAsync();
                }
            }
        }

        private async Task DeleteVoiceChannelAndMoveUsers(SocketGuildChannel channelCurr, SocketVoiceChannel openLobbyVoiceChannel)
        {
            if (openLobbyVoiceChannel != null)
            {
                foreach (var userInChannel in channelCurr.Users)
                {
                    try
                    {
                        await userInChannel.ModifyAsync(y => y.Channel = openLobbyVoiceChannel);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            await channelCurr.DeleteAsync();
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
           ulong> msg, SocketTextChannel channel, SocketReaction reaction)
        {
            var m = await channel.GetMessageAsync(msg.Id);
            await ReportMatchForMessage(m, channel, reaction.UserId, reaction.Emote.Name);
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            // handle remove win for a team based on reaction name
        }
    }
}