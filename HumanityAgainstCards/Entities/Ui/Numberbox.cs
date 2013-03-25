﻿using System;
using System.Linq;
using Californium;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities.Ui
{
	class Numberbox : Entity
	{
		public int MaxValue { get; private set; }
		public int Value
		{
			get
			{
				try
				{
					return Int32.Parse(stringValue);
				}
				catch
				{
					return MinimumValue;
				}
			}

			set
			{
				if (value >= MinimumValue)
					stringValue = value.ToString("G");
				else if (value <= MaxValue)
					stringValue = value.ToString("G");
			}
		}

		public int MinimumValue;

		public float Width
		{
			get
			{
				return Size.X + new Text(label, Assets.LoadFont(Program.DefaultFont)).GetLocalBounds().Width + 16.0f;
			}
		}

		public bool Selected;

		private readonly string label;
		private string stringValue;
		private bool mouseIn;

		public Numberbox(string label, int maxValue)
		{
			this.label = label;
			MinimumValue = 0;
			MaxValue = maxValue;
			stringValue = "";
			Selected = false;
			mouseIn = false;

			Size = new Vector2f(64.0f, 48.0f);

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

				foreach (var numberbox in Parent.Entities.OfType<Numberbox>())
					numberbox.Selected = false;

				Selected = !Selected;

				return true;
			};

			Input.Text += args =>
			{
				if (!Selected)
					return false;

				if (args.Text == "\b")
				{
					if (stringValue.Length == 0)
						return true;

					stringValue = stringValue.Remove(stringValue.Length - 1);
					return true;
				}

				if (Char.IsDigit(args.Text[0]))
				{
					stringValue += args.Text;

					if (Value > maxValue && stringValue.Length != 0)
						stringValue = stringValue.Remove(stringValue.Length - 1);

					if (Value < MinimumValue)
						stringValue = MinimumValue.ToString("G");
				}

				return true;
			};
		}

		public override void Draw(RenderTarget rt)
		{
			RectangleShape numberbox = new RectangleShape(Size)
			{
				Position = Position,
				FillColor = Color.Black,
			};

			rt.Draw(numberbox);

			if (mouseIn)
			{
				RectangleShape checkboxHover = new RectangleShape(new Vector2f(64.0f - 8.0f, 48.0f - 8.0f))
				{
					Position = Position + new Vector2f(4f, 4f),
					FillColor = Color.Black,
					OutlineColor = Color.White,
					OutlineThickness = 2f
				};

				rt.Draw(checkboxHover);
			}

			Text valueText = new Text(stringValue + (Selected ? "_" : ""), Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(8.0f, 8.0f),
				CharacterSize = 28,
				Color = Color.White
			};

			valueText.Round();
			rt.Draw(valueText);

			Text labelText = new Text(label, Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(64.0f + 16.0f, 24.0f + 2.0f),
				CharacterSize = 32,
				Color = Color.Black
			};

			labelText.Center(false);
			labelText.Round();
			rt.Draw(labelText);
		}
	}
}
