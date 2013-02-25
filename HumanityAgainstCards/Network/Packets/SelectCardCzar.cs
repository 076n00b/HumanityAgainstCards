using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class SelectCardCzar : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.SelectCardCzar;
			}
		}

		public ushort Id { get; private set; }

		public SelectCardCzar()
		{
			Id = 0;
		}

		public SelectCardCzar(ushort id)
		{
			Id = id;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Id);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Id = msg.ReadUInt16();
		}
	}
}
