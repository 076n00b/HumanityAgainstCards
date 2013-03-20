using Californium;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities.Ui
{
	internal class Checkbox : Entity
	{
		public bool Value { get; private set; }
		public string Label { get; private set; }

		private bool mouseIn;

		public Checkbox(string label, bool defaultValue = false)
		{
			Value = defaultValue;
			Label = label;

			Size = new Vector2f(48.0f, 48.0f);

			Input.MouseMove = args =>
			{
				if (!BoundingBox.Contains(args.Position.X, args.Position.Y))
				{
					if (mouseIn)
						mouseIn = false;

					return false;
				}

				mouseIn = true;
				return true;
			};

			Input.MouseButton[Mouse.Button.Left] = args =>
			{
				if (!mouseIn || !args.Pressed)
					return false;

				Value = !Value;

				return true;
			};
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape checkbox = new RectangleShape(Size)
			{
				Position = Position,
				FillColor = Color.Black,
			};

			rt.Draw(checkbox);

			if (mouseIn)
			{
				RectangleShape checkboxHover = new RectangleShape(new Vector2f(48.0f - 8.0f, 48.0f - 8.0f))
				{
					Position = Position + new Vector2f(4f, 4f),
					FillColor = Color.Black,
					OutlineColor = Color.White,
					OutlineThickness = 2f
				};

				rt.Draw(checkboxHover);
			}

			Text labelText = new Text(Label, Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(48.0f + 16.0f, 24.0f + 2.0f),
				CharacterSize = 32,
				Color = Color.Black
			};

			labelText.Center(false);
			labelText.Round();
			rt.Draw(labelText);

			if (Value)
			{
				Text check = new Text("X", Assets.LoadFont(Program.DefaultFont))
				{
					Position = Position + Size / 2.0f,
					CharacterSize = 48,
					Color = Color.White
				};

				check.Center();
				check.Round();

				rt.Draw(check);
			}

			base.Draw(rt);
		}
	}
}
