using Redis.Manager.CacheManager;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace Redis.Manager.RedisManager
{
    public class RedisCacheManager : CacheHelper, ICacheManager
    {
        private static string host = "localhost";
        private static int port = 6379;
        private static IDatabase _db;

        public RedisCacheManager(int defaultDataBase = 0)
        {
            RedisCacheManager.CreateRedisDB(defaultDataBase, "", 0);
        }

        public RedisCacheManager(string ipAddres, int port, int defaultDataBase = 0)
        {
            RedisCacheManager.CreateRedisDB(defaultDataBase, ipAddres, port);
        }

        private static IDatabase CreateRedisDB(int defaultDb, string _host = "", int _port = 0)
        {
            if (RedisCacheManager._db == null)
            {
                RedisCacheManager.host = !string.IsNullOrEmpty(_host) ? _host : ConfigurationManager.AppSettings["RedisHost"];
                RedisCacheManager.port = _port > 0 ? _port : int.Parse(ConfigurationManager.AppSettings["RedisPort"]);
                RedisCacheManager._db = ConnectionMultiplexer.Connect(new ConfigurationOptions()
                {
                    Ssl = false,
                    EndPoints = {
            {
              RedisCacheManager.host,
              RedisCacheManager.port
            }
          },
                    DefaultDatabase = new int?(defaultDb)
                }, (TextWriter)null).GetDatabase(-1, (object)null);
            }
            return RedisCacheManager._db;
        }

        public void Clear()
        {
            foreach (RedisKey key in RedisCacheManager._db.Multiplexer.GetServer(RedisCacheManager.host, RedisCacheManager.port, (object)null).Keys(0, new RedisValue(), 250, 0L, 0, CommandFlags.None))
                RedisCacheManager._db.KeyDelete(key, CommandFlags.None);
        }

        public T Get<T>(string key)
        {
            RedisValue[] values = RedisCacheManager._db.SetMembers((RedisKey)key, CommandFlags.None);
            if (values.Length == 0)
                return default(T);
            try
            {
                return this.Deserialize<T>(values.ToStringArray());
            }
            catch (Exception ex)
            {
                throw new Exception(values.ToStringArray().ToString(), ex);
            }
        }

        public List<RedisDbModel> GetKeys()
        {
            if (RedisCacheManager._db == null)
                return new List<RedisDbModel>();
            List<RedisDbModel> redisDbModelList = new List<RedisDbModel>();
            EndPoint endpoint = ((IEnumerable<EndPoint>)RedisCacheManager._db.Multiplexer.GetEndPoints(false)).First<EndPoint>();
            int databaseCount = RedisCacheManager._db.Multiplexer.GetServer(endpoint, (object)null).DatabaseCount;
            for (int database = 0; database < databaseCount; ++database)
            {
                RedisKey[] array = RedisCacheManager._db.Multiplexer.GetServer(endpoint, (object)null).Keys(database, new RedisValue(), 250, 0L, 0, CommandFlags.None).ToArray<RedisKey>();
                if (array != null && (uint)array.Length > 0U)
                {
                    RedisDbModel redisDbModel = new RedisDbModel();
                    redisDbModel.DbNma = database.ToString();
                    redisDbModel.Keys = new List<string>();
                    foreach (RedisKey redisKey in array)
                        redisDbModel.Keys.Add(redisKey.ToString());
                    redisDbModelList.Add(redisDbModel);
                }
            }
            return redisDbModelList;
        }

        public List<string> GetKeys(string dbName)
        {
            List<string> stringList = new List<string>();
            EndPoint endpoint = ((IEnumerable<EndPoint>)RedisCacheManager._db.Multiplexer.GetEndPoints(false)).First<EndPoint>();
            RedisKey[] array = RedisCacheManager._db.Multiplexer.GetServer(endpoint, (object)null).Keys(int.Parse(dbName), new RedisValue(), 250, 0L, 0, CommandFlags.None).ToArray<RedisKey>();
            if (array != null && (uint)array.Length > 0U)
            {
                foreach (RedisKey redisKey in array)
                    stringList.Add(redisKey.ToString());
            }
            return stringList;
        }

        public bool IsSet(string key)
        {
            return RedisCacheManager._db.KeyExists((RedisKey)key, CommandFlags.None);
        }

        public bool Remove(string key)
        {
            return RedisCacheManager._db.KeyDelete((RedisKey)key, CommandFlags.None);
        }

        public bool Remove(string db, string key)
        {
            return ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                Ssl = false,
                EndPoints = {
          {
            RedisCacheManager.host,
            RedisCacheManager.port
          }
        },
                DefaultDatabase = new int?(int.Parse(db))
            }, (TextWriter)null).GetDatabase(-1, (object)null).KeyDelete((RedisKey)key, CommandFlags.None);
        }

        public void RemoveByPattern(string pattern)
        {
            foreach (RedisKey key in RedisCacheManager._db.Multiplexer.GetServer(RedisCacheManager.host, RedisCacheManager.port, (object)null).Keys(0, (RedisValue)("*" + pattern + "*"), 250, 0L, 0, CommandFlags.None))
                RedisCacheManager._db.KeyDelete(key, CommandFlags.None);
        }

        public void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;
            byte[] numArray = this.Serialize(data);
            RedisCacheManager._db.SetAdd((RedisKey)key, (RedisValue)numArray, CommandFlags.None);
            TimeSpan timeSpan = TimeSpan.FromMinutes((double)cacheTime);
            if (cacheTime <= 0)
                return;
            RedisCacheManager._db.KeyExpire((RedisKey)key, new TimeSpan?(timeSpan), CommandFlags.None);
        }
    }
}
