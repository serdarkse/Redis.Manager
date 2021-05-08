using System.Collections.Generic;

namespace Redis.Manager.RedisManager
{
    public class RedisDbModel
    {
        public string DbNma { get; set; }
        public List<string> Keys { get; set; }
    }
}
