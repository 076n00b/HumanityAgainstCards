using System;
using System.Collections.Generic;
using System.Diagnostics;
using Californium;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class MainMenu : State
	{
		private static readonly List<string> slogans = new List<string>
		{
			"Wiglets",
			"the Jews",
			"Nick's Mom",
			"the Westboro Baptist Church",
			"the Pepsi Generation",
			"Facepunch",
			"people with monochrome displays",
			"Recreationists",
			"Justin Bieber Fangirls",
			"White America"
		};

		private readonly string personValue;
		private string persistentDisplayName;

		public MainMenu()
		{
			Program.HandleNetworking = true;
			CardLoader.Reset();

			ClearColor = Color.White;
			persistentDisplayName = "";

			personValue = slogans[new Random().Next(slogans.Count)];
		}

		public override void Enter()
		{
			MenuMain();
			base.Enter();
		}

		public override void Draw(RenderTarget rt)
		{
			// Draw header
			rt.Draw(new RectangleShape(new Vector2f(GameOptions.Width, 350.0f)) { FillColor = Color.Black });

			// GameOptions.Width / 6.0f
			Sprite logoSprite = new Sprite(Assets.LoadTexture("Logo.png"))
			{
				Position = new Vector2f(128.0f, (350.0f - 256.0f) / 2.0f)
			};

			rt.Draw(logoSprite);

			Text sloganText = new Text("A free party game for\n" + personValue, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(128.0f, (350 - 256) / 2.0f + 189.0f),
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

		private void AddButton(Vector2f position, string value, Button.OnClickHandler handler)
		{
			Button button = new Button(position, value);
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
				}
			);

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

			Textbox nameTextbox = new Textbox("Display Name")
			{
				Position =
					new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f,
									250.0f + 128.0f + 16.0f),
				Selected = true,
				Value = persistentDisplayName
			};

			nameTextbox.OnReturn += value => HostNext(nameTextbox.Value);
			Entities.Add(nameTextbox);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f + 12.0f), "Settings",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					Game.PushState(new HostSettingsOverlay());
					return true;
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 128.0f + 12.0f + 64.0f), "Next",
				() =>
				{
					HostNext(nameTextbox.Value);
					return true;
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					return MenuPlay();
				}
			);

			return true;
		}

		private bool MenuJoin()
		{
			Entities.Clear();

			// Display Name textbox
			Textbox nameTextbox = new Textbox("Display Name")
			{
				Position =
					new Vector2f(GameOptions.Width / 2.0f - GameOptions.Width / 3.0f,
									250.0f + 128.0f + 16.0f), // 250.0f + 128.0f + 84.0f + 16.0f
				Value = persistentDisplayName,
				Selected = true
			};

			Entities.Add(nameTextbox);

			// Next button
			AddButton(new Vector2f(GameOptions.Width / 2.0f, 250.0f + 128.0f + 84.0f + 16.0f + 96.0f), "Next",
				() =>
				{
					if (String.IsNullOrEmpty(nameTextbox.Value))
					{
						Game.PushState(new PopupOverlay("You must enter a name before joining a game."));
						return true;
					}

					Client.Name = nameTextbox.Value;
					Game.PushState(new ServerListOverlay());

					return true;
				}
			);

			AddButton(new Vector2f(GameOptions.Width / 2.0f, 350.0f + 288.0f), "Back",
				() =>
				{
					persistentDisplayName = nameTextbox.Value;
					return MenuPlay();
				}
			);

			return true;
		}

		private void HostNext(string name)
		{
			if (!String.IsNullOrEmpty(name))
				Game.SetState(new Lobby(SessionRole.Server, "", name));
			else
				Game.PushState(new PopupOverlay("You must enter a name before hosting a game."));
		}
	}
}
