using System;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CSTube
{
	/// <summary>
	/// Various helper functions for use in CSTube.
	/// </summary>
	public static class Helpers
	{
		#region Regex

		/// <summary>
		/// Shortcut method to search a string for a given pattern.   
		/// </summary>
		internal static string DoRegex(string pattern, string str, int group, RegexOptions option = RegexOptions.None)
		{
			return DoRegex(new Regex(pattern, option), str, group);
		}

		/// <summary>
		/// Shortcut method to search a string for a given pattern.   
		/// </summary>
		internal static string DoRegex(Regex regex, string str, int group)
		{
			Match match = regex.Match(str);
			return match.Success && match.Groups.Count > group ? match.Groups[group].Value : "";
		}

		/// <summary>
		/// Shortcut method to search a string for a given pattern.   
		/// </summary>
		internal static Tuple<string, string> DoRegex(string pattern, string str, int group1, int group2, RegexOptions option = RegexOptions.None)
		{
			return DoRegex(new Regex(pattern, option), str, group1, group2);

		}

		/// <summary>
		/// Shortcut method to search a string for a given pattern.   
		/// </summary>
		internal static Tuple<string, string> DoRegex(Regex regex, string str, int group1, int group2)
		{
			Match match = regex.Match(str);
			return new Tuple<string, string>(
				match.Success && match.Groups.Count > group1 ? match.Groups[group1].Value : "",
				match.Success && match.Groups.Count > group2 ? match.Groups[group2].Value : "");

		}

		#endregion

		#region IO

		/// <summary>
		/// Reads all HTML from the specified URL synchronously.
		/// </summary>
		public static async Task<string> ReadHTML(string URL)
		{
			string result;
			WebRequest req = WebRequest.Create(URL);
			req.Method = "GET";
			try
			{
				using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
					result = await reader.ReadToEndAsync();
			}
			catch (WebException e)
			{
				CSTube.Log("ERROR: Request Timeout, check your internet connection!");
				return null;
			}
			return result;
		}

		/// <summary>
		/// Sanitize a string making it safe to use as a filename.
		/// </summary>
		public static string checkFilename(string name, int maxLength = 255)
		{
			name = Regex.Replace(name, @"\W", "", RegexOptions.IgnoreCase);
			name = name.Length > maxLength ? name.Substring(0, maxLength) : name;
			return name;
		}

		#endregion

		#region JSON

		/// <summary>
		/// Tries to parse the given JSON string into a JObject
		/// </summary>
		internal static JObject TryParseJObject(string JSON)
		{
			try { return JObject.Parse(JSON); }
			catch (Exception e) { return null; }
		}

		/// <summary>
		/// Tries to parse the given JSON string into a JArray
		/// </summary>
		internal static JArray TryParseJArray(string JSON)
		{
			try { return JArray.Parse(JSON); }
			catch (Exception e) { return null; }
		}

		/// <summary>
		/// Retrieves an arbitrary attribute from the specified container.
		/// Path separated by / and \ expects respective container tree.
		/// Returns default(T) on failure to retrive the attribute.
		/// </summary>
		public static T TryGetAttribute<T>(ObscuredContainer container, string path)
		{
			string[] cmd = path.Split('\\', '/');
			ObscuredContainer cur = container;
			for (int i = 0; i < cmd.Length - 1; i++)
			{ // Follows the specified path down the container hierarchy if possible
				cur = cur.GetValue<ObscuredContainer>(cmd[i]);
				if (cur == null) return default(T);
			}
			return cur.GetValue<T>(cmd[cmd.Length - 1]);
		}

		#endregion
	}
}