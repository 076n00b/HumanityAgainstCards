namespace ManateesAgainstCards.Network
{
	public static class Constants
	{
		public const int DefaultPort = 1890;
		public const int MinimalPlayerCount = 3;
		public const int MaxPlayerCount = 6;
		public const string RemoteServerListAddress = "http://www.x2048.com/mac/";
	}

	public enum PacketType : byte
	{
		ServerJoin,
		ChatMessage,
		LobbyBeginGame,
		PlayerNew,
		PlayerDelete,
		BlackCard,
		WhiteCard,
		EndTurn,
		WinnerPicked,
		GameOver,
		SelectCardCzar,
		InitSelectionScreen,
		DeclareWinner,
		SetStatus,
		ServerTime,

		Invalid = 255
	}

	public enum SessionRole
	{
		Server,
		Client
	}
}
