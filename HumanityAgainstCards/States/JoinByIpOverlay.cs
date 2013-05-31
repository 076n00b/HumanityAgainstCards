using System;
using Californium;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class JoinByIpOverlay : State
	{
		private const float PaddingHorizontal = 0.25f;
		private const float PaddingVertical = 0.2f;

		public string Value { get; private set; }
		private Textbox ipAddressTextbox;

		public JoinByIpOverlay()
		{
			Input.Key.Add(Keyboard.Key.Return, args =>
			{
				if (ipAddressTextbox != null)
					JoinNext(ipAddressTextbox.Value);

				return true;
			});

			if (Game.PeekState().GetType() == typeof(PopupOverlay))
				Game.PopState();
		}

		public override void Enter()
		{
			ipAddressTextbox = new Textbox("")
			{
				Position = new Vector2f(GameOptions.Width * PaddingHorizontal + 32.0f, GameOptions.Height * PaddingVertical + 128.0f),
				Size = new Vector2f(GameOptions.Width * (1.0f - PaddingHorizontal * 2.0f) - 64.0f, 48.0f),
				Selected = true
			};

			Entities.Add(ipAddressTextbox);

			Button okButton =
				new Button(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * PaddingVertical + GameOptions.Height * (1.0f - PaddingVertical * 2.0f) - 64.0f), "Alright");
			okButton.OnClick += () =>
			{
				Game.PopState();
				JoinNext(ipAddressTextbox.Value);
				return true;
			};

			Entities.Add(okButton);

			base.Enter();
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape overylay = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height))
			{
				FillColor = new Color(0, 0, 0, 128)
			};

			rt.Draw(overylay);

			RectangleShape window = new RectangleShape(new Vector2f(GameOptions.Width * (1.0f - PaddingHorizontal * 2.0f), GameOptions.Height * (1.0f - PaddingVertical * 2.0f)))
			{
				Position = new Vector2f(GameOptions.Width * PaddingHorizontal, GameOptions.Height * PaddingVertical),
				FillColor = Color.White,
				OutlineColor = Color.Black,
				OutlineThickness = 2.0f
			};

			rt.Draw(window);

			Text labelSettings = new Text("Join by IP", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * PaddingVertical + 48.0f),
				Color = Color.Black,
				CharacterSize = 32
			};

			labelSettings.Center();
			labelSettings.Round();
			rt.Draw(labelSettings);

			base.Draw(rt);
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
	}
}
