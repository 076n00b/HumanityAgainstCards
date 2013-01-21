using System;
using Californium;
using HumanityAgainstCards.Entities;
using SFML.Graphics;
using SFML.Window;

namespace HumanityAgainstCards.States
{
	class ErrorMessageScreen : State
	{
		private readonly string value;

		public ErrorMessageScreen(string value)
		{
			ClearColor = Color.Black;
			this.value = value;

			MenuButton button = new MenuButton(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height - 52.0f - 48.0f), "End Game");
			button.OnClick += Game.Exit;
			Entities.Add(button);
		}

		public override void Draw(RenderTarget rt)
		{
			Text title = new Text("Oh no! Something went wrong!", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, 48.0f),
				CharacterSize = 48,
				Color = Color.White
			};

			title.Center();
			title.Origin = new Vector2f((float)Math.Round(title.Origin.X), (float)Math.Round(title.Origin.Y));
			rt.Draw(title);

			Text blackCardText = new Text(value, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f),
				CharacterSize = 36,
				Color = Color.White
			};

			blackCardText.Center();
			blackCardText.Origin = new Vector2f((float)Math.Round(blackCardText.Origin.X), (float)Math.Round(blackCardText.Origin.Y));
			rt.Draw(blackCardText);

			base.Draw(rt);
		}
	}
}
