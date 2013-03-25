using System.Linq;
using Californium;
using ManateesAgainstCards.Entities;
using ManateesAgainstCards.Entities.Ui;
using ManateesAgainstCards.Network;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class GameOverScreen : State
	{
		public GameOverScreen()
		{
			Program.HandleNetworking = false;
			Timer.NextFrame(() =>
			{
				Client.Disconnect();
				Server.Shutdown();
			});

			Assets.PlaySound("Applause.wav");
		}

		public override void Enter()
		{
			Button button = new Button(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height - 52.0f - 48.0f), "Main Menu");
			button.OnClick += () =>
			{
				//Game.PopState();
				Game.SetState(new MainMenu());
				return true;
			};

			Entities.Add(button);

			base.Enter();
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape bgOverlay = new RectangleShape(new Vector2f(GameOptions.Width, GameOptions.Height)) { FillColor = Color.Black };
			rt.Draw(bgOverlay);

			Text title = new Text("Game Over", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f),
				CharacterSize = 48,
				Color = Color.White
			};

			title.Center();
			title.Round();
			rt.Draw(title);

			Text winnerTitle = new Text(GetWinnerText(), Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f),
				CharacterSize = 48,
				Color = Color.White
			};

			winnerTitle.Center();
			winnerTitle.Round();
			rt.Draw(winnerTitle);

			base.Draw(rt);
		}

		private string GetWinnerText()
		{
			InGame inGame = (InGame)Game.PeekFirstState();
			int highestScore = -1;
			string highestName = "";

			foreach (Player p in inGame.Players)
			{
				if (p.Score > highestScore)
				{
					highestScore = p.Score;
					highestName = p.Name;
				}
			}

			return inGame.Players.Any(p => p.Score == highestScore && p.Name != highestName) ? "Tie Game!" : highestName + " won!";
		}
	}
}
