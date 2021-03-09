using Core.Async;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Caches {
    public abstract class CacheAbs<K, V> : ICache<K, V> {

        protected AsyncLock _lock = new AsyncLock();

        public Dictionary<K, V> Cache { get; } = new Dictionary<K, V>();

        public async Task<V> GetValueAsync(K key, Func<Task<V>> fillTask = null) {
            V retVal = default;
            await _lock.LockAsync(async () => {
                if (!Cache.TryGetValue(key, out retVal)) {
                    if(fillTask != null) {
                        try {
                            retVal = await fillTask();
                            if(retVal != null) {
                                Cache[key] = retVal;
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception($"Unable to fill { GetType().Name } key: { key }.", ex);
                        }
                    }
                    else {
                        throw new ArgumentException($"Invalid argument for {nameof(GetValueAsync)}.", nameof(key));
                    }
                }
            });
            return retVal;
        }

        public async Task SetValueAsync(K key, V value) {
            if(value == null) {
                return;
            }
            await _lock.Lock(() => {
                Cache[key] = value;
            });
        }
    }
}
