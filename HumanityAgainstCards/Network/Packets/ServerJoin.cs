using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class ServerJoin : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.ServerJoin;
			}
		}

		public string Name { get; private set; }
		public string Version { get; private set; }

		public ServerJoin()
		{
			Name = "Missingno";
			Version = "";
		}

		public ServerJoin(string name, string version)
		{
			Name = name;
			Version = version;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Name);
			msg.Write(Version);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Name = msg.ReadString();
			Version = msg.ReadString();
		}
	}
}
