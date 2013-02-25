using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class PlayerDelete : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.PlayerDelete;
			}
		}

		public ushort Id { get; private set; }

		public PlayerDelete()
		{
			Id = 0;
		}

		public PlayerDelete(ushort id)
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
