using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using DiscordBot.Settings;
using DiscordBot.Services;
using Microsoft.Extensions.Options;
using DiscordBot.Models;
using System;
using System.Linq;
using DiscordBot.Caches;
using Core.Async;

namespace DiscordBot.Reactions {
    public class ReportReactions {
        public const string MATCH_LOG_PREFIX = "Match Log #";
        private IMatchService _matchService { get; }
        private ChannelSettings _channelSettings { get; }
        private EmoteSettings _emoteSettings { get; }
        private ITeamService _teamService { get; }
        private IUserService _userService { get; }
        private RoleSettings _roleSettings { get; }
        private IDiscordUserCache _discordUserCache { get; }
        private AsyncLock _lock { get; } = new AsyncLock();

        public ReportReactions(IOptions<ChannelSettings> channelSettings, IDiscordUserCache discordUserCache,
            IOptions<EmoteSettings> emoteSettings, IMatchService matchService,
            ITeamService teamService, IUserService userService, IOptions<RoleSettings> roleSettings)
        {
            _matchService = matchService;
            _userService = userService;
            _teamService = teamService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _roleSettings = roleSettings.Value;
            _discordUserCache = discordUserCache;
        }

        public async Task ReportMatchForMessage(IMessage message, ITextChannel channel, ulong userId, string emoteName) {
            IEmbed embed = message.Embeds.FirstOrDefault(x => x.Title.Contains(MATCH_LOG_PREFIX));
            if(embed == null) {
                return;
            }
            if(!int.TryParse(embed.Title[11..], out int matchNumber)) {
                return;
            }
            try {
                Match match = null;
                IVoiceChannel openLobbyVoiceChannel = null;
                await _lock.LockAsync(async () => {
                    match = await _matchService.GetByNumber(matchNumber) ?? throw new Exception($"No match found for Match Number {matchNumber}");
                    if (match.WinningTeam == 1 || match.WinningTeam == 2 || match.MatchStatus != MatchStatus.Playing) {
                        //Match was already reported or cancelled
                        return;
                    }
                    openLobbyVoiceChannel = (await channel.Guild.GetVoiceChannelsAsync()).FirstOrDefault(x => x.Id == _channelSettings.RoCoOpenLobbyChannelId);
                    IGuildUser guildUser = await channel.GetUserAsync(userId);
                    if (!guildUser.RoleIds.Contains(_roleSettings.ScoreConfirmRoleId)) {
                        await message.RemoveReactionAsync(new Emoji(emoteName), guildUser);
                        return;
                    }
                    match.DateTimeEnded = DateTime.UtcNow;
                    if (emoteName == _emoteSettings.OneEmoteName) {
                        match.WinningTeam = 1;
                        await _matchService.ReportWin(match);
                        await _matchService.UpdateMatchLog(match);
                    }
                    else if (emoteName == _emoteSettings.TwoEmoteName) {
                        match.WinningTeam = 2;
                        await _matchService.ReportWin(match);
                        await _matchService.UpdateMatchLog(match);
                    }
                    else if (emoteName == _emoteSettings.XEmoteName) {
                        match.MatchStatus = MatchStatus.Cancelled;
                        await _matchService.Update(match);
                        await _matchService.UpdateMatchLog(match);
                    }
                });
                await CleanupMatchChannels(channel, openLobbyVoiceChannel, match);
            }
            catch (Exception e) {
                await channel.SendMessageAsync(e.Message);
            }
        }

        private async Task CleanupMatchChannels(ITextChannel socketChannel, IVoiceChannel openLobbyVoiceChannel, Match match) {
            foreach (var channelCurr in await socketChannel.Guild.GetVoiceChannelsAsync()) {
                if ((channelCurr.Name.Contains("Team Alpha") || channelCurr.Name.Contains("Team Bravo"))
                    && channelCurr.Name.Contains(match.MatchNumber.ToString()))
                {
                    await DeleteVoiceChannelAndMoveUsers(channelCurr, openLobbyVoiceChannel);
                }

            }
            foreach(var channelCurr in await socketChannel.Guild.GetTextChannelsAsync()) {
                if (channelCurr.Name.Contains($"match-{match.MatchNumber}")) {
                    await channelCurr.DeleteAsync();
                }
            }
        }

        private async Task DeleteVoiceChannelAndMoveUsers(IVoiceChannel channelCurr, IVoiceChannel openLobbyVoiceChannel) {
            if (openLobbyVoiceChannel != null) {
                foreach (var userInChannel in await channelCurr.GetUsersAsync().FlattenAsync()) {
                    try {
                        await userInChannel.ModifyAsync(y => y.Channel = new Optional<IVoiceChannel>(openLobbyVoiceChannel));
                    }
                    catch (Exception) {
                    }
                }
            }
            await channelCurr.DeleteAsync();
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
           ulong> msg, SocketTextChannel channel, SocketReaction reaction) {
            var m = await channel.GetMessageAsync(msg.Id);
            await ReportMatchForMessage(m, channel, reaction.UserId, reaction.Emote.Name);
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction) {
            // handle remove win for a team based on reaction name
        }
    }
}