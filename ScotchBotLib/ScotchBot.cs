using Newtonsoft.Json;
using ScotchBotLib.Controllers;
using ScotchBotLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WebSocketSharp;

namespace ScotchBotLib {
	public class ScotchBot {
		private string _connectionUrl = "https://slack.com/api/rtm.start?token={0}";
		private LogonEventMessage _teamDetails;
		private WebSocket _ws;

		public void PreFetch(string slackBotToken) {

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(this._connectionUrl, slackBotToken));
			request.Proxy = null;

			using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
				using(Stream stream = response.GetResponseStream()) {
					string str = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
					this._teamDetails = JsonConvert.DeserializeObject<LogonEventMessage>(str);
					this.OpenWebsocket(this._teamDetails.url);
				}
			}
		}
		public void OpenWebsocket(string socketUrl) {
			this._ws = new WebSocket(socketUrl, new string[0]);
			this._ws.OnOpen += (sender, e) => Console.WriteLine("Connected");
			this._ws.OnClose += (sender, e) => Console.WriteLine("Disconnected");
			this._ws.OnError += (sender, e) => Console.WriteLine("Error");
			this._ws.OnMessage += (sender, e) => this.OnMessage(e);
			this._ws.Connect();
		}

		public void LoadScripts(string ScriptSrc) {
			ScriptManager.LoadScripts(ScriptSrc);
		}

		public void CallScript(string ScriptName, SlackMessage message) {
			ScriptManager.ExecuteScript(ScriptName, message, _teamDetails, _ws);
		}

		private void OnMessage(MessageEventArgs e) {
			try {
				Dictionary<string, string> messageObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.Data);


				Type messageType = MessageHandler.ToInternalClassType(messageObject["type"]);

				if(messageType != null) {
					var message = (SlackMessage)JsonConvert.DeserializeObject(e.Data, messageType);
					CallScript(message.GetType().Name, message);
				} else {
					Console.WriteLine("Unscripted Message Type: " + messageObject["type"]);
				}
			} catch(Exception ex) {
				// prevent throw

				// Slack returns a message with no type, but has a key called "reply_to",
				// 90% of the time currently that is what this exception is.
				// TODO: Make a helper function to do this probably
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("Some exception happened (probably reply_to)");
				Console.WriteLine(ex.GetBaseException().Message);
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
	}
}
