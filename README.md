#ScotchBot

The main bulk of this is the library itself, the console is just an example to use it.

The bot will not connect to slack until you register a bot integration with your team.

> The console app in its current form here will most likely not compile until you add the following app.config
> You're responsible for managing your own front end. I may attempt to provide support if needed, but ultimately I have no obligation to help you.

Once you have done so and received a bot api key (they usually start with xoxb-), you'll need to create an App.config file within the ScotchConsole directory with the following:

```XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6" />
  </startup>
  <appSettings>
    <add key="ApiKey" value="<YOUR BOT API KEY KERE>" />
  </appSettings>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

Additionally, the library currently has support for performing YouTube searches. At this point in time, it is hard coded to return the absolute first video result returned from the YouTube API, or null if nothing is found.

To use this you will need to obtain a *Server Key* for YouTube Data API v3 from the Google Developer console. Once you have this key, add it to `<appSettings>` within the Console projects app.config (or your related location depending on your implementation.) using the key name `YouTubeApiKey`.

For completeness, this is the skeleton for the App.config file currently used on the live version of the bot:

```XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6" />
  </startup>
  <appSettings>
    <add key="ApiKey" value="<YOUR BOT API KEY KERE>" />
    <add key="YouTubeApiKey" value="<YOUR YOUTUBE API SERVER KEY HERE>" />
  </appSettings>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

#Scripting

This bot currently supports scripting.

Scripts for the console version are expected to be found within a `Scripts` folder, in the same location as the executable for the bot. You are free to change this as you see fit, just ensure that you pass the scripts directory to the library.

Scripts are in the format of `.csx`, and the name must match the slack event you wish for it to act on.

Additionally, the slack event must already have a defined Model class within the `Models` directory within the library. If the model for the event you wish to respond to isn't there (it probably isn't), then please feel free to convert the JSON of the event to a class, and ensure it uses the base class of `SlackMessage`.

The following is an example script that acts on a message sent to a slack channel, that searches YouTube for a given term and replies to the channel with the first result, or doesn't reply if nothing is found.
The command to use is `!yt <searchtermhere>`

```csharp
using ScotchBotLib.Models;
using ScotchBotLib.Utilities;
using ScotchBotLib.Utilities.Models;
using System;
using Newtonsoft.Json;
using WebSocketSharp;

public static bool HasProperty(this object objectToCheck, string propertyName) {
	var type = objectToCheck.GetType();
	return type.GetProperty(propertyName) != null;
}

void ScriptMethod(SlackMessage message, LogonEventMessage Channel, WebSocket ws) {
	try {
		if(message.HasProperty("text")) {
			Message receivedMessage = message as ScotchBotLib.Models.Message;
			if(receivedMessage.text.StartsWith("!yt")) {
				string searchText = receivedMessage.text.Remove(0, 3).Trim();
				YouTube yt = new YouTube();
				YouTubeResult ytr = yt.Search(searchText);
				string msg = JsonConvert.SerializeObject(new {
					type = "message",
					channel = receivedMessage.channel,
					text = "https://www.youtube.com/watch?v=" + ytr.VideoId
				});
				ws.Send(msg);
			}
		}
	} catch(Exception ex) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("[From script] Script exception:");
		Console.WriteLine(ex.GetBaseException().Message);
		Console.ForegroundColor = ConsoleColor.White;
	}
}
ScriptMethod(Message, Channel, ws)
```

*All* Scripts must have a method with the signature of `void ScriptMethod(SlackMessage message, LogonEventMessage Channel, WebSocket ws)` and *all* scripts must have `ScriptMethod(Message, Channel, ws)` at the end in order to work correctly.

Scripts that do not compile correctly will output the compile error, as if the error was a build message. In this case, the error is output to the console.
