using Californium;
using ManateesAgainstCards.Network;
using ManateesAgainstCards.States;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.Entities
{
	class Player : Entity
	{
		public string Name { get; private set; }
		public bool IsLocalPlayer { get; private set; }
		public int Score;
		public bool Czar, Thinking;
		
		private string chatMessage;
		private bool showMessage;

		public Player(string name, bool localPlayer = false)
		{
			IsLocalPlayer = localPlayer;
			Name = name;

			showMessage = false;
			chatMessage = "";
			Score = 0;
			Czar = false;
			Thinking = true;
		}

		public override void Draw(RenderTarget rt)
		{
			if (Parent.GetType() == typeof(InGame))
			{
				// Draw avatar
				Sprite avatar = new Sprite(Assets.LoadTexture("Avatar.png"))
				{
					Position = Position,
					Scale = new Vector2f(1.0f, 1.0f)
				};

				avatar.Origin = new Vector2f(avatar.TextureRect.Width / 2.0f, avatar.TextureRect.Height);
				rt.Draw(avatar);

				// Draw crown if czar
				if (Czar)
				{
					Sprite crown = new Sprite(Assets.LoadTexture("Crown.png"))
					{
						Position = Position
					};

					crown.Origin = new Vector2f(crown.TextureRect.Width / 2.0f, crown.TextureRect.Height);
					rt.Draw(crown);
				}

				Text nameText = GameUtility.Wrap(showMessage ? chatMessage : Name, Assets.LoadFont(Program.DefaultFont), 18,
				                             128.0 + 32.0 - 8.0);
				nameText.Position = Position + new Vector2f(0.0f, 16.0f);
				nameText.CharacterSize = 18;
				nameText.Color = Color.Black;
				
				nameText.Center();
				nameText.Round();
				rt.Draw(nameText);

				// Draw hand
				if (IsLocalPlayer)
				{
					for (int i = 0; i < Client.Hand.Count; ++i)
						Client.Hand[i].Position = new Vector2f(i * 124 + 20, GameOptions.Height - 194 + 8);
				}

				DrawOutlineText(Score.ToString("G"), Assets.LoadFont(Program.DefaultFont), 24,
								new Vector2f(Position.X + 24.0f, Position.Y - 24.0f), rt);

				if (Thinking && !Czar)
					DrawOutlineText("...", Assets.LoadFont(Program.DefaultFont), 30, Position - new Vector2f(9.0f + 6.0f, 40.0f), rt);
			}
			else if (Parent.GetType() == typeof(Lobby))
			{
				// Draw avatar
				Sprite avatar = new Sprite(Assets.LoadTexture("Avatar.png"))
				{
					Position = Position,
					Scale = new Vector2f(0.87f, 0.87f)
				};

				rt.Draw(avatar);

				Text nameText = new Text(Name, Assets.LoadFont(Program.DefaultFont))
				{
					Position = Position + new Vector2f(96.0f, 87.0f / 2.0f),
					CharacterSize = 36,
					Color = Color.Black
				};

				nameText.Position -= new Vector2f(0.0f, nameText.GetLocalBounds().Height / 2.0f);
				nameText.Round();

				rt.Draw(nameText);
			}

			base.Draw(rt);
		}

		private void DrawOutlineText(string value, Font font, uint size, Vector2f position, RenderTarget rt)
		{
			Text outlineText = new Text(value, font)
			{
				Color = Color.Black,
				CharacterSize = size,
				Position = position + new Vector2f(-1.0f, 0.0f)
			};

			rt.Draw(outlineText);
			outlineText.Position = position + new Vector2f(+1.0f, 0.0f);
			rt.Draw(outlineText);
			outlineText.Position = position + new Vector2f(0.0f, -1.0f);
			rt.Draw(outlineText);
			outlineText.Position = position + new Vector2f(0.0f, +1.0f);
			rt.Draw(outlineText);

			outlineText.Color = Color.White;
			outlineText.Position = position;
			rt.Draw(outlineText);
		}

		public void SetMessage(string value)
		{
			Timer.After(5.0f, () => { showMessage = false; });
			
			chatMessage = "\"" + value + "\"";
			showMessage = true;
		}
	}
}
