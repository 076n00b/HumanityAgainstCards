using System.Collections.Generic;
using System.Linq;

namespace HumanityAgainstCards
{
	class CardLoader
	{
		private const string JsonCards = "Cards";

		public static List<CardInfo> Cards { get; private set; }

		static CardLoader()
		{
			Cards = new List<CardInfo>();
		}

		public static void LoadCards()
		{
			List<string> packs = Json.Load<List<string>>(JsonCards);
			foreach (CardPack cardPack in packs.Select(pack => Json.Load<CardPack>(pack)))
			{
				foreach(string card in cardPack.WhiteCards)
					Cards.Add(new CardInfo(CardType.White, card));

				foreach(string card in cardPack.BlackCards)
					Cards.Add(new CardInfo(CardType.Black, card));
			}
		}

		private class CardPack
		{
			public readonly List<string> WhiteCards;
			public readonly List<string> BlackCards;

			public CardPack()
			{
				WhiteCards = new List<string>();
				BlackCards = new List<string>();
			}
		}
	}
}
