using System;
using System.Collections.Generic;
using System.Diagnostics;
using Californium;
using ManateesAgainstCards.Entities;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class MainMenu : State
	{
		private static readonly List<string> slogans = new List<string>
		{
			"wiglets",
			"the Jews",
			"Nick's Mom",
			"the Westboro Baptist Church",
			"the Pepsi Generation",
			"Facepunch",
			"people with monochrome displays",
			"Recreationists",
			"Justin Bieber Fangirls"
		};

		private readonly string personValue;
		private string persistentDisplayName;

		public MainMenu()
		{
			ClearColor = Color.White;
			persistentDisplayName = "";

			personValue = slogans[new Random(DateTime.Now.Millisecond).Next(slogans.Count)];
			MenuMain();
		}

		public override void Draw(RenderTarget rt)
		{
			// Draw header
			rt.Draw(new RectangleShape(new Vector2f(GameOptions.Width, 350.0f)) { FillColor = Color.Black });

			Sprite logoSprite = new Sprite(Assets.LoadTexture("Logo.png"))
			{
				Position = new Vector2f(GameOptions.Width / 6.0f, (350.0f - 256.0f) / 2.0f)
			};

			rt.Draw(logoSprite);

			Text sloganText = new Text("A free party game for\n" + personValue, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 6.0f, (350 - 256) / 2.0f + 189.0f),
				Color = Color.White,
				CharacterSize = 26,
				Style = Text.Styles.Bold
			};

			sloganText.Round();
			rt.Draw(sloganText);

			// Draw version and anti-sue-hammer strings
			Text versionText = new Text("Version " + Program.Version, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width - 16.0f, GameOptions.Height - 32.0f),
				CharacterSize = 16,
				Color = Color.Black
			};

			versionText.Position -= new Vector2f(versionText.GetLocalBounds().Width, 0.0f);

			versionText.Round();
			rt.Draw(versionText);

			Text originalText = new Text("A shameless rip-off of \"Cards Against Humanity\".", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(16.0f, GameOptions.Height - 32.0f),
				CharacterSize = 16,
				Color = Color.Black
			};

			originalText.Round();
			rt.Draw(originalText);

			base.Draw(rt);
		}

		private void AddButton(Vector2f position, string value, MenuButton.OnClickHandler handler)
		{
			MenuButton button = new MenuButton(position, value);
			button.OnClick += handler;

			Entities.Add(button);
		}

		private bool MenuMain()
		{
			Entities.Clear();

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 64.0f), "Play", MenuPlay);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f), "Original Game",
				() =>
				{
					Process.Start("http://www.cardsagainsthumanity.com");
					return true;
				});

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Exit", Game.Exit);

			return true;
		}

		private bool MenuPlay()
		{
			Entities.Clear();

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 64.0f), "Host", MenuHost);
			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f), "Join", MenuJoin);
			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back", MenuMain);

			return true;
		}

		private bool MenuHost()
		{
			Entities.Clear();

			MenuTextbox nameTextbox = new MenuTextbox("Display Name")
			{
				Position =
					new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f,
									250.0f + 128.0f + 16.0f),
				Selected = true,
				Value = persistentDisplayName
			};

			nameTextbox.OnReturn += a =>
			{
				if (nameTextbox.Value != "")
					Game.SetState(new Lobby(SessionRole.Server, "", nameTextbox.Value));
			};

			Entities.Add(nameTextbox);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f + 12.0f), "Settings",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					return MenuHostSettings();
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f + 12.0f + 64.0f), "Next",
				() =>
				{
					if (nameTextbox.Value != "")
						Game.SetState(new Lobby(SessionRole.Server, "", nameTextbox.Value));

					return true;
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					return MenuPlay();
				});

			return true;
		}

		private bool MenuJoin()
		{
			Entities.Clear();

			// IP Address textbox
			MenuTextbox ipTextbox = new MenuTextbox("IP Address")
			{
				Position =
					new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f, 250.0f + 128.0f + 16.0f),
				Selected = true
			};

			Entities.Add(ipTextbox);

			// Display Name textbox
			MenuTextbox nameTextbox = new MenuTextbox("Display Name")
			{
				Position =
					new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f,
									250.0f + 128.0f + 84.0f + 16.0f),
				Value = persistentDisplayName
			};

			nameTextbox.OnReturn += a =>
			{
				if (nameTextbox.Value != "" && ipTextbox.Value != "")
					Game.SetState(new Lobby(SessionRole.Client, ipTextbox.Value, nameTextbox.Value));
			};

			Entities.Add(nameTextbox);

			// Next button
			AddButton(new Vector2f(GameOptions.Width / 2.0f, 250.0f + 128.0f + 84.0f + 16.0f + 96.0f), "Next",
				() =>
				{
					if (nameTextbox.Value != "" && ipTextbox.Value != "")
						Game.SetState(new Lobby(SessionRole.Client, ipTextbox.Value, nameTextbox.Value));

					return true;
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					return MenuPlay();
				});

			return true;
		}

		private bool MenuHostSettings()
		{
			Entities.Clear();

			MenuNumberbox numberboxPointCap = new MenuNumberbox("Point Cap", 99)
			{
				Position = new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f, 250.0f + 128.0f + 16.0f),
				Value = Server.PointCap
			};

			Entities.Add(numberboxPointCap);

			MenuNumberbox numberSecondsPerTurn = new MenuNumberbox("Seconds Per Turn", 180)
			{
				Position = new Vector2f(GameOptions.Width / 2.0f + (GameOptions.Width / 8.0f), 250.0f + 128.0f + 16.0f),
				Value = Server.SecondsPerTurn
			};

			Entities.Add(numberSecondsPerTurn);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back",
				() =>
				{
					Server.PointCap = numberboxPointCap.Value;
					Server.SecondsPerTurn = numberSecondsPerTurn.Value;

					return MenuHost();
				});

			return true;
		}
	}
}
