using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class SetStatus : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.SetStatus;
			}
		}

		public bool TurnOver;
		public ushort Id;

		public SetStatus()
		{
			Id = 0;
			TurnOver = true;
		}

		public SetStatus(ushort id, bool turnOver)
		{
			Id = id;
			TurnOver = turnOver;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Id);
			msg.Write(TurnOver);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Id = msg.ReadUInt16();
			TurnOver = msg.ReadBoolean();
		}
	}
}
