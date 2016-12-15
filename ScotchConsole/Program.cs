using ScotchBotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ScotchBotLib.Utilities.Logging;
using System.Net.WebSockets;

namespace ScotchConsole {
	class Program {

		private static string _scriptDirectory = "./Scripts";

		static void Main(string[] args) {
			SetupBot();
			ScotchBot bot = new ScotchBot(ConfigurationManager.AppSettings["ApiKey"]);
			bot.Connect();
			bot.LoadScripts(_scriptDirectory);
			//bot.CallScript("TestScript");
			Console.ReadLine();
		}

		private static void SetupBot() {
			if(Directory.Exists(_scriptDirectory)) {
				Logging.Log("Script directory already exists. Skipping.");
				//Console.WriteLine("Script directory already exists. Skipping.");
			} else {
				DirectoryInfo di = Directory.CreateDirectory(_scriptDirectory);
				Logging.Log(string.Format("Created script directory at {0}.", Directory.GetCreationTime(_scriptDirectory)));
				//Console.WriteLine("Created script directory at {0}.", Directory.GetCreationTime(_scriptDirectory));
			}
		}
	}
}
