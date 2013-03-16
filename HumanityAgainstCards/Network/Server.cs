using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Lidgren.Network;
using ManateesAgainstCards.Network.Packets;

namespace ManateesAgainstCards.Network
{
	static class Server
	{
		public const int SecondsPerTurn = 45;

		private static readonly Random random;

		public static List<ServerClient> Clients { get; private set; }
		private static NetServer server;

		public enum States
		{
			Lobby,
			InGame
		}

		public static States State;
		public static bool CanDeal, WaitingForAllReady;
		public static ushort CurrentCardCzar;
		private static int currentCarzIndex;

		private static readonly Deck whiteDeck;
		private static readonly Deck blackDeck;

		private static readonly Stopwatch timer;
		private static int secondsLeft;
		private static bool inMatch;

		static Server()
		{
			whiteDeck = new Deck(CardType.White);
			blackDeck = new Deck(CardType.Black);

			State = States.Lobby;
			CanDeal = true;
			WaitingForAllReady = false;
			CurrentCardCzar = 0;
			currentCarzIndex = 0;

			timer = new Stopwatch();
			random = new Random();
			server = null;

			inMatch = false;
			secondsLeft = 0;
			timer.Start();
		}

		public static void Initialize(int port)
		{
			Clients = new List<ServerClient>();

			NetPeerConfiguration config = new NetPeerConfiguration("HumanityAgainstCards")
			{
				Port = port,
				ConnectionTimeout = 10,
				PingInterval = 1
			};

			server = new NetServer(config);
			server.Start();

			Console.WriteLine("Server running...");
		}

		public static void UpdateNetwork()
		{
			NetIncomingMessage msg;

			if (server == null)
				return;

			while ((msg = server.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
						Console.WriteLine("{0}: {1}", msg.MessageType, msg.ReadString());
						break;

					case NetIncomingMessageType.StatusChanged:
						{
							if (msg.SenderConnection == null)
								break;

							NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
							string reason = msg.ReadString();

							Console.WriteLine("StatusChanged: {0} '{1}'", status, reason);

							switch (status)
							{
								case NetConnectionStatus.Connected:
									{
										if (State == States.InGame)
										{
											// Fuck off, no visitors
											msg.SenderConnection.Disconnect("Already in game, fuck off.");
											break;
										}

										if (Clients.Count == 6)
										{
											msg.SenderConnection.Disconnect("Too crowded, fuck off.");
											break;
										}

										ServerClient client = new ServerClient(msg.SenderConnection, GenerateId());
										msg.SenderConnection.Tag = client;
										Clients.Add(client);
										break;
									}

								case NetConnectionStatus.Disconnected:
									{
										if (msg.SenderConnection.Tag == null)
											break;

										ServerClient serverClient = (ServerClient)msg.SenderConnection.Tag;
										if (serverClient.Verified)
										{
											SendMessageToAllExcept(new PlayerDelete(serverClient.Id), serverClient.Id);

											ServerClient client = (ServerClient)msg.SenderConnection.Tag;
											client.Disconnect();
											Clients.Remove(client);

											// Card Czar left like the asshole he is
											if (serverClient.Id == CurrentCardCzar)
												DeclareWinner(Clients[random.Next(Clients.Count)].Id);
										}
										break;
									}
							}
							break;
						}

					case NetIncomingMessageType.Data:
						{
							if (msg.SenderConnection == null || msg.SenderConnection.Tag == null)
								break;

							ServerClient client = (ServerClient)msg.SenderConnection.Tag;
							Packet packet = Packet.ReadFromMessage(msg);
							client.HandleMessage(packet);
							break;
						}

					default:
						Console.WriteLine("Unhandled type: " + msg.MessageType);
						break;
				}

				server.Recycle(msg);
			}

			HandleGame();
		}

