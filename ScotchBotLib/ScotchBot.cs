using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScotchBotLib.Controllers;
using ScotchBotLib.Models;
using ScotchBotLib.Utilities.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WebSocketSharp;

namespace ScotchBotLib {
    public class NestedDictionaryConverter : CustomCreationConverter<IDictionary<string, object>> {
        public override IDictionary<string, object> Create(Type objectType) {
            return new Dictionary<string, object>();
        }

        public override bool CanConvert(Type objectType) {
            // in addition to handling IDictionary<string, object>
            // we want to handle the deserialization of dict value
            // which is of type object
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if(reader.TokenType == JsonToken.StartObject
                || reader.TokenType == JsonToken.Null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            // if the next token is not an object
            // then fall back on standard deserializer (strings, numbers etc.)
            return serializer.Deserialize(reader);
        }
    }
    public class ScotchBot {
        private string _connectionUrl = "https://slack.com/api/rtm.start?token={0}";
        private LogonEventMessage _teamDetails;
        private BotDetails _botDetails;
        private WebSocket _ws;
        private readonly string _token;
        private static IDatabase _redisCache = RedisStore.RedisCache;

        public ScotchBot(string slackBotToken) {
            _token = slackBotToken;
            _botDetails = new BotDetails {
                TimesCrashed = 0,
                Redis = _redisCache
            };
        }

        public void Connect() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(this._connectionUrl, _token));
            request.Proxy = null;
            _botDetails.StartTime = DateTime.Now;
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
            this._ws.OnOpen += (sender, e) => Logging.Log("Connected");//Console.WriteLine("Connected");
            this._ws.OnClose += (sender, e) => Logging.Log("Disconnected");//Console.WriteLine("Disconnected");
            this._ws.OnError += (sender, e) => this.OnError(e);//Console.WriteLine("Error");
            this._ws.OnMessage += (sender, e) => this.OnMessage(e);
            this._ws.Connect();
        }

        public void LoadScripts(string ScriptSrc) {
            ScriptManager.LoadScripts(ScriptSrc, _botDetails);
        }

        public void CallScript(string ScriptName, SlackMessage message) {
            ScriptManager.ExecuteScript(ScriptName, message, _teamDetails, _ws);
        }

        private void OnMessage(MessageEventArgs e) {
            try {

                Dictionary<string, object> messageObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data, new JsonConverter[] { new NestedDictionaryConverter() });
                messageObject.Add("_timestamp", DateTime.UtcNow.ToString("o"));
                Type messageType = MessageHandler.ToInternalClassType(messageObject["type"] as string);

                if(messageType != null) {
                    if(messageType.Name != "ReconnectUrl") {
                        AddToList<string>(DateTime.UtcNow.AddSeconds(-59).ToString("yyyy-MM-dd:HH-00-00"), /*e.Data*/JsonConvert.SerializeObject(messageObject));
                    }
                    var message = (SlackMessage)JsonConvert.DeserializeObject(e.Data, messageType);
                    CallScript(message.GetType().Name, message);
                } else {
                    if(messageObject["type"] as string != "user_typing" && messageObject["type"] as string != "pong") {
                        AddToList<string>(DateTime.UtcNow.AddSeconds(-59).ToString("yyyy-MM-dd:HH-00-00"), /*e.Data*/JsonConvert.SerializeObject(messageObject));
                    }
                    Logging.Log("Unscripted Message Type: " + messageObject["type"]);
                }
                /*
				if(DateTime.Now.Minute % 15 == 0) {
					string msg = JsonConvert.SerializeObject(new {
						//id = 100,
						type = "ping",
						channel = _teamDetails.channels[0].id
					});
					_ws.Send(msg);
				}*/

            } catch(Exception ex) {
                // prevent throw

                // Slack returns a message with no type, but has a key called "reply_to",
                // 90% of the time currently that is what this exception is.
                // TODO: Make a helper function to do this probably

                Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine("Some exception happened (probably reply_to)");
                //Console.WriteLine(ex.GetBaseException().Message);
                Logging.Log("Some exception happened (probably reply_to)");
                Logging.Log(ex.GetBaseException().Message);
                Logging.Log(e.Data);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void OnError(WebSocketSharp.ErrorEventArgs e) {
            Console.ForegroundColor = ConsoleColor.Red;
            Logging.Log("Critical Web Socket Error:");
            Logging.Log(e.Exception.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            _botDetails.TimesCrashed += 1;
            Connect();
        }
        private static void AddToList<TSource>(string ListKey, string source/*IEnumerable<TSource> source, ushort TIL*/) {
            return;
            //var data = JsonConvert.SerializeObject(source);
            _redisCache.ListRightPush(ListKey, source);
            /*_redisCache.KeyExpire(key, new TimeSpan(0, 0, TIL));*/
        }
    }
}
