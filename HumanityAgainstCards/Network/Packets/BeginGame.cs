using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class BeginGame : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.LobbyBeginGame;
			}
		}

		public override void Write(NetOutgoingMessage msg)
		{
			
		}

		public override void Read(NetIncomingMessage msg)
		{
			
		}
	}
}
