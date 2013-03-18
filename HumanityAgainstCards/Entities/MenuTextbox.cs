using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Californium;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities
{
	class MenuTextbox : Entity
	{
		public delegate void OnReturnHandler(string value);
		public event OnReturnHandler OnReturn;

		public string Value;
		public bool Selected { get; set; }

		private readonly string label;
		private bool mouseIn;

		public MenuTextbox(string label)
		{
			OnReturn = null;
			Selected = false;
			mouseIn = false;

			Value = "";

			this.label = label;

			Size = new Vector2f((GameOptions.Width / 3.0f) * 2.0f - 64.0f, 48.0f);

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

				foreach (var textbox in Parent.Entities.OfType<MenuTextbox>())
					textbox.Selected = false;

				Selected = !Selected;

				return true;
			};

			Input.Text = args =>
			{
				if (!Selected)
					return false;

				// 0x16 is Control + V
				if (args.Text[0] == 0x16)
				{
					Value += GetClipboardText();
					return true;
				}

				if (args.Text == "\t")
				{
					foreach (var textbox in Parent.Entities.OfType<MenuTextbox>())
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
						OnReturn(Value);
						Value = "";
					}

					return true;
				}

				if (!Char.IsControl(args.Text[0]) && Value.Length < 36)
					Value += args.Text;

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

			labelText.Position = new Vector2f((float) Math.Floor(labelText.Position.X), (float) Math.Floor(labelText.Position.Y));
			rt.Draw(labelText);

			Text messageText = new Text(Value + (Selected ? "|" : ""), Assets.LoadFont(Program.DefaultFont))
			{
				Position = Position + new Vector2f(8.0f, 8.0f + 4.0f),
				CharacterSize = 22,
				Color = Color.White
			};

			messageText.Position = new Vector2f((float)Math.Floor(messageText.Position.X), (float)Math.Floor(messageText.Position.Y));
			rt.Draw(messageText);

			base.Draw(rt);
		}

		private string GetClipboardText()
		{
			string value = "";

			ThreadStart threadMethod =
			delegate
			{
				try
				{
					value = Clipboard.GetText(TextDataFormat.Text);
				}
				catch (Exception)
				{
					Console.WriteLine("Cannot get clipboard text!");
				}
			};

			Thread staThread = new Thread(threadMethod);
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.Start();
			staThread.Join();
			staThread.Abort();

			return value;
		}
	}
}
