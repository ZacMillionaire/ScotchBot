using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Controllers {
	public class RedisStore {
		private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

		private static Lazy<ConfigurationOptions> configOptions
		   = new Lazy<ConfigurationOptions>(() => {
			   var configOptions = new ConfigurationOptions();
#if( DEBUG )
			   configOptions.EndPoints.Add(ConfigurationManager.AppSettings["RedisEndpoint"]);
#else
			   configOptions.EndPoints.Add("localhost:6379");
#endif
			   configOptions.ClientName = "RedisCache";
			   configOptions.ConnectTimeout = 100000;
			   configOptions.SyncTimeout = 100000;
			   configOptions.AbortOnConnectFail = false;
			   configOptions.DefaultDatabase = 0;
			   configOptions.Password = ConfigurationManager.AppSettings["RedisPassword"];
			   return configOptions;
		   });

		static RedisStore() {
			LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configOptions.Value));
		}

		public static ConnectionMultiplexer Connection => LazyConnection.Value;

		public static IDatabase RedisCache => Connection.GetDatabase();
	}
}
