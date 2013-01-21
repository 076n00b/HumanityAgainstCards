using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace Californium
{
    public static class Utility
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

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : ((value > max) ? max : value);
        }

        public static float ToDegrees(float dir)
        {
            return dir * (180 / (float)Math.PI);
        }

        public static float ToRadians(float dir)
        {
            return dir * ((float)Math.PI / 180);
        }

        public static float Distance(Vector2f p1, Vector2f p2)
        {
            return (float)Math.Sqrt(((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y)));
        }

        public static float Direction(Vector2f p1, Vector2f p2)
        {
            var r = (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);
            return r < 0 ? r + (2 * (float)Math.PI) : r;
        }

        public static Vector2f LengthDir(float dir, float len)
        {
            return new Vector2f((float)Math.Cos(dir) * len, (float)-Math.Sin(dir) * len);
        }

        public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<KeyValuePair<TKey, TValue>, bool> match)
        {
            foreach (var cur in dict.Where(match).ToList())
            {
                dict.Remove(cur.Key);
            }
        }

        public static void Center(this Text text, bool horizontal = true, bool vertical = true)
        {
            text.Origin = new Vector2f();

            var bounds = text.GetGlobalBounds();
            bounds.Left -= text.Position.X;
            bounds.Top -= text.Position.Y;

            text.Origin = new Vector2f(bounds.Left / text.Scale.X, bounds.Top / text.Scale.Y);
            
            if (horizontal)
                text.Origin += new Vector2f((bounds.Width / text.Scale.X) / 2, 0);

            if (vertical)
                text.Origin += new Vector2f(0, (bounds.Height / text.Scale.Y) / 2);

            text.Origin.Round();
        }

        public static Vector2f Round(this Vector2f vec)
        {
            vec.X = (float)Math.Round(vec.X);
            vec.Y = (float)Math.Round(vec.Y);
            return vec;
        }
    }
}
