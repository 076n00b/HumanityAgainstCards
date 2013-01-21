using System.Collections.Generic;
using System.Linq;
using Californium;

namespace HumanityAgainstCards
{
	class Deck
	{
		public CardType Type { get; private set; }
		public Stack<CardInfo> Cards;

		public Deck(CardType type)
		{
			Type = type;
			Cards = new Stack<CardInfo>();

			// Shuffle deck
			List<CardInfo> shuffledCards = CardLoader.Cards.Where(t => t.Type == Type).ToList();
			Utility.Shuffle(shuffledCards);

			foreach (CardInfo card in shuffledCards)
				Cards.Push(card);
		}
	}
}
