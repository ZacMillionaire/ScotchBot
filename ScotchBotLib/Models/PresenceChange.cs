using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Models {

	public class PresenceChange : SlackMessage {
		public string type { get; set; }
		public string user { get; set; }
		public string presence { get; set; }
	}

}
