using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScotchBotLib.Utilities.Logging {
	public static class Logging {

		private static string _logDirectory = ConfigurationManager.AppSettings["LoggingDirectory"];
		// add command to toggle this at runtime later
		private static string _loggingEnabled = ConfigurationManager.AppSettings["LoggingEnabled"];

		static Logging() {
			if(Directory.Exists(_logDirectory)) {
				Log("Log directory already exists.");
			} else {
				DirectoryInfo di = Directory.CreateDirectory(_logDirectory);
			}
		}

		public static void Log(string message) {

			try {
				if(_loggingEnabled == "true") {
					var thisLogFile = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + ".txt";

					using(StreamWriter w = File.AppendText(_logDirectory + "/" + thisLogFile)) {
						w.WriteLine("[" + DateTime.Now + "] " + message);
					}
				}
			} catch {
				Console.WriteLine("Error: Log file in use.");
			}
			Console.WriteLine(message);
		}
	}
}
