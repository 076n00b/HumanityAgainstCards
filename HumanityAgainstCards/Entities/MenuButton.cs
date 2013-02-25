using Californium;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities
{
	class MenuButton : Entity
	{
		private const float ButtonWidth = 222.0f;
		private const float ButtonHeight = 52.0f;

		public delegate void OnClickHandler();
		public event OnClickHandler OnClick;

		private readonly string value;
		private bool mouseIn;
		public bool Visible;

		private bool Clickable
		{
			get
			{
				return Game.PeekState() != Game.PeekFirstState() || Visible;
			}
		}

		public MenuButton(Vector2f position, string value)
		{
			Size = new Vector2f(ButtonWidth, ButtonHeight);
			Position = position - Size / 2.0f;
			Visible = true;
			this.value = value;

			mouseIn = false;

			Input.MouseMove = args =>
			{
				if (!BoundingBox.Contains(args.Position.X, args.Position.Y))
				{
					mouseIn = false;

					return false;
				}

				mouseIn = true;
				return false;
			};

			Input.MouseButton[Mouse.Button.Left] = args =>
			{
				if (!mouseIn || !args.Pressed || !Clickable)
					return false;

				if (OnClick != null)
				{
					Assets.PlaySound("Click.wav");
					OnClick();
				}

				return true;
			};

			// Simulate mouse move
			Vector2i mousePosition = Mouse.GetPosition(Game.Window);
			Input.MouseMove(new MouseMoveInputArgs(mousePosition.X, mousePosition.Y));
		}

		public override void Draw(RenderTarget rt)
		{
			if (!Visible)
				return;

			RectangleShape button = new RectangleShape(new Vector2f(ButtonWidth, ButtonHeight))
			{
				Position = Position,
				FillColor = Color.Black
			};

			rt.Draw(button);

			// Hover
			if (mouseIn)
			{
				RectangleShape buttonHover = new RectangleShape(new Vector2f(ButtonWidth - 8, ButtonHeight - 8))
				{
					Position = Position + new Vector2f(4f, 4f),
					FillColor = Color.Black,
					OutlineColor = Color.White,
					OutlineThickness = 2f
				};

				rt.Draw(buttonHover);
			}

			Text text = new Text(value, Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + Size / 2.0f,
				CharacterSize = 24,
				Style = Text.Styles.Bold
			};

			text.Center();
			text.Round();
			rt.Draw(text);

			base.Draw(rt);
		}
	}
}
