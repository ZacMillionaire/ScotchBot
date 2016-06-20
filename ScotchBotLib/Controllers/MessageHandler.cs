using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScotchBotLib.Models;
using WebSocketSharp;
using System.Globalization;

namespace ScotchBotLib.Controllers {
	public static class MessageHandler {
		public static Type GetMessageType(MessageEventArgs message) {
			if(message.Type.ToString() == "hello") {
				return typeof(LogonEventMessage);
			}
			return typeof(SlackMessage);
		}

		public static Type ToInternalClassType(string TypeString) {
			string FullyQualifiedName = ToFullyQualifiedName(TypeString);
			Type type = Type.GetType(FullyQualifiedName);
			return type;
		}

		private static string ToFullyQualifiedName(string TypeString) {
			return CultureInfo.CurrentCulture
				.TextInfo
				.ToTitleCase(
					TypeString.ToLower()
						.Replace("_", " ")
				)
				.Replace(" ", string.Empty)
				.Insert(0, "ScotchBotLib.Models.");
		}

		public static void Test(SlackMessage m) {

		}
	}
}
