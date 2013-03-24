using Californium;
using ManateesAgainstCards.Entities;
using SFML.Graphics;
using SFML.Window;

namespace ManateesAgainstCards.States
{
	class WrapTest : State
	{
		private readonly Deck blackDeck, whiteDeck;
		private Card card;

		public WrapTest()
		{
			ClearColor = Color.White;

			foreach (CardLoader.CardDeck deck in CardLoader.Decks)
				deck.Include = true;

			CardLoader.LoadCards();

			blackDeck = new Deck(CardType.Black);
			whiteDeck = new Deck(CardType.White);

			card = null;

			Input.Key.Add(Keyboard.Key.Return,  args =>
			{
				if (!args.Pressed)
					return true;

				if (card != null)
					Entities.Remove(card);

				card = blackDeck.Cards.Count == 0 ? null : new Card(blackDeck.Cards.Pop());
				card.Position = new Vector2f(GameOptions.Width / 2.0f - 256.0f, GameOptions.Height / 2.0f - 256.0f);

				if (card != null)
					Entities.Add(card);

				return true;
			});
		}
	}
}
