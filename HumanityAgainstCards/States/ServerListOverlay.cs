using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Californium;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class ServerListOverlay : State
	{
		private const float PaddingHorizontal = 0.05f;
		private const float PaddingVertical = 0.05f;

		private List<Server> servers;
		private bool refreshing;
		private int listOffset, selectedIndex;

		public Server SelectedServer
		{
			get
			{
				return selectedIndex == -1 ? null : servers[selectedIndex];
			}
		}

		public ServerListOverlay()
		{
			servers = new List<Server>();
			refreshing = false;
			listOffset = 0;
			selectedIndex = -1;

			RefreshServerList();

			Input.MouseWheel += args =>
			{
				listOffset += args.Delta;

				if (listOffset >= servers.Count)
					listOffset = servers.Count - 1;

				if (listOffset < 0)
					listOffset = 0;

				return true;
			};

			Input.MouseButton[Mouse.Button.Left] = args =>
			{
				if (!args.Pressed)
					return false;

				float boundingY = GameOptions.Height * (PaddingVertical * 3.0f) + 64.0f;
				float boundingX = GameOptions.Width * (PaddingHorizontal * 2.5f);
				float width = GameOptions.Width - boundingX * 2.0f;
				const float height = 64.0f * 6;

				FloatRect area = new FloatRect(boundingX, boundingY, width, height);
				if (!area.Contains(args.Position.X, args.Position.Y))
				{
					selectedIndex = -1;
					return false;
				}

				selectedIndex = (int)(args.Position.Y - boundingY) / 64 + listOffset;
				if (selectedIndex >= servers.Count)
					selectedIndex = -1;

				if (SelectedServer != null)
					JoinNext(SelectedServer.IpAddress);

				return false;
			};
		}

		public override void Enter()
		{
			Button refreshButton =
				new Button(new Vector2f(GameOptions.Width * (PaddingHorizontal * 2.5f) + Button.Width / 2.0f, GameOptions.Height / 2.0f + GameOptions.Height / 3.0f + 16.0f), "Refresh");
			refreshButton.OnClick += () =>
			{
				RefreshServerList();
				return true;
			};

			Entities.Add(refreshButton);

			Button backButton =
				new Button(new Vector2f(GameOptions.Width - GameOptions.Width * (PaddingHorizontal * 2.5f) - Button.Width / 2.0f, GameOptions.Height / 2.0f + GameOptions.Height / 3.0f + 16.0f), "Back");
			backButton.OnClick += () =>
			{
				Game.PopState();
				return true;
			};

			Entities.Add(backButton);

			base.Enter();
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape overylay = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height))
			{
				FillColor = new Color(0, 0, 0, 128)
			};

			rt.Draw(overylay);

			RectangleShape window =
				new RectangleShape(new Vector2f(GameOptions.Width * (1.0f - PaddingHorizontal * 2.0f),
												GameOptions.Height * (1.0f - PaddingVertical * 2.0f)))
				{
					Position = new Vector2f(GameOptions.Width * PaddingHorizontal, GameOptions.Height * PaddingVertical),
					FillColor = Color.White,
					OutlineColor = Color.Black,
					OutlineThickness = 2.0f
				};

			rt.Draw(window);

			Text labelSettings = new Text("Server List", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * (PaddingVertical * 3.0f)),
				Color = Color.Black,
				CharacterSize = 48
			};

			labelSettings.Center();
			labelSettings.Round();
			rt.Draw(labelSettings);

			if (servers.Count == 0)
			{
				Text serverLabel = new Text(refreshing ? "Refreshing!" : "No servers found!", Assets.LoadFont(Program.DefaultFont), 36)
				{
					Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f),
					Color = Color.Black
				};

				serverLabel.Center();
				serverLabel.Round();
				rt.Draw(serverLabel);
			}
			else
			{
				float boundingY = GameOptions.Height * (PaddingVertical * 3.0f) + 64.0f;
				float boundingX = GameOptions.Width * (PaddingHorizontal * 2.5f);
				float yy = 0.0f;
				bool lightGray = true;

				for (int i = listOffset; i < listOffset + 6; ++i)
				{
					// Draw backing
					RectangleShape backing = new RectangleShape(new Vector2f(GameOptions.Width - boundingX * 2.0f, 64.0f))
					{
						Position = new Vector2f(boundingX, boundingY + (i - listOffset) * 64.0f),
						FillColor = lightGray ? new Color(192, 192, 192) : new Color(128, 128, 128)
					};

					if (i == selectedIndex)
						backing.FillColor = new Color(100, 149, 237);

					rt.Draw(backing);

					lightGray = !lightGray;

					// Draw server name
					if (i >= servers.Count)
						continue;

					Server server = servers[i];

					Text serverLabel = new Text(server.Name, Assets.LoadFont(Program.DefaultFont), 36)
					{
						Position = new Vector2f(boundingX + 12.0f, boundingY + yy + 32.0f),
						Color = Color.White
					};

					serverLabel.Center(false);
					serverLabel.Round();
					rt.Draw(serverLabel);

					// Draw player count
					Sprite playerAvatar = new Sprite(Assets.LoadTexture("Avatar.png"))
					{
						Position = new Vector2f(boundingX + GameOptions.Width - boundingX * 2.0f - 128.0f, boundingY + yy + 32.0f),
						Scale = new Vector2f(0.5f, 0.5f)
					};

					playerAvatar.Origin = new Vector2f(playerAvatar.TextureRect.Width / 2.0f, playerAvatar.TextureRect.Height / 2.0f);
					rt.Draw(playerAvatar);

					Text playerCountLabel = new Text(server.PlayerCount.ToString("G") + "/" + Constants.MaxPlayerCount.ToString("G"), Assets.LoadFont(Program.DefaultFont), 36)
					{
						Position = new Vector2f(boundingX + GameOptions.Width - boundingX * 2.0f - 128.0f + 32.0f, boundingY + yy + 32.0f),
						Color = Color.White
					};

					playerCountLabel.Center(false);
					playerCountLabel.Round();
					rt.Draw(playerCountLabel);

					yy += 64.0f;
				}
			}

			base.Draw(rt);
		}

		private void RefreshServerList()
		{
			Thread fetchThread = new Thread(FetchServerList);
			fetchThread.Start();
		}

		private void FetchServerList()
		{
			// Clear servers
			refreshing = true;
			servers.Clear();

			// Get server list data
			WebClient client = new WebClient();
			string jsonStream = Encoding.ASCII.GetString(client.DownloadData(Constants.RemoteServerListAddress + "get.php"));
			client.Dispose();

			// Decode JSON stream
			try
			{
				servers = Json.LoadStream<List<Server>>(jsonStream);
			}
			catch (Exception)
			{
				Console.WriteLine("No servers!");
			}

			refreshing = false;
		}

		private void JoinNext(string ip)
		{
			if (!String.IsNullOrEmpty(Client.Name) && !String.IsNullOrEmpty(ip))
				Game.SetState(new Lobby(SessionRole.Client, ip, Client.Name));
			else
			{
				if (String.IsNullOrEmpty(Client.Name))
				{
					Game.PushState(new PopupOverlay("You must enter a name before joining a game."));
					return;
				}

				if (String.IsNullOrEmpty(ip))
					Game.PushState(new PopupOverlay("You must enter an IP to join a game."));
			}
		}

		internal class Server
		{
			public string Name;
			public string IpAddress;
			public int PlayerCount;
			public bool PasswordProtected;

			public Server()
			{
				Name = "";
				IpAddress = "";
				PlayerCount = 0;
				PasswordProtected = false;
			}
		}
	}
}
