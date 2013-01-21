using System;
using System.Collections.Generic;
using System.Linq;
using Californium;
using HumanityAgainstCards.Entities;
using HumanityAgainstCards.Network.Packets;
using HumanityAgainstCards.States;
using Lidgren.Network;
using SFML.Window;

namespace HumanityAgainstCards.Network
{
	class Client
	{
		public static List<Card> Hand { get; private set; }
		public static Card CurrentBlackCard { get; private set; }
		public static bool InMatch;

		public static string Name;
		public static int SecondsLeft { get; private set; }

		private static readonly Dictionary<ushort, Player> players;
		private static NetClient client;
		private static NetConnectionStatus lastStatus;
		private static readonly Random random;

		public static bool Connected
		{
			get { return lastStatus == NetConnectionStatus.Connected; }
		}

		static Client()
		{
			Hand = new List<Card>();
			CurrentBlackCard = null;
			InMatch = false;

			lastStatus = NetConnectionStatus.None;
			players = new Dictionary<ushort, Player>();
			Name = "Missingno";
			client = null;

			random = new Random();
		}

		public static void Connect(string host, int port)
		{
			if (client != null)
				Disconnect();

			NetPeerConfiguration config = new NetPeerConfiguration("HumanityAgainstCards")
			{
				MaximumHandshakeAttempts = 1,
				ConnectionTimeout = 10,
				PingInterval = 1
			};

			client = new NetClient(config);
			client.Start();
			client.Connect(host, port);

			Console.WriteLine("Client running...");
		}

		public static void Disconnect()
		{
			client.Disconnect("Disconnected");
			client = null;
		}

		public static void UpdateNetwork()
		{
			NetIncomingMessage msg;

			if (client == null)
				return;

			while ((msg = client.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						Console.WriteLine("{0}: {1}", msg.MessageType, msg.ReadString());
						break;

					case NetIncomingMessageType.StatusChanged:
						{
							NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
							string reason = msg.ReadString();

							Console.WriteLine("StatusChanged: {0}: {1}", status, reason);

							switch (status)
							{
								case NetConnectionStatus.Connected:
									Console.WriteLine("Connected, sending ServerJoin packet!");
									SendMessage(new ServerJoin(Name, Program.Version));
									break;

								case NetConnectionStatus.Disconnected:
									Game.SetState(lastStatus == NetConnectionStatus.Connected
													  ? new ErrorMessageScreen(reason)
													  : new ErrorMessageScreen("Cannot connect to host!"));

									Console.WriteLine("Disconnected!");
									break;
							}

							lastStatus = status;

							break;
						}

					case NetIncomingMessageType.Data:
						HandlePacket(Packet.ReadFromMessage(msg));
						break;
				}

				client.Recycle(msg);
			}
		}

		public static void SendMessage(Packet packet, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
		{
			if (client == null)
				return;

			NetOutgoingMessage msg = client.CreateMessage();
			Packet.WriteToMessage(msg, packet);
			client.SendMessage(msg, method);
		}

		private static void HandlePacket(Packet packet)
		{
			Console.WriteLine(packet.Type);

			switch (packet.Type)
			{
				case PacketType.ServerTime:
					SecondsLeft = ((ServerTime)packet).Seconds;
					break;

				case PacketType.SetStatus:
					{
						SetStatus setStatus = (SetStatus)packet;
						players[setStatus.Id].Thinking = setStatus.TurnOver;
						break;
					}

				case PacketType.GameOver:
					Game.PushState(new GameOverScreen());
					break;

				case PacketType.InitSelectionScreen:
					{
						InitSelectionScreen initSelectionScreen = (InitSelectionScreen)packet;
						Game.PushState(new SelectionScreen(initSelectionScreen.Options));
						InMatch = false;
						break;
					}

				case PacketType.SelectCardCzar:
					{
						SelectCardCzar selectCardCzar = (SelectCardCzar)packet;
						InGame game = (InGame)Game.PeekFirstState();

						foreach (Player p in game.Players)
							p.Czar = false;

						if (players.ContainsKey(selectCardCzar.Id))
							players[selectCardCzar.Id].Czar = true;
						else
							game.LocalPlayer.Czar = true;

						foreach (Player p in game.Entities.OfType<Player>())
							p.Thinking = !p.Czar;

						break;
					}

				case PacketType.WinnerPicked:
					{
						if (Game.PeekState().GetType() != typeof(GameOverScreen))
						{
							InMatch = true;
							Game.PopState();

							WinnerPicked winnerPicked = (WinnerPicked)packet;
							InGame game = (InGame)Game.PeekState();

							if (winnerPicked.Id != 0)
							{
								Player player = players.ContainsKey(winnerPicked.Id) ? players[winnerPicked.Id] : game.LocalPlayer;
								++player.Score;

								Game.PushState(new WinnerScreen(player.Name, CurrentBlackCard.Info.Value, winnerPicked.Cards));
							}
							else
								Game.PushState(new WinnerScreen("No one", CurrentBlackCard.Info.Value, new List<string>()));
						}
						break;
					}

				case PacketType.WhiteCard:
					{
						WhiteCard whiteCards = (WhiteCard)packet;

						foreach (CardInfo c in whiteCards.Cards)
						{
							Card card = new Card(c) { Scale = new Vector2f(0.643f * 0.8f, 0.643f * 0.8f) };
							Hand.Add(card);
							Game.PeekFirstState().Entities.Add(card);
						}

						bool haveBrainTumor = false;
						foreach (Card c in Hand)
						{
							if (c.Info.Value.Contains("brain"))
								haveBrainTumor = true;
						}

						if (haveBrainTumor)
							Assets.PlaySound("BrainTumorCardStart.wav");
						else if (random.Next(100) < 5)
							Assets.PlaySound("NoBrainTumorCardStart5.wav");

						break;
					}

				case PacketType.BlackCard:
					{
						BlackCard blackCard = (BlackCard)packet;

						if (CurrentBlackCard != null)
							Game.PeekFirstState().Entities.Remove(CurrentBlackCard);

						CurrentBlackCard = new Card(blackCard.Card)
							{ Position = new Vector2f(GameOptions.Width - 256.0f + 4.0f, 48.0f + 32.0f) };
						Game.PeekFirstState().Entities.Add(CurrentBlackCard);
						break;
					}

				case PacketType.LobbyBeginGame:
					Game.SetState(new InGame(((Lobby)Game.PeekState()).Players));
					InMatch = true;
					break;

				case PacketType.PlayerDelete:
					{
						PlayerDelete playerDelete = (PlayerDelete)packet;
						Game.PeekFirstState().Entities.Remove(players[playerDelete.Id]);
						players.Remove(playerDelete.Id);

						break;
					}

				case PacketType.PlayerNew:
					{
						PlayerNew playerNew = (PlayerNew)packet;
						Player player = new Player(playerNew.Name);
						players.Add(playerNew.Id, player);

						Game.PeekState().Entities.Add(player);
						break;
					}

				case PacketType.ChatMessage:
					{
						ChatMessage chatMessage = (ChatMessage)packet;
						if (Game.PeekState().GetType() == typeof(Lobby))
							((Lobby)Game.PeekState()).ChatBacklog.Add(chatMessage.Value);
						else
						{
							players[chatMessage.Id].SetMessage(chatMessage.Value);
							Console.WriteLine("{0} {1}", chatMessage.Id, players[chatMessage.Id].Name);
						}

						Assets.PlaySound("Bubble.wav");
						break;
					}

				default:
					Console.WriteLine("Unhandled packet!");
					break;
			}
		}
	}
}
