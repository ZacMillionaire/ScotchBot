using System;
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

namespace ScotchBotLib {

	public class Script {
		public string Source { get; set; }
		public string Name { get; set; }
	}

	public static class ScriptManager {

		private static string _scriptDirectory;
		private static List<Script> _scriptFiles;

		public static void LoadScripts(string ScriptSrc) {
			_scriptDirectory = ScriptSrc;
			_scriptFiles = Directory.GetFiles(_scriptDirectory, "*.csx")
				.Select(x =>
					new Script {
						Source = x,
						Name = Regex.Replace(x, _scriptDirectory + @"\\(.+?).csx", "$1")
					}
				)
				.ToList();
		}

		public class Host {
			// Example method exposed
			public void ExecuteHostMethod(string a) {
				Console.WriteLine("From script: " + a);
			}
			public SlackMessage Message { get; set; }
			public LogonEventMessage Channel { get; set; }
			public WebSocket ws { get; set; }
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
					Console.WriteLine("[SCRIPT] Executing "+ScriptName);
					var s = CSharpScript.Create(code: script, options: options, globalsType: typeof(Host));
					ScriptRunner<object> runner = s.CreateDelegate();
					Host h = new Host { Message = message, Channel = channelState, ws = ws };
					await runner(h);
				} catch(Exception ex) {

					// TODO: Make a helper function to do this probably
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Script failed to compile:");
					Console.WriteLine(ex.GetBaseException().Message);
					Console.ForegroundColor = ConsoleColor.White;
				}

			} else {
				Console.WriteLine("No script exists for: "+ScriptName);
			}

			// syntax tree explorer for usings, won't pick up methods directly accessed via fully qualified names though
			// so do that first before trying again
			//var a = CSharpSyntaxTree.ParseText(script);
			//var r = (CompilationUnitSyntax)a.GetRoot();
			//var u = r.DescendantNodes().Where(x => x.GetType().Equals(typeof(UsingDirectiveSyntax))).Select(x => x.GetText());
		}
	}
}
