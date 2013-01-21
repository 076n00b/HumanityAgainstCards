using Lidgren.Network;

namespace HumanityAgainstCards.Network.Packets
{
	public class ServerTime : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.ServerTime;
			}
		}

		public int Seconds { get; private set; }

		public ServerTime()
		{
			Seconds = 0;
		}

		public ServerTime(int seconds)
		{
			Seconds = seconds;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Seconds);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Seconds = msg.ReadInt32();
		}
	}
}