		public static void SendMessageToAllExcept(Packet packet, ushort id, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
		{
			if (server == null)
				return;

			foreach (var serverClient in Clients.Where(c => c.Id != id))
				serverClient.Connection.SendMessage(packet, method);
		}

		public static void SendMessageToAll(Packet packet, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered, Func<ServerClient, bool> condition = null)
		{
			if (server == null)
				return;

			foreach (var serverClient in Clients.Where(c => condition == null || condition(c)))
				serverClient.Connection.SendMessage(packet, method);
		}

		public static NetSendResult SendMessage(this NetConnection connection, Packet packet, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
		{
			var msg = server.CreateMessage();
			Packet.WriteToMessage(msg, packet);

			return connection.SendMessage(msg, method, 0);
		}

		public static ushort GenerateId()
		{
			ushort id;

			if (Clients.Count >= ushort.MaxValue)
				throw new Exception("Too many clients connected!");

			do
			{
				id = (ushort)random.Next(ushort.MaxValue);
			} while (id == 0 || Clients.Count(c => c.Id == id) != 0);

			return id;
		}

		public static void DeclareWinner(ushort id)
		{
            // Check if they're still in game
            if (Clients.Where(c => c.Id == id).Count() == 0)
                id = Clients[random.Next(Clients.Count)].Id;

			List<string> cards = new List<string>();
			foreach (ServerClient c in Clients.Where(c => c.Id == id))
			{
				cards = c.SelectedCards;
				break;
			}

			// Pick random winner
			SendMessageToAll(new WinnerPicked(id, cards));
			CanDeal = true;

			foreach (ServerClient c in Clients)
				c.Ready = false;

			CurrentCardCzar = 0;
		}

		private static void HandleGame()
		{
			if (State != States.InGame)
				return;

			// Kill games that only have a single player
			if (Clients.Count < 2)
			{
				SendMessageToAll(new GameOver());
				return;
			}

			// Select random new card czar
			if (CurrentCardCzar == 0)
			{
				CurrentCardCzar = Clients[currentCarzIndex++ % Clients.Count].Id;
				SendMessageToAll(new SelectCardCzar(CurrentCardCzar));
			}

			// Keep games in sync with timer
			if (timer.ElapsedMilliseconds >= 1000 && inMatch)
			{
				if (secondsLeft != 0)
				{
					if (--secondsLeft <= SecondsPerTurn)
						SendMessageToAll(new ServerTime(secondsLeft));

					timer.Restart();
				}
			}

			// Check if we're waiting for everyone to be ready
			bool allReady = Clients.Where(sc => sc.Id != CurrentCardCzar).All(sc => sc.Ready);
			if ((allReady && WaitingForAllReady) || (secondsLeft == 0 && inMatch))
			{
				// Get cards from everyone except czar
				List<Tuple<ushort, List<string>>> cards = Clients.Where(sc => sc.Id != CurrentCardCzar).Select(sc => new Tuple<ushort, List<string>>(sc.Id, sc.SelectedCards)).ToList();
				cards.RemoveAll(c => c.Item2.Count == 0);
				GameUtility.Shuffle(cards);

				if (cards.Count == 0)
				{
					DeclareWinner(0x0000);
					StartMatch(true);
				}
				else
				{
					SendMessageToAll(new InitSelectionScreen(cards));
					inMatch = false;
				}

				WaitingForAllReady = false;
			}

			if (CanDeal)
			{
				if (whiteDeck.Cards.Count == 0)
				{
					Console.WriteLine("No more white cards!");

					bool noCards = true;
					foreach(ServerClient sc in Clients)
					{
						if (sc.CardCount != 0)
							noCards = false;
					}

					if (noCards)
						SendMessageToAll(new GameOver());
				}

				if (blackDeck.Cards.Count == 0)
				{
					Console.WriteLine("Game Over! No more black cards!");
					SendMessageToAll(new GameOver());
					return;
				}

				SendMessageToAll(new BlackCard(blackDeck.Cards.Pop()));

				// Deal out new white cards to everyone who needs them
				if (whiteDeck.Cards.Count != 0)
				{
					foreach (ServerClient c in Clients)
					{
						List<CardInfo> cards = new List<CardInfo>();

						int cardsNeeded = 10 - c.CardCount;
						for (int i = 0; i < cardsNeeded; ++i)
						{
							if (whiteDeck.Cards.Count != 0)
								cards.Add(whiteDeck.Cards.Pop());
						}

						c.Connection.SendMessage(new WhiteCard(cards));

						c.CardCount = 10;
					}
				}

				CanDeal = false;
				WaitingForAllReady = true;
			}
		}

		public static void StartMatch(bool cooldown)
		{
			// Clear selected cards
			foreach (ServerClient sc in Clients)
				sc.SelectedCards.Clear();
	
			// We are now in a match
			inMatch = true;

			// Restart timer
			secondsLeft = SecondsPerTurn + (cooldown ? 4 : 0);
			timer.Restart();
			SendMessageToAll(new ServerTime(SecondsPerTurn));
		}
	}
}
