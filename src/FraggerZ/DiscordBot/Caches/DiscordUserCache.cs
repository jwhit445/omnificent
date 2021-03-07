using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public class DiscordUserCache : CacheAbs<ulong, IUser>, IDiscordUserCache {
        public async Task<IUser> GetUserAsync(ulong key, Func<Task<IUser>> fillTask = null) {
            return await GetValueAsync(key, fillTask);
        }
    }
}
