using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Models {
	public class BotDetails {
		public int TimesCrashed { get; set; }
		public DateTime StartTime { get; set; }
		public IDatabase Redis { get; set; }
	}
}
