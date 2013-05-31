using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Californium;
using SFML.Graphics;

namespace ManateesAgainstCards
{
	static class GameUtility
	{
		public static string GetClipboardText()
		{
			string value = "";

			ThreadStart threadMethod =
			delegate
			{
				try
				{
					value = Clipboard.GetText(TextDataFormat.Text);
				}
				catch (Exception)
				{
					Console.WriteLine("Cannot get clipboard text!");
				}
			};

			Thread staThread = new Thread(threadMethod);
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.Start();
			staThread.Join();
			staThread.Abort();

			return value;
		}

		public static bool PlayTaunt(string value)
		{
			string[] values = value.Split(new[] { ' ' });

			if (values.Length == 0 || !values[0].Contains(":"))
				return false;

			foreach (string v in values)
			{
				Int32 number;

				if (Int32.TryParse(v, out number) == false)
					continue;

				string[] files = Directory.GetFiles(GameOptions.SoundLocation + "Taunt");
				foreach (string file in files.Select(Path.GetFileName))
				{
					Int32 fileNumber;
					if (!Int32.TryParse(file.Remove(2, file.Length - 2), out fileNumber))
						continue;

					if (number != fileNumber)
						continue;

					Assets.PlaySound("Taunt\\" + file);
					return true;
				}
			}

			return false;
		}

		public static void Shuffle<T>(List<T> set)
		{
			Random rng = new Random();
			int n = set.Count;

			while (n > 1)
			{
				int k = rng.Next(--n + 1);
				T value = set[k];
				set[k] = set[n];
				set[n] = value;
			}
		}

		public static string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search, StringComparison.InvariantCulture);
			return pos < 0 ? text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static Text Wrap(string value, Font font, uint characterSize, double width, bool expandUnderscores = true)
		{
			float spaceWidth = font.GetGlyph((uint)Char.ConvertToUtf32("_", 0), characterSize, false).Advance;

			string[] originalLines = Regex.Split(value, "(?<=[ \n])");
			List<string> wrappedLines = new List<string>();

			StringBuilder actualLine = new StringBuilder();
			double actualWidth = 0.0;

			foreach (var tmpItem in originalLines)
			{
				string item = tmpItem;
				if (expandUnderscores)
					item = item.Replace("_", "______________");

				Text formatted = new Text(item, font) { CharacterSize = characterSize };
				double tmpWidth = formatted.GetGlobalBounds().Width + spaceWidth;

				if (actualWidth + tmpWidth < width && item.Contains("\n"))
				{
					actualLine.Append(item.Trim(new[] { '\n' }));
					wrappedLines.Add(actualLine.ToString());
					actualLine.Clear();
					actualWidth = 0;
				}
				else if (actualWidth + tmpWidth >= width)
				{
					wrappedLines.Add(actualLine.ToString());
					actualLine.Clear();

					actualLine.Append(item);
					actualWidth = tmpWidth;
				}
				else
				{
					actualLine.Append(item);
					actualWidth += formatted.GetLocalBounds().Width;
				}
			}

			if (actualLine.Length > 0)
				wrappedLines.Add(actualLine.ToString());

			if (String.IsNullOrEmpty(wrappedLines.First()))
				wrappedLines.RemoveAt(0);

			return new Text(wrappedLines.Aggregate("", (current, line) => current + (line + "\n")), font)
			{
				CharacterSize = characterSize
			};
		}
	}
}
