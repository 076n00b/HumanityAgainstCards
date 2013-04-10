using System;
using System.Linq;
using System.Text.RegularExpressions;
using Californium;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities.Ui
{
	class Textbox : Entity
	{
		public delegate bool OnReturnHandler(string value);
		public event OnReturnHandler OnReturn;

		public string Value;
		public bool Selected;

		private readonly string label;
		private bool mouseIn, cursorVisible;
		private readonly State myState;

		public Textbox(string label)
		{
			OnReturn = null;
			Selected = false;
			mouseIn = false;
			cursorVisible = true;

			Value = "";

			this.label = label;

			myState = Game.PeekState();

			Size = new Vector2f((GameOptions.Width / 3.0f) * 2.0f - 64.0f, 48.0f);

			Timer.Every(0.65f, () =>
			{
				cursorVisible = !cursorVisible;
				return false;
			});

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

				foreach (var textbox in Parent.Entities.OfType<Textbox>())
					textbox.Selected = false;

				Selected = !Selected;

				return true;
			};

			Input.Text = args =>
			{
				if (!Selected || !myState.Equals(Game.PeekState()))
					return false;

				// 0x16 is Control + V
				if (args.Text[0] == 0x16)
				{
					Value += GameUtility.GetClipboardText();
					if (Value.Length >= 36)
						Value = Value.Remove(36, Value.Length - 36);

					return true;
				}

				if (args.Text == "\t")
				{
					foreach (var textbox in Parent.Entities.OfType<Textbox>())
						textbox.Selected = !textbox.Selected;

					return true;
				}

				if (args.Text == "\b")
				{
					if (Value.Length == 0)
						return true;

					Value = Value.Remove(Value.Length - 1);
					return true;
				}

				if (args.Text == "\r")
				{
					if (OnReturn != null && Selected)
					{
						Assets.PlaySound("Click.wav");
						if (OnReturn(Value))
							Value = "";
					}

					return true;
				}

				if (!Char.IsControl(args.Text[0]) && Value.Length < 36)
					Value += Regex.Replace(args.Text, "[\x01-\x1F]", "");

				return true;
			};
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape barArea = new RectangleShape(Size)
			{
				Position = Position,
				FillColor = Color.Black
			};

			rt.Draw(barArea);

			Text labelText = new Text(label, Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(16.0f, -24.0f),
				CharacterSize = 18,
				Color = Color.Black
			};

			labelText.Round();
			rt.Draw(labelText);

			Text messageText = new Text(Value, Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(8.0f, 8.0f),
				CharacterSize = 28,
				Color = Color.White
			};
			
			messageText.Round();
			rt.Draw(messageText);

			if (Selected && cursorVisible)
			{
				RectangleShape cursor = new RectangleShape(new Vector2f(2.0f, Size.Y - 16.0f))
				{
					Position = Position + new Vector2f(messageText.GetWidth() + 8.0f + 2.0f, 8.0f),
					FillColor = Color.White
				};

				cursor.Round();
				rt.Draw(cursor);
			}

			base.Draw(rt);
		}
	}
}
