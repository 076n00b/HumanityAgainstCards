using Californium;
using ManateesAgainstCards.Entities.Ui;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class PopupOverlay : State
	{
		private const float PaddingHorizontal = 0.25f;
		private const float PaddingVertical = 0.2f;

		public string Value { get; private set; }

		public PopupOverlay(string value)
		{
			Value = value;

			if (Game.PeekState().GetType() == typeof(PopupOverlay))
				Game.PopState();
		}

		public override void Enter()
		{
			Button okButton =
				new Button(new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * PaddingVertical + GameOptions.Height * (1.0f - PaddingVertical * 2.0f) - 64.0f), "Alright");
			okButton.OnClick += () =>
			{
				Game.PopState();
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

			Text labelSettings = new Text("Oh no! Something bad happened!", Assets.LoadFont(Program.DefaultFont))
			{
				Position = new Vector2f(GameOptions.Width / 2.0f, GameOptions.Height * PaddingVertical + 48.0f),
				Color = Color.Black,
				CharacterSize = 32
			};

			labelSettings.Center();
			labelSettings.Round();
			rt.Draw(labelSettings);

			Text labelText = GameUtility.Wrap(Value, Assets.LoadFont(Program.DefaultFont), 24,
											  GameOptions.Width * (1.0f - PaddingHorizontal * 2.0f) - 128.0f);
			labelText.Position = new Vector2f(GameOptions.Width * PaddingHorizontal + 32.0f,
												  GameOptions.Height * PaddingVertical + 80.0f);
			labelText.Color = Color.Black;
			labelText.CharacterSize = 24;
			labelText.Round();
			rt.Draw(labelText);

			base.Draw(rt);
		}
	}
}
