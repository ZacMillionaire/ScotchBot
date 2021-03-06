﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using ScotchBotLib.Models;
using WebSocketSharp;
using ScotchBotLib.Utilities.Logging;
using StackExchange.Redis;
using ScotchBotLib.Controllers;
using System.Net;

namespace ScotchBotLib {

    public class Script {
        public string Source { get; set; }
        public string Name { get; set; }
    }

    public static class ScriptManager {

        private static string _scriptDirectory;
        private static List<Script> _scriptFiles;
        private static BotDetails _self;
        private static IDatabase _redisCache = RedisStore.RedisCache;

        public static void LoadScripts(string ScriptSrc, BotDetails self) {
            _scriptDirectory = ScriptSrc;
            _self = self;
            _scriptFiles = Directory.GetFiles(_scriptDirectory, "*.csx")
                .Select(x =>
                    new Script {
                        Source = x,
                        Name = Regex.Replace(x, _scriptDirectory + @"\\(.+?).csx", "$1")
                    }
                )
                .ToList();
        }

        private static Dictionary<string, object> ScriptCache = new Dictionary<string, object>();

        public class Host {
            // Example method exposed
            public void ExecuteHostMethod(string a) {
                Console.WriteLine("From script: " + a);
            }

            public BotDetails GetSelfStatus() {
                return _self;
            }
            public SlackMessage Message { get; set; }
            public LogonEventMessage Channel { get; set; }
            public WebSocket ws { get; set; }
            public Dictionary<string, object> Cache = ScriptCache;
            public void SendSlackMessage(string MessageJSON, string url) {
                WebRequest request = WebRequest.Create(url);
                request.ContentType = "application/json; charset=UTF-8;";
                request.Method = "POST";
                var data = Encoding.UTF8.GetBytes(MessageJSON);
                request.ContentLength = data.Length;
                using(Stream dataStream = request.GetRequestStream()) {
                    dataStream.Write(data, 0, data.Length);
                }

                WebResponse response = request.GetResponse();
            }

        }

        // I might want to expand on this later, even if it will most likely be a one liner
        private static bool ScriptExistsForClass(string ScriptName) {
            return _scriptFiles.Any(x => x.Name.Contains(ScriptName));
        }


        public static async void ExecuteScript(string ScriptName, SlackMessage message, LogonEventMessage channelState, WebSocket ws) {

            // The program will hard crash if the script fails to compile

            string script;
            // good way to restrict namespaces would be to include them at the top then include the content after
            // just gotta work out a way to figure out if a method is using a fully qualified name
            ScriptOptions options = ScriptOptions.Default.WithReferences(new System.Reflection.Assembly[] { typeof(SlackMessage).Assembly });

            if(ScriptExistsForClass(ScriptName)) {
                // load script from file and execute
                script = System.IO.File.ReadAllText(_scriptFiles.First(x => x.Name == ScriptName).Source);
                try {
                    Logging.Log("[SCRIPT] Executing " + ScriptName);
                    //Console.WriteLine("[SCRIPT] Executing "+ScriptName);
                    var s = CSharpScript.Create(code: script, options: options, globalsType: typeof(Host));
                    ScriptRunner<object> runner = s.CreateDelegate();
                    Host h = new Host { Message = message, Channel = channelState, ws = ws };
                    await runner(h);
                } catch(Exception ex) {

                    // TODO: Make a helper function to do this probably
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine("Script failed to compile:");
                    //Console.WriteLine(ex.GetBaseException().Message);
                    Logging.Log("Script failed to compile:");
                    Logging.Log(ex.GetBaseException().Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }

            } else {
                Console.WriteLine("No script exists for: " + ScriptName);
            }

            // syntax tree explorer for usings, won't pick up methods directly accessed via fully qualified names though
            // so do that first before trying again
            //var a = CSharpSyntaxTree.ParseText(script);
            //var r = (CompilationUnitSyntax)a.GetRoot();
            //var u = r.DescendantNodes().Where(x => x.GetType().Equals(typeof(UsingDirectiveSyntax))).Select(x => x.GetText());
        }
    }
}
