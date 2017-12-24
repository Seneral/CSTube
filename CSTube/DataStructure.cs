using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CSTube
{
	/// <summary>
	/// Container to represent a dynamic JSON Datastructure
	/// </summary>
	public class ObscuredContainer : Dictionary<string, object>
	{
		public ObscuredContainer() : base() { }

		public ObscuredContainer(IDictionary<string, object> dict) : base(dict) { }

		public static ObscuredContainer FromDictionary(Dictionary<string, object> dict)
		{
			return new ObscuredContainer(dict);
		}

		public Dictionary<string, T> ToDictionary<T>()
		{
			Dictionary<string, T> dict = new Dictionary<string, T>();
			foreach (KeyValuePair<string, object> pair in this)
				dict.Add(pair.Key, (T)pair.Value);
			return dict;
		}

		/// <summary>
		/// Tries to get the value of the specified type from the ID.
		/// If ID does not exist or it is not of type T, returns default(T) (null for classes)
		/// </summary>
		public T GetValue<T>(string id)
		{
			if (!ContainsKey(id))
				return default(T);
			object val = this[id];
			if (!(val is T))
				return default(T);
			return (T)val;
		}

		/// <summary>
		/// Tries to convert the tree of the given JToken into a ObscuredContainer tree.
		/// Values are either own ObscuredContainer or string values, building a tree.
		/// 
		/// Usually assumes jToken to be a JObject or JArray.
		/// If it is not, tries to find an encapsulated structure (quoted & escaped) and parses that.
		/// If no such structure was found, returns null!
		/// </summary>
		public static ObscuredContainer FromJSONRecursive(JToken jToken, int depth)
		{
			if (jToken == null || depth <= 0 || !jToken.HasValues)
				return null;
			if (string.IsNullOrWhiteSpace(jToken.ToString(Newtonsoft.Json.Formatting.None)))
				return null;

			if (jToken.Type == JTokenType.Object)
			{ // Convert the JObject
				JObject jObject = (JObject)jToken;
				ObscuredContainer objectContainer = new ObscuredContainer();
				foreach (KeyValuePair<string, JToken> cToken in jObject)
				{ // Interpret the child tokens either as string values or as own ObscureContainers
					string value = cToken.Value.ToString(Newtonsoft.Json.Formatting.None).Trim(' ', '\"');
					object childObject = FromJSONRecursive(cToken.Value, depth - 1);
					objectContainer.Add(cToken.Key, childObject ?? value);
				}
				return objectContainer;
			}

			if (jToken.Type == JTokenType.Array)
			{ // Convert all JObject children of the JArray
				JArray jArray = (JArray)jToken;
				if (jArray.First.Type != JTokenType.Object)
					return null;

				int counter = 0;
				ObscuredContainer arrayContainer = new ObscuredContainer();
				foreach (JObject cArrObject in jArray.Children<JObject>())
				{ // Parse array children as own containers and encapsulate them
					ObscuredContainer cArrContainer = FromJSONRecursive(cArrObject, depth - 1);
					arrayContainer.Add((counter++).ToString(), cArrContainer);
				}
				return arrayContainer;
			}


			// Token is NOT a JObject -> Try to find an encapsulated structure in the token
			string rawValue = jToken.ToString(Newtonsoft.Json.Formatting.None).Trim(' ', '\"');
			string safeValue = Regex.Unescape(rawValue);

			// Check if it is an embedded JObject
			if (safeValue.StartsWith("{") && safeValue.EndsWith("}") && safeValue.Length > 2)
				return FromJSONRecursive(Helpers.TryParseJObject(safeValue), depth - 1);
			
			// Check if it is an embedded JArray
			if (safeValue.StartsWith("[") && safeValue.EndsWith("]"))
				return FromJSONRecursive(Helpers.TryParseJArray(safeValue), depth - 1);

			return null;
		}
	}

	/// <summary>
	/// Stores youtube player configuration (raw JSON representation)
	/// </summary>
	public struct YTPlayerConfig
	{
		public ObscuredContainer args;
		public ObscuredContainer assets;
	}

	/// <summary>
	/// Stores format of a stream
	/// </summary>
	public struct FormatProfile
	{
		public string resolution, bitrate;
		public bool isLive, is3D;
		public int fps;
	}
}
