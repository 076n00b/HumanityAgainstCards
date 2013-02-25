using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;

namespace ManateesAgainstCards
{
	static class GameUtility
	{
		public static void Shuffle<T>(List<T> set)
		{
			Random rng = new Random();
			int n = set.Count;

			while (n > 1)
			{
				--n;

				int k = rng.Next(n + 1);
				T value = set[k];
				set[k] = set[n];
				set[n] = value;
			}
		}

		public static string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search, StringComparison.InvariantCulture);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static Text Wrap(string value, Font font, uint characterSize, double width)
		{
			string[] originalLines = value.Split(new[] { " " }, StringSplitOptions.None);
			List<string> wrappedLines = new List<string>();

			StringBuilder actualLine = new StringBuilder();
			double actualWidth = 0;

			foreach (var tmpItem in originalLines)
			{
				string item = tmpItem;
				item = item.Replace("_", "______________");
				item += " ";

				Text formatted = new Text(item, font) { CharacterSize = characterSize };
				double tmpWidth = formatted.GetLocalBounds().Width;

				if (actualWidth + tmpWidth >= width)
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

			if (wrappedLines.First() == "")
				wrappedLines.RemoveAt(0);

			return new Text(wrappedLines.Aggregate("", (current, line) => current + (line + "\n")), font)
			{
				CharacterSize = characterSize
			};
		}
	}
}
