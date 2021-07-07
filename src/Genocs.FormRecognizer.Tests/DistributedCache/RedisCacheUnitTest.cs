using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Xunit;

namespace Genocs.FormRecognizer.Tests.DistributedCache
{
    public class RedisCacheUnitTest
    {
        [Fact (Skip = "ci")]
        public void CheckRedisConnection()
        {

            string server = "localhost";
            string port = "6379";
            string connectionString = $"{server}:{port}";

            var redisOptions = new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions
            {
                ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions() 
                {
                    Ssl = false,
                    AbortOnConnectFail = false, 
                    Password = ""
                }
            };
            redisOptions.ConfigurationOptions.EndPoints.Add(connectionString);
            var opts = Options.Create<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions>(redisOptions);

            IDistributedCache cache = new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache(opts);
            string expectedStringData = "1ca8195a-f5e6-41c1-83f3-034df7f3a6ff";
            cache.Set("d1fdb12d-c360-4e80-a7e8-75ff63971f0c", System.Text.Encoding.UTF8.GetBytes(expectedStringData));
            var dataFromCache = cache.Get("d1fdb12d-c360-4e80-a7e8-75ff63971f0c");
            var actualCachedStringData = System.Text.Encoding.UTF8.GetString(dataFromCache);
            Assert.Equal(expectedStringData, actualCachedStringData);

        }
    }
}
