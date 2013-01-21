using Lidgren.Network;

namespace HumanityAgainstCards.Network.Packets
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
		public ushort Id { get; private set; }

		public ChatMessage()
		{
			Id = 0;
			Value = "";
		}

		public ChatMessage(string name, string value)
		{
			Id = 0;
			Value = name + ": " + value;
		}

		public ChatMessage(string value)
		{
			Id = 0;
			Value = value;
		}

		public ChatMessage(ushort id, string value)
		{
			Id = id;
			Value = value;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Id);
			msg.Write(Value);
		}

		public override void Read(NetIncomingMessage msg)
		{
			Id = msg.ReadUInt16();
			Value = msg.ReadString();
		}
	}
}
