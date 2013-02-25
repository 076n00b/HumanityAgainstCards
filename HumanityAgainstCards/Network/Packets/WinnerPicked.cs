using System.Collections.Generic;
using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class WinnerPicked : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.WinnerPicked;
			}
		}

		public ushort Id { get; private set; }
		public List<string> Cards;

		public WinnerPicked()
		{
			Cards = new List<string>();
			Id = 0;
		}

		public WinnerPicked(ushort id, List<string> cards)
		{
			Cards = cards;
			Id = id;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Id);
			msg.Write(Cards.Count);
			foreach(string value in Cards)
				msg.Write(value);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Id = msg.ReadUInt16();
			int count = msg.ReadInt32();
			for(int i = 0; i < count; ++i)
				Cards.Add(msg.ReadString());
		}
	}
}
