using System;
using System.Collections.Generic;
using System.Linq;
using Californium;
using HumanityAgainstCards.Entities;
using HumanityAgainstCards.Network;
using HumanityAgainstCards.Network.Packets;
using SFML.Graphics;
using SFML.Window;

namespace HumanityAgainstCards.States
{
	class InGame : State
	{
		private string chatValue;
		private bool chatVisible;

		private readonly MenuButton endTurnButton;

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
			Input.Text = InputText;

			chatVisible = false;
			chatValue = "";

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

			endTurnButton = new MenuButton(new Vector2f(GameOptions.Width - 132.0f, GameOptions.Height - 244.0f), "End Turn");
			endTurnButton.OnClick += () =>
			{
				if (Game.PeekState().GetType() != typeof(InGame))
					return;

				if (SelectedCards.Count != Client.CurrentBlackCard.Info.PickCount || !Client.InMatch)
					return;

				LocalPlayer.Thinking = false;
				Client.InMatch = false;
				Client.SendMessage(new EndTurn(SelectedCards));
				RemoveSelectedWhiteCards();
			};

			Entities.Add(endTurnButton);

			// Play match start sound
			string startSoundFilename = "Start" + new Random().Next(5).ToString("G") + ".wav";
			Timer.NextFrame(() => Assets.PlaySound(startSoundFilename));
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
			rt.Draw(new RectangleShape(new Vector2f(GameOptions.Width - 16, GameOptions.Height - 16))
					{
						Position = new Vector2f(8, 8),
						OutlineColor = Color.Black,
						FillColor = Color.White,
						OutlineThickness = 8
					});

			rt.Draw(new RectangleShape(new Vector2f(1264.0f, 202.0f))
					{
						Position = new Vector2f(8.0f, GameOptions.Height - 194.0f - 16.0f),
						FillColor = Color.Black
					});

			rt.Draw(new RectangleShape(new Vector2f(256.0f, 63.0f))
					{
						Position = new Vector2f(GameOptions.Width - 256.0f - 8.0f, 8.0f),
						FillColor = Color.Black
					});

			rt.Draw(new RectangleShape(new Vector2f(8.0f, 502.0f))
					{
						Position = new Vector2f(GameOptions.Width - 256.0f - 8.0f, 8.0f),
						FillColor = Color.Black
					});

			string timerValue = String.Format("{0}:{1}", (Client.SecondsLeft / 60).ToString("D"),
											  (Client.SecondsLeft % 60).ToString("D").PadLeft(2, '0'));
			Text text = new Text(timerValue, Assets.LoadFont("QuartzMS.ttf"))
			{
				Position = new Vector2f(GameOptions.Width - 128.0f - 8.0f, 40.0f),
				CharacterSize = 48
			};

			text.Center();
			text.Origin = new Vector2f((float)Math.Round(text.Origin.X), (float)Math.Round(text.Origin.Y));
			text.Color = Color.White;
			rt.Draw(text);

			// Draw decks
			rt.Draw(new Sprite(Assets.LoadTexture("Decks.png"))
					{
						Position = new Vector2f(GameOptions.Width / 2.0f - 31.0f - 128.0f - 70.0f, GameOptions.Height / 2.0f - 44.0f - 89.0f - 22.0f)
					});

			if (chatVisible)
			{
				RectangleShape chatMessageBacking = new RectangleShape(new Vector2f(GameOptions.Width - 256.0f - 16.0f, 12.0f + 5.0f))
				{
					FillColor = new Color(0, 0, 0, 128),
					Position = new Vector2f(8.0f, 8.0f)
				};

				rt.Draw(chatMessageBacking);

				Text chatMessageText = new Text(chatValue + "|", Assets.LoadFont(Program.DefaultFont))
				{
					Position = new Vector2f(8.0f + 6.0f, 8.0f),
					Color = Color.White,
					CharacterSize = 12
				};

				rt.Draw(chatMessageText);
			}

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
			if (chatVisible)
			{
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
						ChatMessage message = new ChatMessage(chatValue);
						Client.SendMessage(message);

						LocalPlayer.SetMessage(chatValue);
						chatValue = "";
					}

					chatVisible = false;
					return true;
				}

				if (chatValue.Length < 80)
					chatValue += args.Text;
				return true;
			}

			if (args.Text == "\r")
				chatVisible = !chatVisible;

			return false;
		}
	}
}
