using System.Collections.Generic;
using System.Linq;
using Californium;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class HostSettingsOverlay : State
	{
		private const float PaddingHorizontal = 0.05f;
		private const float PaddingVertical = 0.05f;

		public override void Enter()
		{
			float boundingY = GameOptions.Height * (PaddingVertical * 3.0f) + 80.0f;
			float boundingX = GameOptions.Width * (PaddingHorizontal * 2.5f);

			Numberbox numberSecondsPerTurn = new Numberbox("Seconds Per Turn", 180)
			{
				Position = new Vector2f(boundingX, boundingY),
				Value = Server.SecondsPerTurn,
				MinimumValue = 30
			};

			Numberbox numberboxPointCap = new Numberbox("Point Cap", 99)
			{
				Position = new Vector2f(GameOptions.Width - boundingX, boundingY),
				Value = Server.PointCap
			};

			Entities.Add(numberboxPointCap);

			numberboxPointCap.Position.X -= numberboxPointCap.Width;

			Entities.Add(numberSecondsPerTurn);

			List<Checkbox> decks = new List<Checkbox>();
			for (int i = 0; i < CardLoader.Decks.Count; i++)
			{
				CardLoader.CardDeck deck = CardLoader.Decks[i];

				Checkbox checkboxDeck = new Checkbox(deck.Name, deck.Include)
				{
					Position = new Vector2f(boundingX + (i / 4 * 128), boundingY + 64.0f + 80.0f + ((i % 4) * 64.0f))
				};

				decks.Add(checkboxDeck);
				Entities.Add(checkboxDeck);
			}

			Button applyButton =
				new Button(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f + GameOptions.Height / 3.0f + 16.0f), "Apply");
			applyButton.OnClick += () =>
			{
				if (decks.Count(d => d.Value) == 0)
				{
					Game.PushState(new PopupOverlay("You must select at least one deck in order to host a game."));
					return true;
				}

				Server.PointCap = numberboxPointCap.Value;
				Server.SecondsPerTurn = numberSecondsPerTurn.Value;

				foreach (Checkbox deck in decks)
					CardLoader.Decks.First(d => d.Name == deck.Label).Include = deck.Value;

				Game.PopState();
				return true;
			};

			Entities.Add(applyButton);

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

			Text labelSettings = new Text("Host Settings", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * (PaddingVertical * 3.0f)),
				Color = Color.Black,
				CharacterSize = 48
			};

			labelSettings.Center();
			labelSettings.Round();
			rt.Draw(labelSettings);

			// boundingY + 64.0f + 80.0f
			Text labelDecks = new Text("Decks", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * (PaddingVertical * 3.0f) + 64.0f + 80.0f + 40.0f),
				Color = Color.Black,
				CharacterSize = 36
			};

			labelDecks.Center();
			labelDecks.Round();
			rt.Draw(labelDecks);

			Text portLabel = new Text("Forward port 1890.", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f,
					GameOptions.Height * (1.0f - PaddingVertical * 2.0f) + GameOptions.Height * PaddingVertical - 20.0f),
				CharacterSize = 16,
				Color = Color.Black
			};

			portLabel.Center();
			portLabel.Round();
			rt.Draw(portLabel);

			base.Draw(rt);
		}
	}
}
