using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Californium;
using ManateesAgainstCards.Entities;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using ManateesAgainstCards.Network.Packets;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class Lobby : State
	{
		private const int ChatBacklogItems = 21;

		public List<string> ChatBacklog;
		private string chatValue;
		private readonly string sessionIp;
		private readonly SessionRole sessionRole;
		private bool cursorVisible;

		public List<Player> Players
		{
			get
			{
				return Entities.OfType<Player>().ToList();
			}
		}

		private Player LocalPlayer
		{
			get
			{
				return Players.First(p => p.IsLocalPlayer);
			}
		}

		public Lobby(SessionRole role, string ip, string username)
		{
			Input.Text += TextInput;
			ClearColor = Color.White;

			ChatBacklog = new List<string>();
			chatValue = "";

			Client.Name = username;
			sessionRole = role;
			sessionIp = ip;

			cursorVisible = true;

			Timer.Every(0.65f, () =>
			{
				cursorVisible = !cursorVisible;
				return false;
			});
		}

		public override void Enter()
		{
			// Add local player
			Entities.Add(new Player(Client.Name, true));

			// Initialize session
			InitializeSession(sessionRole, sessionIp);

			base.Enter();
		}

		public override void Update()
		{
			// Update player position in list
			for (int i = 0; i < Players.Count; ++i)
				Players[i].Position = new Vector2f(32.0f, GameOptions.Height / 6.0f + 87.0f * i);

			base.Update();
		}

		public override void Draw(RenderTarget rt)
		{
			// Display connecting message if needed
			if (!Client.Connected)
			{
				RectangleShape bgShape = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height)) { FillColor = Color.Black };
				rt.Draw(bgShape);

				Text waitingText = new Text("Connecting", Assets.LoadFont(Program.DefaultFont))
				{
					Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f),
					CharacterSize = 48,
					Color = Color.White
				};

				waitingText.Center();
				waitingText.Round();

				rt.Draw(waitingText);

				return;
			}

			// Draw lobby text
			Text lobbyText = new Text("Lobby", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f),
				CharacterSize = 48,
				Color = Color.Black
			};

			lobbyText.Center();
			lobbyText.Round();

			rt.Draw(lobbyText);

			// Draw chat area
			RectangleShape chatArea = new RectangleShape(
				new Vector2f((GameOptions.Width / 3.0f) * 2.0f - 64.0f, (GameOptions.Height / 3.0f) * 2.0f + 64.0f))
			{
				Position = new Vector2f(GameOptions.Width / 3.0f, GameOptions.Height / 3.0f - 128.0f),
				FillColor = new Color(96, 96, 96, 255)
			};

			rt.Draw(chatArea);

			RectangleShape chatBarArea = new RectangleShape(new Vector2f((GameOptions.Width / 3.0f) * 2.0f - 64.0f, 48.0f))
			{
				Position = new Vector2f(GameOptions.Width / 3.0f, GameOptions.Height - 64.0f - 48.0f),
				FillColor = Color.Black
			};

			rt.Draw(chatBarArea);

			Text messageText = new Text(chatValue, Assets.LoadFont(Program.DefaultFont))
			{
				Position =
					new Vector2f(GameOptions.Width / 3.0f + 8.0f, GameOptions.Height - 64.0f - 48.0f + 8.0f + 4.0f),
				CharacterSize = 22,
				Color = Color.White
			};

			messageText.Round();
			rt.Draw(messageText);

			if (cursorVisible)
			{
				RectangleShape cursor = new RectangleShape(new Vector2f(2.0f, 24.0f))
				{
					Position = messageText.Position + new Vector2f(messageText.GetGlobalBounds().Width + 2.0f, 2.0f),
					FillColor = Color.White
				};

				cursor.Round();
				rt.Draw(cursor);
			}

			// Draw chat backlog
			float y = 0.0f;
			for (int i = ChatBacklog.Count - 1; i > ChatBacklog.Count - ChatBacklogItems && i != -1; --i)
			{
				Text itemText = GameUtility.Wrap(ChatBacklog[i], Assets.LoadFont(Program.DefaultFont), 22,
					(GameOptions.Width / 3.0f) * 2.0f - 64.0f - 32.0f);

				y -= 24.0f * itemText.DisplayedString.Count(c => c == '\n');
				itemText.Position = new Vector2f((float)Math.Floor(GameOptions.Width / 3.0f + 8.0f), GameOptions.Height - 144.0f + y + 24.0f);
				itemText.Color = Color.White;

				rt.Draw(itemText);
			}

			base.Draw(rt);
		}

		private bool TextInput(TextInputArgs args)
		{
			// 0x16 is Control + V
			if (args.Text[0] == 0x16)
			{
				chatValue += GameUtility.GetClipboardText();
				if (chatValue.Length >= 80)
					chatValue = chatValue.Remove(80, chatValue.Length - 80);

				return true;
			}

			if (args.Text == "\b")
			{
				if (chatValue.Length == 0)
					return true;

				chatValue = chatValue.Remove(chatValue.Length - 1);
				return true;
			}

			if (args.Text == "\r")
			{
				if (chatValue != "")
				{
					string value = LocalPlayer.Name + ": " + chatValue;
					ChatMessage message = new ChatMessage(value);
					Client.SendMessage(message);

					ChatBacklog.Add(value);
					GameUtility.PlayTaunt(value);
					chatValue = "";

					Assets.PlaySound("Bubble.wav");
				}

				return true;
			}

			if (chatValue.Length < 80)
				chatValue += Regex.Replace(args.Text, "[\x01-\x1F]", "");

			return true;
		}

		private void InitializeSession(SessionRole role, string ip)
		{
			switch (role)
			{
				case SessionRole.Server:
					{
						// Initialize server
						try
						{
							Server.Initialize(Constants.DefaultPort);
						}
						catch (Exception)
						{
							Timer.NextFrame(() => Game.SetState(new ErrorMessageScreen("You are already hosting in another instance, moron.")));
							return;
						}

						// Initialize client frontend
						try
						{
							Client.Connect("localhost", Constants.DefaultPort);
						}
						catch (Exception)
						{
							Timer.NextFrame(() => Game.SetState(new ErrorMessageScreen("Cannot connect to host!")));
							return;
						}

						// Add start button
						Button startButton = new Button(new Vector2f(GameOptions.Width - 222.0f / 2.0f - 8.0f - 64.0f + 8.0f, Button.Height / 2.0f + 8.0f + 32.0f + 8.0f), "Begin Game");
						startButton.OnClick += () =>
						{
							// Do not start unless we have enough players
							if (Players.Count < Constants.MinimalPlayerCount)
							{
								ChatBacklog.Add(String.Format("Cannot begin game with less than {0} people!", Constants.MinimalPlayerCount));
								return true;
							}

							// Load cards if we're the server
							Server.LoadCards();

							// Begin game
							Client.SendMessage(new BeginGame());

							return true;
						};

						Entities.Add(startButton);

						// Add settings button
						Button settingsButton = new Button(new Vector2f(GameOptions.Width - Button.Width / 2.0f - 8.0f - 64.0f + 8.0f - Button.Width - 4.0f, Button.Height / 2.0f + 8.0f + 32.0f + 8.0f), "Settings");
						settingsButton.OnClick += () =>
						{
							ServerList.Remove();
							Game.PushState(new HostSettingsOverlay());
							return true;
						};

						Entities.Add(settingsButton);

						ServerList.Add();

						break;
					}

				case SessionRole.Client:
					try
					{
						Client.Connect(ip, Constants.DefaultPort);
					}
					catch (Exception)
					{
						Timer.NextFrame(() => Game.SetState(new ErrorMessageScreen("Cannot connect to host!")));
					}
					break;
			}
		}
	}
}
