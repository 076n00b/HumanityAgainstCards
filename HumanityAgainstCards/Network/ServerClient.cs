﻿using System;
using System.Collections.Generic;
using System.Linq;
using HumanityAgainstCards.Network.Packets;
using Lidgren.Network;

namespace HumanityAgainstCards.Network
{
	class ServerClient
	{
		public NetConnection Connection { get; private set; }
		public ushort Id { get; private set; }
		public string Name { get; private set; }
		public bool Verified { get; private set; }
		public int CardCount;
		public bool Ready;
		public List<string> SelectedCards { get; private set; }

		public ServerClient(NetConnection connection, ushort id)
		{
			Connection = connection;
			Id = id;

			SelectedCards = new List<string>();
			CardCount = 0;
			Ready = false;
			Verified = false;
		}

		public void HandleMessage(Packet msg)
		{
			switch (msg.Type)
			{
				case PacketType.DeclareWinner:
					Server.DeclareWinner(((DeclareWinner)msg).Id);
					Server.StartMatch(true);
					break;

				case PacketType.EndTurn:
				{
					EndTurn endTurn = (EndTurn) msg;
					SelectedCards.Clear();
					foreach(CardInfo info in endTurn.Cards)
						SelectedCards.Add(info.Value);
					
					CardCount -= endTurn.Cards.Count;
					Ready = true;

					Server.SendMessageToAllExcept(new SetStatus(Id, false), Id);
					break;
				}

				case PacketType.LobbyBeginGame:
					Server.SendMessageToAll(new BeginGame());
					Server.State = Server.States.InGame;
					Server.StartMatch(false);
					break;

				case PacketType.ServerJoin:
					{
						Console.WriteLine("ServerJoin recvd");
						ServerJoin serverJoin = (ServerJoin)msg;

						// Check version
						if (serverJoin.Version != Program.Version)
						{
							Connection.Disconnect("Host is running on a different version.");
							return;
						}

						Name = serverJoin.Name;

						Server.SendMessageToAllExcept(new PlayerNew(Name, Id), Id);
						foreach (var c in Server.Clients.Where(a => a.Id != Id))
							Connection.SendMessage(new PlayerNew(c.Name, c.Id));

						Verified = true;

						break;
					}

				case PacketType.ChatMessage:
				{
					ChatMessage chatMessage = (ChatMessage) msg;

					Server.SendMessageToAllExcept(new ChatMessage(Id, chatMessage.Value), Id);
					break;
				}

				default:
					Console.WriteLine("Unhandled packet {0}", msg.Type.ToString());
					break;
			}
		}

		public void Disconnect(string reason = "Disconnect")
		{
			Connection.Disconnect(reason);
		}
	}
}
