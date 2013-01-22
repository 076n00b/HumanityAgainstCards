using System;
using Californium;
using HumanityAgainstCards.Network;
using HumanityAgainstCards.States;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace HumanityAgainstCards.Entities
{
	internal class Card : Entity
	{
		private bool mouseIn;

		public CardInfo Info { get; private set; }
		public int Index { get; private set; }
		public bool Selected;

		private static int indexCounter;

		public Card(CardInfo info)
		{
			Index = 0;
			Info = info;
			Selected = false;
			mouseIn = false;

			Input.MouseButton[Mouse.Button.Left] = LeftMouseButton;
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
		}

		public override void Draw(RenderTarget rt)
		{
			Vector2f actualPosition = Position + ((Selected) ? new Vector2f(0, -12.0f) : new Vector2f(0.0f, 0.0f));

			// Draw card
			Sprite sprite = new Sprite(Assets.LoadTexture(Info.Type == CardType.White ? "CardWhite.png" : "CardBlack.png"));
			Size = new Vector2f(sprite.GetGlobalBounds().Width, sprite.GetGlobalBounds().Height);
			sprite.Position = actualPosition;
			sprite.Scale = Scale;
			rt.Draw(sprite);

			// Draw text
			Text text = GameUtility.Wrap(Info.Value, Assets.LoadFont("arialbd.ttf"), (uint)Math.Floor(24.0f * Scale.X),
									 Math.Floor(192.0f * Scale.X));

			text.Color = Info.Type == CardType.White ? Color.Black : Color.White;
			text.Position = actualPosition + new Vector2f(17.0f * Scale.X, 10.0f * Scale.Y);
			text.Position = new Vector2f((float)Math.Round(text.Position.X), (float)Math.Round(text.Position.Y));
			rt.Draw(text);

			// Draw decorations
			if (Info.PickCount > 1)
			{
				Sprite pickMultiple = new Sprite(Assets.LoadTexture(Info.PickCount == 2 ? "PickTwo.png" : "PickThree.png"))
				{
					Position =
						actualPosition +
						new Vector2f((241.0f - 56.0f - 10.0f - 4.0f) * Scale.X, (320.0f - 10.0f - 20.0f) * Scale.Y),
					Scale = Scale
				};

				rt.Draw(pickMultiple);
			}
		}

		private bool LeftMouseButton(MouseButtonInputArgs args)
		{
			bool prevSelected = Selected;

			if (Game.PeekState().GetType() != typeof(InGame))
				return false;

			if (!mouseIn || !args.Pressed || Info.Type == CardType.Black)
				return false;

			InGame inGame = Parent as InGame;
			if (inGame == null)
				return true;

			int checkCount = Client.CurrentBlackCard.Info.PickCount;
			checkCount -= Client.Hand.Count(c => c.Selected);

			if (checkCount > 0 && !Selected)
				Selected = !Selected;
			else if (checkCount >= 0 && Selected)
				Selected = false;

			if (Selected)
				Index = indexCounter++;

			if (prevSelected != Selected && !inGame.LocalPlayer.Czar)
				Assets.PlaySound("Bubble.wav");

			return true;
		}
	}
}
