namespace ManateesAgainstCards.Network
{
	public static class Constants
	{
		public const int DefaultPort = 1890;
		public const int MinimalPlayerCount = 2;
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
