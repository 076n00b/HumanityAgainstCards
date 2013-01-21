using System.Linq;

namespace HumanityAgainstCards
{
	public class CardInfo
	{
		public CardType Type { get; private set; }
		public string Value { get; private set; }
		public int PickCount { get; private set; }

		public CardInfo(CardType type, string value)
		{
			PickCount = 0;
			Type = type;
			Value = value;

			if (type == CardType.Black)
			{
				PickCount = Value.Count(c => c == '_');
				if (PickCount == 0)
					PickCount = 1;

				// Special cases -- possibly abstract this into the JSON somehow?
				if (Value.Contains("superhero/sidekick"))
					PickCount = 2;
				else if (Value.Contains("haiku"))
					PickCount = 3;
			}
		}
	}

	public enum CardType : byte
	{
		White,
		Black
	}
}
