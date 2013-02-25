using System.Collections.Generic;
using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class EndTurn : Packet
	{
		public override PacketType Type
		{
			get
			{ 
				return PacketType.EndTurn;
			}
		}

		public List<CardInfo> Cards { get; private set; }

		public EndTurn()
		{
			Cards = new List<CardInfo>();
		}

		public EndTurn(List<CardInfo> cards)
		{
			Cards = cards;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Cards.Count);
			foreach (CardInfo info in Cards)
				msg.Write(info.Value);
		}

		public override void Read(NetIncomingMessage msg)
		{
			int count = msg.ReadInt32();
			for (int i = 0; i < count; ++i)
				Cards.Add(new CardInfo(CardType.White, msg.ReadString()));
		}
	}
}
