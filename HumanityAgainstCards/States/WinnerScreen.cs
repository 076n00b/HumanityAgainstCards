using System;
using System.Collections.Generic;
using Californium;
using SFML.Graphics;
using SFML.Window;

namespace HumanityAgainstCards.States
{
	class WinnerScreen : State
	{
		private readonly string winnerName;
		private readonly string winnerCombo;

		public WinnerScreen(string winnerName, string blackCard, List<string> whiteCards)
		{
			this.winnerName = winnerName;

			if (Game.PeekState().GetType() == typeof(SelectionScreen))
				Game.PopState();

			winnerCombo = blackCard;
			if (whiteCards.Count > 0)
			{
				if (!blackCard.Contains("_"))
				{
					winnerCombo += "\n";
					foreach (string whiteCard in whiteCards)
						winnerCombo += whiteCard + "\n";
				}
				else
				{
					foreach (string whiteCard in whiteCards)
					{
						string blackValue = whiteCard.Replace(".", null);
						if (winnerCombo[0] != '_')
							blackValue = blackValue.ToLower();

						winnerCombo = GameUtility.ReplaceFirst(winnerCombo, "_", blackValue);
					}
				}
			}

			Timer.After(4.0f, () =>
				{
					if (Game.PeekState().GetType() == typeof(WinnerScreen))
						Game.PopState();
			    }
			);

			Assets.PlaySound("Boop.wav");
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape bgColor = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height))
			                         { FillColor = new Color(0, 0, 0, 128) };
			rt.Draw(bgColor);

			Text title = new Text(winnerName + " won!", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f),
				CharacterSize = 48,
				Color = Color.White
			};

			title.Center();
			title.Origin = new Vector2f((float)Math.Round(title.Origin.X), (float)Math.Round(title.Origin.Y));

			title.Position += new Vector2f(1, 1);
			title.Color = Color.Black;
			rt.Draw(title);

			title.Position -= new Vector2f(1, 1);
			title.Color = Color.White;
			rt.Draw(title);

			Text blackCardText = GameUtility.Wrap(winnerCombo, Assets.LoadFont(Program.DefaultFont), 36, GameOptions.Width * 0.6f);
			blackCardText.Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f - 128.0f);
			blackCardText.CharacterSize = 36;
			blackCardText.Color = Color.White;

			blackCardText.Center();
			blackCardText.Origin = new Vector2f((float)Math.Round(blackCardText.Origin.X), 0.0f);

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
