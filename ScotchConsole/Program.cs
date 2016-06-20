using ScotchBotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ScotchConsole {
	class Program {

		private static string _scriptDirectory = "./Scripts";

		static void Main(string[] args) {
			SetupBot();
			ScotchBot bot = new ScotchBot();
			bot.PreFetch(ConfigurationManager.AppSettings["ApiKey"]);
			bot.LoadScripts(_scriptDirectory);
			//bot.CallScript("TestScript");
			Console.ReadLine();
		}

		private static void SetupBot() {
			if(Directory.Exists(_scriptDirectory)) {
				Console.WriteLine("Script directory already exists. Skipping.");
			} else {
				DirectoryInfo di = Directory.CreateDirectory(_scriptDirectory);
				Console.WriteLine("Created script directory at {0}.", Directory.GetCreationTime(_scriptDirectory));
			}
		}
	}
}
