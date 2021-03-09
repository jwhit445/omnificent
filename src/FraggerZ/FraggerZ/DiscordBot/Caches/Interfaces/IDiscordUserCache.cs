using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public interface IDiscordUserCache : ICache<ulong, IUser> {
        public Task<IUser> GetUserAsync(ulong key, Func<Task<IUser>> fillTask = null);
    }
}
