using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class PlayerNew : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.PlayerNew;
			}
		}

		public string Name { get; private set; }
		public ushort Id { get; private set; }

		public PlayerNew()
		{
			Name = "";
			Id = 0;
		}

		public PlayerNew(string name, ushort id)
		{
			Name = name;
			Id = id;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Name);
			msg.Write(Id);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Name = msg.ReadString();
			Id = msg.ReadUInt16();
		}
	}
}
