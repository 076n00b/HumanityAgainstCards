﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lidgren.Network;

namespace ManateesAgainstCards.Network
{
	public abstract class Packet
	{
		public virtual PacketType Type
		{
			get { return PacketType.Invalid; }
		}

		public abstract void Write(NetOutgoingMessage msg);
		public abstract void Read(NetIncomingMessage msg);

		private static readonly Dictionary<PacketType, Type> PacketTypes;

		static Packet()
		{
			PacketTypes = new Dictionary<PacketType, Type>();

			Assembly assembly = Assembly.GetCallingAssembly();
			var types = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Packet)));

			foreach (Type type in types)
			{
				Packet instance = (Packet)Activator.CreateInstance(type);
				PacketTypes[instance.Type] = type;
			}
		}

		public static void WriteToMessage(NetOutgoingMessage msg, Packet packet)
		{
			msg.Write((byte)packet.Type);
			packet.Write(msg);
		}

		public static Packet ReadFromMessage(NetIncomingMessage msg)
		{
			PacketType type = (PacketType)msg.ReadByte();
			Packet packet = (Packet)Activator.CreateInstance(PacketTypes[type]);
			packet.Read(msg);

			return packet;
		}
	}
}
