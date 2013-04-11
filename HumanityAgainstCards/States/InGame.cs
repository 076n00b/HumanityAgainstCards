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
	class InGame : State
	{
		private const int ChatBacklogItems = 24;

		private string chatValue;
		private Button endTurnButton;
		private bool cursorVisible;

		public List<string> ChatBacklog;

		public List<Player> Players
		{
			get
			{
				return Entities.OfType<Player>().ToList();
			}
		}

		public Player LocalPlayer
		{
			get
			{
				return Players.First(p => p.IsLocalPlayer);
			}
		}

		private List<CardInfo> SelectedCards
		{
			get
			{
				return Client.Hand.Where(c => c.Selected).OrderBy(c => c.Index).Select(c => c.Info).ToList();
			}
		}

		public InGame(IEnumerable<Player> players)
		{
			ChatBacklog = new List<string>();

			Input.Text = InputText;
			chatValue = "";
			cursorVisible = true;

			// Add every player from lobby
			foreach (Player p in players)
				Entities.Add(p);

			// Set local player position first
			LocalPlayer.Position = new Vector2f(GameOptions.Width / 2.0f - 128.0f - 8.0f, GameOptions.Height - 194.0f - 8.0f - 64.0f);

			// Add everyone else
			Vector2f otherPosition = new Vector2f(GameOptions.Width / 2.0f - ((128.0f + 32.0f) * (Players.Count - 2)) / 2.0f - 128.0f - 8.0f, 100.0f + 32.0f + 8.0f);
			foreach (Player p in Players.Where(p => !p.IsLocalPlayer))
			{
				p.Position = otherPosition;

				otherPosition += new Vector2f(128.0f + 32.0f, 0.0f);
			}

			// Play match start sound
			string startSoundFilename = "Start" + new Random().Next(5).ToString("G") + ".wav";
			Timer.NextFrame(() => Assets.PlaySound(startSoundFilename));

			Timer.Every(0.65f, () =>
			{
				cursorVisible = !cursorVisible;
				return false;
			});
		}

		public override void Enter()
		{
			endTurnButton = new Button(new Vector2f(GameOptions.Width - 132.0f, GameOptions.Height - 244.0f), "End Turn");
			endTurnButton.OnClick += () =>
			{
				if (Client.InMatch && SelectedCards.Count != Client.CurrentBlackCard.Info.PickCount)
				{
					string errorMessage = String.Format("Select {0} {1} before ending turn, moron.",
														Client.CurrentBlackCard.Info.PickCount,
														Client.CurrentBlackCard.Info.PickCount == 1 ? "card" : "cards");

					Game.PushState(new PopupOverlay(errorMessage));

					return false;
				}

				if (!Client.InMatch)
					return false;

				LocalPlayer.Thinking = false;
				Client.InMatch = false;
				Client.SendMessage(new EndTurn(SelectedCards));
				RemoveSelectedWhiteCards();

				return true;
			};

			Entities.Add(endTurnButton);

			base.Enter();
		}

		public override void Update()
		{
			if (LocalPlayer.Czar)
			{
				endTurnButton.Visible = false;
				foreach (Card card in Client.Hand)
					card.Selected = false;
			}

			endTurnButton.Visible = !(LocalPlayer.Czar || !Client.InMatch);

			base.Update();
		}

		public override void Draw(RenderTarget rt)
		{
			// Outline
			rt.Draw(new RectangleShape(new Vector2f(GameOptions.Width - 16, GameOptions.Height - 16))
					{
						Position = new Vector2f(8, 8),
						OutlineColor = Color.Black,
						FillColor = Color.White,
						OutlineThickness = 8
					});

			// Draw chat backlog
			float y = 0.0f;
			for (int i = ChatBacklog.Count - 1; i > ChatBacklog.Count - ChatBacklogItems && i != -1; --i)
			{
				Text itemText = GameUtility.Wrap(ChatBacklog[i], Assets.LoadFont(Program.DefaultFont), 18,
					GameOptions.Width / 3.0f, false);

				y -= 20.0f * itemText.DisplayedString.Count(c => c == '\n');
				itemText.Position = new Vector2f(16.0f, GameOptions.Height - 226.0f - 24.0f - 24.0f + y + 24.0f);
				itemText.Color = new Color(128, 128, 128);

				rt.Draw(itemText);
			}

			Text chatMessageText = new Text(chatValue, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(8.0f + 6.0f, GameOptions.Height - 220.0f - 24.0f),
				Color = Color.Black,
				CharacterSize = 22
			};

			rt.Draw(chatMessageText);

			if (cursorVisible)
			{
				RectangleShape cursor = new RectangleShape(new Vector2f(2.0f, 24.0f))
				{
					Position = chatMessageText.Position + new Vector2f(chatMessageText.GetWidth() + 2.0f, 2.0f),
					FillColor = Color.Black
				};

				cursor.Round();
				rt.Draw(cursor);
			}

			// White card table
			rt.Draw(new RectangleShape(new Vector2f(1264.0f, 202.0f))
					{
						Position = new Vector2f(8.0f, GameOptions.Height - 194.0f - 16.0f),
						FillColor = Color.Black
					});

			// Timer table
			rt.Draw(new RectangleShape(new Vector2f(256.0f, 63.0f))
					{
						Position = new Vector2f(GameOptions.Width - 256.0f - 8.0f, 8.0f),
						FillColor = Color.Black
					});

			// Table border
			rt.Draw(new RectangleShape(new Vector2f(8.0f, 502.0f))
					{
						Position = new Vector2f(GameOptions.Width - 256.0f - 8.0f, 8.0f),
						FillColor = Color.Black
					});

			// Timer text
			string timerValue = String.Format("{0}:{1}", (Client.SecondsLeft / 60).ToString("D"),
											  (Client.SecondsLeft % 60).ToString("D").PadLeft(2, '0'));
			Text text = new Text(timerValue, Assets.LoadFont("QuartzMS.ttf"))
			{
				Position = new Vector2f(GameOptions.Width - 128.0f - 8.0f, 40.0f),
				Color = Color.White,
				CharacterSize = 48
			};

			text.Center();
			text.Round();
			rt.Draw(text);

			// Draw decks
			rt.Draw(new Sprite(Assets.LoadTexture("Decks.png"))
					{
						Position = new Vector2f(GameOptions.Width / 2.0f - 31.0f - 128.0f - 70.0f, GameOptions.Height / 2.0f - 44.0f - 89.0f - 22.0f)
					});

			base.Draw(rt);
		}

		private void RemoveSelectedWhiteCards()
		{
			List<Card> toRemove = Client.Hand.Where(c => c.Selected).ToList();

			if (Client.CurrentBlackCard != null)
			{
				if (Client.CurrentBlackCard.Info.PickCount != toRemove.Count)
					return;
			}

			foreach (Card c in toRemove)
			{
				Client.Hand.Remove(c);
				Entities.Remove(c);
			}
		}

		private bool InputText(TextInputArgs args)
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
					if (!ParseCommand(chatValue))
					{
						string value = Client.Name + ": " + chatValue;

						ChatMessage message = new ChatMessage(value);
						Client.SendMessage(message);

						ChatBacklog.Add(value);

						if (!GameUtility.PlayTaunt(value))
							Assets.PlaySound("Bubble.wav");
					}

					chatValue = "";
				}

				return true;
			}

			if (chatValue.Length < 80)
				chatValue += Regex.Replace(args.Text, "[\x01-\x1F]", "");

			return true;
		}

		private bool ParseCommand(string value)
		{
			string[] values = value.Split(new[] { ' ' });
			if (value.Length < 2)
				return false;

			switch (values[0])
			{
				case "/kick":
					KickCommand(values[1]);
					return true;

				case "/mute":
					ChatBacklog.Add("Muted sounds!");
					GameOptions.SoundVolume = 0;
					return true;

				case "/unmute":
					ChatBacklog.Add("Unmuted sounds!");
					GameOptions.SoundVolume = 100;
					return true;
			}

			return false;
		}

		private void KickCommand(string value)
		{
			if (Server.State == Server.States.NotRunning)
			{
				ChatBacklog.Add("You are not host, you cannot kick.");
				return;
			}

			foreach (ServerClient sc in Server.Clients.Where(sc => sc.Name.ToLower() == value.ToLower()))
			{
				ChatBacklog.Add("Kicked player!");
				sc.Disconnect("Fuck off");
			}
		}
	}
}
