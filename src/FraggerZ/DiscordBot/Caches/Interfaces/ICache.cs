using Core.Async;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public interface ICache<K,V> {
        public Dictionary<K, V> Cache { get; }
        public Task<V> GetValueAsync(K key, Func<Task<V>> fillTask=null);
        public Task SetValueAsync(K key, V value);
    }
}
