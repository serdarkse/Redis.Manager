using Redis.Manager.RedisManager;
using System.Collections.Generic;

namespace Redis.Manager.CacheManager
{
    public interface ICacheManager
    {
        T Get<T>(string key);

        void Set(string key, object data, int cacheTime);

        bool IsSet(string key);

        bool Remove(string key);

        void RemoveByPattern(string pattern);

        void Clear();

        List<RedisDbModel> GetKeys();

        List<string> GetKeys(string dbName);

        bool Remove(string db, string key);
    }
}
