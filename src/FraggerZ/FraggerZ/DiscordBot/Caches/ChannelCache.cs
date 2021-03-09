using Core;
using Core.Async;
using Discord;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public class ChannelCache : CacheAbs<ulong, IChannel>, IChannelCache {

        public ChannelSettings _channelSettings { get; }

        public ChannelCache(IOptions<ChannelSettings> channelSettings) => _channelSettings = channelSettings.Value;

        ///<summary>Throws an exception if there is no RoCoMatchLogsChannelId in the cache, or if it isn't an IMessageChannel.</summary>
        public async Task<IMessageChannel> GetMatchLogChannel() {
            try {
                return (await GetValueAsync(_channelSettings.RoCoMatchLogsChannelId)) as IMessageChannel;
            }
            catch (Exception ex) {
                throw new Exception($"Unable to retrieve Match Log channel.", ex);
            }
        }

        ///<summary>Throws an exception if there is no RoCoMatchLogsChannelId in the cache, or if it isn't an IMessageChannel.</summary>
        public async Task InitCache(IDiscordClient client) {
            try {
                List<Func<Task>> listTasks = new List<Func<Task>>();
                foreach(var idProperty in _channelSettings.GetType().GetProperties().Where(x => x.Name.EndsWith("Id"))) {
                    listTasks.Add(async () => {
                        var channel = await client.GetChannelAsync((ulong)idProperty.GetValue(_channelSettings));
                        if(channel == null) {
                            return;
                        }
                        await SetValueAsync(channel.Id, channel);
                    });
                }
                await TaskUtils.WhenAll(listTasks);
                var numCache = Cache.Count;
            }
            catch (Exception ex) {
                throw new Exception($"Unable to init {nameof(ChannelCache)}.", ex);
            }
        }
    }
}
