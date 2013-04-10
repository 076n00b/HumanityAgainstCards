using System;
using System.Text;
using System.Windows.Forms;
using Californium;

namespace ManateesAgainstCards
{
	class Program
	{
		public const string DefaultFont = "arial.ttf";
		public const string Version = "1.99d";
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

			// Prepare network loop
			Californium.Timer.EveryFrame(() =>
			{
				if (HandleNetworking)
				{
					Network.Client.UpdateNetwork();
					Network.Server.UpdateNetwork();
				}

				return false;
			});

#if !DEBUG
			try
			{
#endif
				// Initialize engine
				Console.Write("Initializing engine... ");
				Game.Initialize();
				Console.WriteLine("Done!");

				// Set main state
				Console.WriteLine("Initializing MainMenu state...");
				Game.SetState(new States.MainMenu());

				Console.WriteLine("Entering main loop!");
				Game.Run();
#if !DEBUG
			}
			catch(Exception e)
			{
				StringBuilder formattedException = new StringBuilder();
				formattedException.AppendLine(e.Message);
				formattedException.AppendLine("");
				formattedException.AppendLine(e.StackTrace);

				MessageBox.Show(formattedException.ToString(), "An error occurred!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#endif
		}
	}
}
