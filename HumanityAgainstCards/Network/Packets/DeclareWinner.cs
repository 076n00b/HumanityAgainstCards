using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class DeclareWinner : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.DeclareWinner;
			}
		}

		public ushort Id { get; private set; }

		public DeclareWinner()
		{
			Id = 0;
		}

		public DeclareWinner(ushort id)
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
