using Lidgren.Network;

namespace ManateesAgainstCards.Network.Packets
{
	public class Voice : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.Voice;
			}
		}

		public byte[ ] VoiceData;

		public Voice()
		{
			VoiceData = null;
		}

		public Voice(byte[] voiceData)
		{
			VoiceData = voiceData;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(VoiceData.Length);
			msg.Write(VoiceData);
		}

		public override void Read(NetIncomingMessage msg)
		{
			VoiceData = msg.ReadBytes(msg.ReadInt32());
		}
	}
}
