using System;
using System.Collections.Generic;
using System.Configuration;
using StackExchange.Redis;

namespace KursAM2.Repositories.RedisRepository
{
    public class RedisStore
    {
        private static Lazy<ConnectionMultiplexer> LazyConnection;

        private static readonly IDictionary<RedisChannel, ISubscriber>
            mySubscribes = new Dictionary<RedisChannel, ISubscriber>();

        //static RedisStore()
        //{
        //    var configurationOptions = new ConfigurationOptions
        //    {
        //        EndPoints = { ConfigurationManager.AppSettings["redis.connection"] }
        //    };
        //    try
        //    {
        //        LazyConnection =
        //            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        public static ConnectionMultiplexer Connection
        {
            get
            {
                if (LazyConnection?.Value != null)
                    return LazyConnection.Value;
                var configurationOptions = new ConfigurationOptions
                {
                    EndPoints =
                    {
                        ConfigurationManager.AppSettings["redis.connection"],
                        
                    },
                    AbortOnConnectFail = false
                };

                LazyConnection =
                    new Lazy<ConnectionMultiplexer>(() =>
                    {
                        var mp = ConnectionMultiplexer.Connect(configurationOptions);
                        mp.ConnectionFailed += Mp_ConnectionFailed;
                        mp.ConnectionRestored += Mp_ConnectionRestored;
                        if (!mp.IsConnected) Console.WriteLine(@"Cannot connect to Redis.  Will retry.");
                        return mp;
                        //return ConnectionMultiplexer.Connect(configurationOptions);
                    });

                return LazyConnection?.Value;
            }
        }

        private static void Mp_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine($@"Redis connection restored at {DateTime.Now}");
        }

        private static void Mp_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine(@"Cannot connect to Redis.  Will retry.");
        }

        public static IDatabase RedisCache => Connection.GetDatabase();
    }
}
