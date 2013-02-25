using System;
using System.IO;
using Newtonsoft.Json;

namespace ManateesAgainstCards
{
	class Json
	{
		private const string JsonLocation = "Assets/Json/";
		private const string ResourceExt = ".json";

		public static T Load<T>(string name)
		{
			JsonSerializerSettings s = new JsonSerializerSettings
			{
				Error = (sender, arg) =>
				{
					throw new Exception(arg.ToString());
				}
			};

			T obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(JsonLocation + name + ResourceExt), s);

			if (obj.Equals(null))
				throw new Exception("Empty JSON file: " + name);

			return obj;
		}

		public static void Save<T>(string name, T obj)
		{
			File.WriteAllText(JsonLocation + name + ResourceExt, JsonConvert.SerializeObject(obj));
		}
	}
}
