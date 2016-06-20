using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Models {

	public class Message : SlackMessage {
		public string user { get; set; }
		public string inviter { get; set; }
		public string type { get; set; }
		public string subtype { get; set; }
		public string text { get; set; }
		public string channel { get; set; }
		public string ts { get; set; }
		public File file { get; set; }
		public bool upload { get; set; }
		public bool display_as_bot { get; set; }
		public string username { get; set; }
		public object bot_id { get; set; }
		public string item_type { get; set; }
		public Item item { get; set; }
	}
}
