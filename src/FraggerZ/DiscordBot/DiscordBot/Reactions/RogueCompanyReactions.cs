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
                await _rocoPugService.StartDuoQueueAttempt(type, reaction.User.Value);
            }
            else
            {
                await _rocoPugService.Join(region, channel.GetUser(reaction.UserId), channel);
            }
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            if (channel.Id == _channelSettings.RoCoNAQueueChannelId || channel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId)
                await _rocoPugService.Leave("NA", channel.GetUser(reaction.UserId), channel);
            else if (channel.Id == _channelSettings.RoCoEUQueueChannelId)
                await _rocoPugService.Leave("EU", channel.GetUser(reaction.UserId), channel);
        }
    }
}
