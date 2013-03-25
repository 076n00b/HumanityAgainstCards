using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class ChatMessage : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.ChatMessage;
			}
		}

		public string Value { get; private set; }

		public ChatMessage()
		{
			Value = "";
		}

		public ChatMessage(string value)
		{
			Value = value;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Value);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Value = msg.ReadString();
		}
	}
}
