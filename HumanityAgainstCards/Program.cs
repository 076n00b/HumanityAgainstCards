using System;
using Californium;

namespace ManateesAgainstCards
{
	class Program
	{
		public const string DefaultFont = "arial.ttf";
		public const string Version = "1.96";
		public static bool HandleNetworking;

		public static void Main(string[] args)
		{
			HandleNetworking = true;

			Console.WriteLine("Manatees Against Humanity; Version {0}", Version);

			// Set window resolution
			GameOptions.Caption = "Manatees Against Cards";
			GameOptions.Icon = "Icon.png";
			GameOptions.Width = 1280; // 1280
			GameOptions.Height = 720; // 720
			GameOptions.Resizable = false;
			
			Console.WriteLine("Loading cards...");

			// Load cards
			CardLoader.LoadDecks();

			// Prepare network loop
			Timer.EveryFrame(() =>
			{
				if (!HandleNetworking)
					return true;

				Network.Client.UpdateNetwork();
                Network.Server.UpdateNetwork();

				return false;
			});

			Console.WriteLine("Initializing engine...");
			Game.Initialize();

			Console.WriteLine("Running...");
			Game.SetState(new States.MainMenu());
			Game.Run();
		}
	}
}
