using Californium;
using ManateesAgainstCards.Entities.Ui;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class ErrorMessageScreen : State
	{
		private readonly string value;

		public ErrorMessageScreen(string value)
		{
			ClearColor = Color.Black;
			this.value = value;
		}

		public override void Enter()
		{
			Button button = new Button(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height - 52.0f - 48.0f), "End Game");
			button.OnClick += Game.Exit;
			Entities.Add(button);

			base.Enter();
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
			title.Round();
			rt.Draw(title);

			Text blackCardText = new Text(value, Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height / 2.0f),
				CharacterSize = 36,
				Color = Color.White
			};

			blackCardText.Center();
			blackCardText.Round();
			rt.Draw(blackCardText);

			base.Draw(rt);
		}
	}
}
