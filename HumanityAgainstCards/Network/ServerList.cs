using System;
using System.Net;
using System.Text;
using System.Threading;

namespace ManateesAgainstCards.Network
{
	static class ServerList
	{
		private static string token;

		private static string ServerName
		{
			get
			{
				return Client.Name;
			}
		}

		static ServerList()
		{
			token = "";
		}

		public static void Add()
		{
			token = "";

			new Thread(ServerListCommand).Start(new ServerListRequest("add", !String.IsNullOrEmpty(Server.Password)));

			Californium.Timer.Every(30.0f, () =>
			{
				if (Server.State == Server.States.Lobby)
				{
					new Thread(ServerListCommand).Start(new ServerListRequest("heartbeat"));
					return false;
				}

				Remove();

				return true;
			});
		}

		public static void Remove()
		{
			new Thread(ServerListCommand).Start("remove");
		}

		public static void Update()
		{
			new Thread(ServerListCommand).Start(new ServerListRequest("update", Server.Clients.Count));
		}

		private static void ServerListCommand(object command)
		{
			ServerListRequest request = (ServerListRequest)command;
			ServerListResponse response = new ServerListResponse();
			string address = request.GetAddress();

			WebClient client = new WebClient();
			string jsonStream = Encoding.ASCII.GetString(client.DownloadData(address));
			client.Dispose();

			// Decode JSON stream
			try
			{
				response = Json.LoadStream<ServerListResponse>(jsonStream);
			}
			catch (Exception e)
			{
				Console.WriteLine("ServerList error: {0}", e.Message);
			}

			if (!String.IsNullOrEmpty(response.Token))
			{
				Console.WriteLine("Private token: {0}", response.Token);
				token = response.Token;
			}

			Console.WriteLine("Status: {0}; Reason: {1}", response.Status, response.Reason);
		}

		internal class ServerListRequest
		{
			public string Command { get; private set; }
			//public string Name { get; private set; }
			//public string Token { get; private set; }
			public bool PasswordProtected { get; private set; }
			public int PlayerCount { get; private set; }

			public ServerListRequest(string command)
			{
				Command = command;
				//Name = name;
				//Token = null;
			}

			public ServerListRequest(string command, bool passwordProtected)
			{
				Command = command;
				PasswordProtected = passwordProtected;
			}

			public ServerListRequest(string command, int playerCount)
			{
				Command = command;
				//Token = token;
				//PasswordProtected = passwordProtected;
				PlayerCount = playerCount;
			}

			public string GetAddress()
			{
				string address = Constants.RemoteServerListAddress + Command + ".php";
				if (!String.IsNullOrEmpty(token))
					address += "?token=" + token;
				else
					address += "?name=" + ServerName;

				switch(Command)
				{
					case "add":
						address += "&passwordProtected=" + (PasswordProtected ? "1" : "0");
						break;

					case "update":
						address += "&playerCount=" + PlayerCount.ToString("G");
						break;
				}

				Console.WriteLine(address);
				
				return address;
			}
		}

		internal class ServerListResponse
		{
			public string Status;
			public string Reason;
			public string Token;

			public ServerListResponse()
			{
				Status = "";
				Reason = "";
				Token = "";
			}
		}
	}
}
