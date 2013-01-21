using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace HumanityAgainstCards.Network.Packets
{
	public class InitSelectionScreen : Packet
	{
		public override PacketType Type
		{
			get
			{
				return PacketType.InitSelectionScreen;
			}
		}

		public List<Tuple<ushort, List<string>>> Options { get; private set; }

		public InitSelectionScreen()
		{
			Options = new List<Tuple<ushort, List<string>>>();
		}

		public InitSelectionScreen(List<Tuple<ushort, List<string>>> options)
		{
			Options = options;
		}

		public override void Write(NetOutgoingMessage msg)
		{
			msg.Write(Options.Count);
			foreach (Tuple<ushort, List<string>> t1 in Options)
			{
				msg.Write(t1.Item1);
				msg.Write(t1.Item2.Count);
				foreach (string t in t1.Item2)
					msg.Write(t);
			}
		}

		public override void Read(NetIncomingMessage msg)
		{
			int count = msg.ReadInt32();
			for(int i = 0; i < count; ++i)
			{
				ushort uid = msg.ReadUInt16();
				List<string> cards = new List<string>();

				int cardCount = msg.ReadInt32();
				for(int j = 0; j < cardCount; ++j)
					cards.Add(msg.ReadString());

				Options.Add(new Tuple<ushort, List<string>>(uid, cards));
			}
		}
	}
}
