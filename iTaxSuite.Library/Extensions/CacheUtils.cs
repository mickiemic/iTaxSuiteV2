using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Extensions
{
    public static class RedisHelper
    {
        public static T GetOrDefault<T>(this IDatabase _database, string redisKey)
        {
            if (_database == null || !_database.KeyExists(redisKey))
            {
                return default(T);
            }
            string _strValue = _database.StringGet(redisKey);
            return string.IsNullOrWhiteSpace(_strValue) ? default(T) : JsonConvert.DeserializeObject<T>(_strValue);
        }

        public static async Task<T> GetOrDefaultAsync<T>(this IDatabase _database, string redisKey)
        {
            if (_database == null || !_database.KeyExists(redisKey))
            {
                return default(T);
            }
            string _strValue = await _database.StringGetAsync(redisKey);
            return string.IsNullOrWhiteSpace(_strValue) ? default(T) : JsonConvert.DeserializeObject<T>(_strValue);
        }
        public static T GetHashOrDefault<T>(this IDatabase _database, string redisKey, string hashKey)
        {
            if (_database == null || !_database.HashExists(redisKey, hashKey))
            {
                return default(T);
            }
            string _strValue = _database.HashGet(redisKey, hashKey);
            return string.IsNullOrWhiteSpace(_strValue) ? default(T) : JsonConvert.DeserializeObject<T>(_strValue);
        }
        public static async Task<T> GetHashOrDefaultAsync<T>(this IDatabase _database, string redisKey, string hashKey)
        {
            if (_database == null || !_database.HashExists(redisKey, hashKey))
            {
                return default(T);
            }
            string _strValue = await _database.HashGetAsync(redisKey, hashKey);
            return string.IsNullOrWhiteSpace(_strValue) ? default(T) : JsonConvert.DeserializeObject<T>(_strValue);
        }

        public static async Task<string> StrGetOrDefaultAsync(this IDatabase _database, string redisKey)
        {
            if (_database == null || !_database.KeyExists(redisKey))
            {
                return string.Empty;
            }
            string _strValue = await _database.StringGetAsync(redisKey);
            return string.IsNullOrWhiteSpace(_strValue) ? string.Empty : _strValue;
        }
        public static bool SetValue<T>(this IDatabase _database, string redisKey, T objValue, int expiry = 0)
        {
            string _strValue = JsonConvert.SerializeObject(objValue);
            if (expiry == 0)
                return _database.StringSet(redisKey, _strValue);
            else
            {
                TimeSpan _valueExpiry;
                if (expiry <= 0)
                {
                    _valueExpiry = TimeSpan.FromHours(72);
                }
                else
                {
                    _valueExpiry = TimeSpan.FromSeconds(expiry);
                }
                return _database.StringSet(redisKey, _strValue, _valueExpiry);
            }
        }

        public static async Task<bool> SetValueAsync<T>(this IDatabase _database, string redisKey, T objValue, int expiry = 0)
        {
            string _strValue = JsonConvert.SerializeObject(objValue);
            if (expiry == 0)
                return await _database.StringSetAsync(redisKey, _strValue);
            else
            {
                TimeSpan _valueExpiry;
                if (expiry < 0)
                {
                    _valueExpiry = TimeSpan.FromHours(72);
                }
                else
                {
                    _valueExpiry = TimeSpan.FromSeconds(expiry);
                }
                return await _database.StringSetAsync(redisKey, _strValue, _valueExpiry);
            }
        }

        public static bool SetHashValue<T>(this IDatabase _database, string redisKey, RedisValue hashField, T objValue, int expiry = 0)
        {
            string _strValue = JsonConvert.SerializeObject(objValue, Formatting.None,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            bool result = _database.HashSet(redisKey, hashField, _strValue);
            if (expiry != 0)
            {
                TimeSpan _valueExpiry;
                if (expiry < 0)
                {
                    _valueExpiry = TimeSpan.FromHours(72);
                }
                else
                {
                    _valueExpiry = TimeSpan.FromSeconds(expiry);
                }
                return _database.KeyExpire(redisKey, _valueExpiry);
            }
            if (!result)
            {
                var keys = _database.HashKeys(redisKey);
                result = keys.Contains(hashField);
            }

            return result;
        }
        public static async Task<bool> SetHashValueAsync<T>(this IDatabase _database, string redisKey, RedisValue hashField, T objValue, int expiry = 0)
        {
            string _strValue = JsonConvert.SerializeObject(objValue, Formatting.None,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            bool result = await _database.HashSetAsync(redisKey, hashField, _strValue);
            if (expiry != 0)
            {
                TimeSpan _valueExpiry;
                if (expiry < 0)
                {
                    _valueExpiry = TimeSpan.FromHours(72);
                }
                else
                {
                    _valueExpiry = TimeSpan.FromSeconds(expiry);
                }
                return await _database.KeyExpireAsync(redisKey, _valueExpiry);
            }
            if (!result)
            {
                var keys = await _database.HashKeysAsync(redisKey);
                result = keys.Contains(hashField);
            }

            return result;
        }

        public static async Task<RedisValue> SetStreamItemAsync(this IDatabase _database, string redisKey, string channelKey, string itemKey, string strObject)
        {
            return await _database.StreamAddAsync(redisKey, new NameValueEntry[]
                {
                    new NameValueEntry("Time", DateTime.Now.ToString("s")),
                    new NameValueEntry("Key", itemKey),
                    new NameValueEntry("Value", strObject),
                    new NameValueEntry("Channel", channelKey)
                });
        }
        public static Dictionary<string, string> ParseResult(StreamEntry entry)
        {
            return entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        }
    }

}
