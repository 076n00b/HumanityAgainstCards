using System;
using System.Collections.Generic;
using Californium;
using ManateesAgainstCards.Entities;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using ManateesAgainstCards.Network.Packets;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class SelectionScreen : State
	{
		private readonly bool isCzar;

		public SelectionScreen(ICollection<Tuple<ushort, List<string>>> options)
		{
			isCzar = ((InGame)Game.PeekFirstState()).LocalPlayer.Czar;

			Vector2f position = new Vector2f(GameOptions.Width / 2.0f - ((241.0f * 0.95f) * (options.Count - 1)) / 2.0f, 256.0f + 346.0f + 48.0f);
			foreach (Tuple<ushort, List<string>> option in options)
			{
				float yOffset = 0.0f;
				foreach (string cardValue in option.Item2)
				{
					Card card = new Card(new CardInfo(CardType.White, cardValue))
					{
						Position = position + new Vector2f((-(241.0f * 0.95f) / 2.0f) * 1.0f, -346.0f - 128.0f + yOffset),
						Scale = new Vector2f(0.95f, 0.95f)
					};

					Entities.Add(card);

					yOffset += 96.0f;
				}

				if (isCzar)
				{
					SelectButton dismissButton = new SelectButton(position + new Vector2f(0.0f, -346.0f + 56.0f + 32.0f + yOffset - 3.0f), "Pick");
					dismissButton.Position = new Vector2f((float) Math.Floor(dismissButton.Position.X),
					                                      (float) Math.Floor(dismissButton.Position.Y));
					Tuple<ushort, List<string>> option1 = option;
					dismissButton.OnClick += () =>
					{
						Client.SendMessage(new DeclareWinner(option1.Item1));
						Game.PopState();
					};

					Entities.Add(dismissButton);
				}

				position += new Vector2f(228.0f, 0.0f);
			}

			Assets.PlaySound("Boop.wav");
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape bgOverlay = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height))
			                           { FillColor = new Color(0, 0, 0, 128) };
			rt.Draw(bgOverlay);

			Text title = new Text(isCzar ? "Select a Champion" : "Cards on the Table", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f),
				CharacterSize = 48,
				Color = Color.White
			};

			title.Center();
			title.Round();

			title.Position += new Vector2f(1, 1);
			title.Color = Color.Black;
			rt.Draw(title);

			title.Position -= new Vector2f(1, 1);
			title.Color = Color.White;
			rt.Draw(title);

			Text blackCardText = GameUtility.Wrap(Client.CurrentBlackCard.Info.Value, Assets.LoadFont(Program.DefaultFont), 24, GameOptions.Width / 2.0f);
			blackCardText.Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f + 16.0f);
			blackCardText.CharacterSize = 24;
			blackCardText.Color = Color.White;

			blackCardText.Center();
			blackCardText.Origin = new Vector2f(blackCardText.Origin.X, 0.0f);
			blackCardText.Round();

			blackCardText.Position += new Vector2f(1, 1);
			blackCardText.Color = Color.Black;
			rt.Draw(blackCardText);

			blackCardText.Position -= new Vector2f(1, 1);
			blackCardText.Color = Color.White;
			rt.Draw(blackCardText);

			base.Draw(rt);
		}
	}
}
