using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using DiscordBot.Settings;
using DiscordBot.Services;
using Microsoft.Extensions.Options;

namespace DiscordBot.Reactions
{
    public class RogueCompanyReactions
    {
        private readonly RoCoPugService _rocoPugService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;

        public RogueCompanyReactions(RoCoPugService rocoPugService, IOptions<ChannelSettings> channelSettings, IOptions<EmoteSettings> emoteSettings)
        {
            _rocoPugService = rocoPugService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
           ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            var user = channel.GetUser(reaction.UserId);
            string region = "NA";
            QueueType type = QueueType.NAMain;
            if(channel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId)
            {
                type = QueueType.NACPlus;
            }
            else if(channel.Id == _channelSettings.RoCoEUQueueChannelId)
            {
                type = QueueType.EUMain;
                region = "EU";
            }
            if (reaction.Emote.Name == _emoteSettings.PlayDuoEmoteName)
            {
                await _rocoPugService.StartDuoQueueAttempt(type, user);
            }
            else
            {
                await _rocoPugService.Join(region, user, channel);
            }
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, SocketTextChannel channel, SocketReaction reaction) {
            string region = "NA";
            if (channel.Id == _channelSettings.RoCoEUQueueChannelId) {
                region = "EU";
            }
            if (reaction.Emote.Name == _emoteSettings.PlayDuoEmoteName) {
                await _rocoPugService.LeaveDuo(region, channel.GetUser(reaction.UserId), channel);
            }
            else {
                await _rocoPugService.Leave(region, channel.GetUser(reaction.UserId), channel);
            }
        }
    }
}
