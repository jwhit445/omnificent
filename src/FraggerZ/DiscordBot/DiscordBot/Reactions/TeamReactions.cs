using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using DiscordBot.Settings;
using DiscordBot.Services;
using Microsoft.Extensions.Options;
using DiscordBot.Models;

namespace DiscordBot.Reactions
{
    public class TeamReactions
    {
        private readonly MatchService _matchService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly TeamService _teamService;
        private readonly UserService _userService;

        public TeamReactions(IOptions<ChannelSettings> channelSettings, 
            IOptions<EmoteSettings> emoteSettings, MatchService matchService,
            TeamService teamService, UserService userService)
        {
            _matchService = matchService;
            _userService = userService;
            _teamService = teamService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
           ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            if(reaction.Emote.Name == _emoteSettings.PlayEmoteName)
            {
                //handle a scrim accept
                string gameName = "";
                if (channel.CategoryId == _channelSettings.RoCoCategoryId) gameName = "Rogue Company";

                int removeIndex = -1;
                Scrim scrim = await _teamService.GetScrimByMessageId(reaction.MessageId);
                if (scrim == null) return;

                if (scrim.ChallengeMessageId == message.Id)
                {
                    Team team2 = await _teamService.Get(scrim.Team2Id);
                    User user = await _userService.GetById(reaction.UserId);
                    if (user.DiscordId == team2.CaptainDiscordId)
                    {
                        // Only generate scrim if team2 captain accepts it.
                        Team team1 = await _teamService.Get(scrim.Team1Id);
                        await _matchService.GenerateScrim(gameName, channel, team1, team2);
                    }
                    else
                    {
                        await reaction.Message.Value.RemoveReactionAsync(new Emoji(_emoteSettings.PlayEmoteUnicode), channel.GetUser(reaction.UserId));
                    }
                }

                await _teamService.RemoveCurrentScrim(scrim);
            }
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {

        }
    }
}
