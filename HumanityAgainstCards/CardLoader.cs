﻿using System.Collections.Generic;
using System.Linq;

namespace ManateesAgainstCards
{
	class CardLoader
	{
		private const string JsonCards = "Decks";

		public static List<CardDeck> Decks { get; private set; } 
		public static List<CardInfo> Cards { get; private set; }

		static CardLoader()
		{
			Decks = new List<CardDeck>();
			Cards = new List<CardInfo>();
		}

		public static void LoadDecks()
		{
			List<string> decks = Json.Load<List<string>>(JsonCards);
			foreach (string deckName in decks)
			{
				CardDeck cardDeck = Json.Load<CardDeck>(deckName);
				cardDeck.Name = deckName;

				Decks.Add(cardDeck);
			}

			Decks.First().Include = true;
		}

		public static void LoadCards()
		{
			foreach (CardDeck deck in Decks)
			{
				if (!deck.Include)
					continue;

				foreach (string value in deck.BlackCards)
					Cards.Add(new CardInfo(CardType.Black, value));

				foreach (string value in deck.WhiteCards)
					Cards.Add(new CardInfo(CardType.White, value));
			}
		}

		/*public static void LoadCards(List<CardDeck> decks)
		{
			foreach (CardDeck deck in decks)
			{
				foreach(string value in deck.BlackCards)
					Cards.Add(new CardInfo(CardType.Black, value));

				foreach(string value in deck.WhiteCards)
					Cards.Add(new CardInfo(CardType.White, value));
			}
		}

		public static void LoadAllCards()
		{
			LoadCards(Decks);
		}*/

		internal class CardDeck
		{
			public string Name;
			public bool Include;
			public readonly List<string> WhiteCards;
			public readonly List<string> BlackCards;

			public CardDeck(string name)
			{
				Name = name;
				Include = false;
				WhiteCards = new List<string>();
				BlackCards = new List<string>();
			}
		}
	}
}
