using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class BlackCard : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.BlackCard;
			}
		}

		public CardInfo Card { get; private set; }

		public BlackCard()
		{
			Card = null;
		}

		public BlackCard(CardInfo card)
		{
			Card = card;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Card.Value);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Card = new CardInfo(CardType.Black, msg.ReadString());
		}
	}
}
