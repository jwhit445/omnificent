using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public interface IChannelCache : ICache<ulong, IChannel> {
        Task<IMessageChannel> GetMatchLogChannel();
        Task InitCache(IDiscordClient client);
    }
}
