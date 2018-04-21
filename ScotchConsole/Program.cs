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
using GDQLib;

namespace ScotchConsole {
	class Program {

		private static string _scriptDirectory = "./Scripts";

		static void Main(string[] args) {
			
			/*
			GDQ.GetCurrentGame(DateTime.UtcNow);
			GDQ.GetCurrentGame(new DateTime(2017, 1, 9, 2, 30, 00));
			GDQ.GetCurrentGame(new DateTime(2017, 1, 9, 3, 00, 00));
			GDQ.GetCurrentGame(new DateTime(2017, 1, 9, 3, 30, 00));
			GDQ.GetCurrentGame(new DateTime(2017, 1, 9, 3, 43, 00));
			GDQ.GetCurrentGame(new DateTime(2017, 1, 11, 12, 15, 00));
			Console.ReadLine();
			*/

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
