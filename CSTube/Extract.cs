using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;

namespace CSTube
{
	/// <summary>
	/// This module contains all non-cipher related data extraction logic.
	/// </summary>
	public static class Extract
	{
		private static Regex
			validateVideoURL = new Regex(@"(?:/watch\?v=|youtu.be/|/v/|/embed/)([\w-]{11})", RegexOptions.Compiled),
			validatePlaylistURL = new Regex(@"(/playlist\?list=)([\w-]{34})", RegexOptions.Compiled),
			extractVideoID = new Regex(@"(?:v=|\/)([\w-]{11})", RegexOptions.Compiled),
			extractPlaylistID = new Regex(@"list=([\w-]{34})", RegexOptions.Compiled),
			checkAgeRestriction = new Regex(@"og:restrictions:age", RegexOptions.Compiled),
			extractTParamValue = new Regex("[\'|\"]t[\'|\"] ?[:|=] ?[\'|\"](.{1,5}?)[\'|\"]", RegexOptions.Compiled),
			mimeTypeSplit = new Regex(@"(\w+\/\w+)\;\scodecs=\""([a-zA-Z-0-9.,\s]*)\""", RegexOptions.Compiled),
			extractYTPlayerConfig = new Regex(@";ytplayer\.config\s*=\s*({.*?});", RegexOptions.Compiled);

		
		/// <summary>
		/// Read the YouTube player configuration (args and assets) from the JSON data embedded into the HTML page.
		/// It serves as the primary source of obtaining the stream manifest data.
		/// </summary>
		internal static YTPlayerConfig getYTPlayerConfig(string watchHTML)
		{
			string configRaw = Helpers.DoRegex(extractYTPlayerConfig, watchHTML, 1);
			if (string.IsNullOrEmpty(configRaw))
			{
				CSTube.Log("ERROR: Video is unavailable!");
				return new YTPlayerConfig();
			}

			// Get config as JSON structure
			JObject obj = Helpers.TryParseJObject(configRaw);
			JToken argsToken = obj.GetValue("args");
			JToken assetsToken = obj.GetValue("assets");

			// Create config and read it from JSON
			YTPlayerConfig config = new YTPlayerConfig();
			config.args = ObscuredContainer.FromJSONRecursive(argsToken, 10);
			config.assets = ObscuredContainer.FromJSONRecursive(assetsToken, 10);

			if (config.args == null || config.assets == null)
				CSTube.Log("ERROR: Player Config JSON is invalid!");
			return config;
		}

		/// <summary>
		/// Contruct the video info url for the specified video.
		/// </summary>
		internal static string getVideoInfoURL(string videoID, string watchHTML)
		{
			// Unidentified parameter t (probably boolean, usually 1). Format "t"="1"
			string t = Helpers.DoRegex(extractTParamValue, watchHTML, 1);

			// Compile list of parameters
			Dictionary<string, string> parameters = new Dictionary<string, string> {
				{ "video_id", videoID },
				{ "el", "$el" },
				{ "ps", "default" },
				{ "eurl", HttpUtility.UrlEncode(getWatchURL(videoID)) },
				{ "t", HttpUtility.UrlEncode(t) }
			};

			// Construct video info URL
			string parameterList = string.Join("&", parameters.Select(p => p.Key + "=" + p.Value));
			return "https://youtube.com/get_video_info?" + parameterList;
		}

		#region URLs

		/// <summary>
		/// Returns if the specified URL points to a video.
		/// This function supports the following patterns:
		/// - :samp:`https://youtube.com/watch?v={videoID}`
		/// - :samp:`https://youtube.com/embed/{videoID}`
		/// - :samp:`https://youtu.be/{videoID}`
		/// </summary>
		public static bool isVideoURL(string URL)
		{
			return validateVideoURL.IsMatch(URL);
		}

		/// <summary>
		/// Returns if the specified URL points to a playlist.
		/// 
		/// This function supports the following patterns:
		/// - :samp:`https://youtube.com/playlist?list={playlistID}`
		/// </summary>
		public static bool isPlaylistURL(string URL)
		{
			return validatePlaylistURL.IsMatch(URL);
		}

		/// <summary>
		/// Extracts the videoID out of the specified video URL.
		/// This function supports the following patterns:
		/// - :samp:`https://youtube.com/watch?v={videoID}`
		/// - :samp:`https://youtube.com/embed/{videoID}`
		/// - :samp:`https://youtu.be/{videoID}`
		/// </summary>
		public static string getVideoID(string URL)
		{
			return Helpers.DoRegex(extractVideoID, URL, 1);
		}

		/// <summary>
		/// Extracts the playlistID out of the specified playlist URL.
		/// </summary>
		public static string getPlaylistID(string URL)
		{
			return Helpers.DoRegex(extractPlaylistID, URL, 1);
		}

		/// <summary>
		/// Construct a sanitized YouTube watch url from given the videoID.
		/// </summary>
		public static string getWatchURL(string videoID)
		{
			return "https://youtube.com/watch?v=" + videoID;
		}

		/// <summary>
		/// Construct a sanitized YouTube watch url from given the videoID.
		/// </summary>
		public static string getPlaylistURL(string playlistID)
		{
			return "https://youtube.com/playlist?list=" + playlistID;
		}

		#endregion

		/// <summary>
		/// Check if the specified content is age restricted.
		/// </summary>
		internal static bool isAgeRestricted(string watchHTML)
		{
			return checkAgeRestriction.IsMatch(watchHTML);
		}

		/// <summary>
		/// Get the URL to the base JavaScript.
		/// </summary>
		internal static string getJSURL(YTPlayerConfig config)
		{
			return "https://youtube.com" + config.assets.GetValue<string>("js");
		}

		/// <summary>
		/// Split up the mime type and codec data serialized into the stream type key.
		/// 
		/// >>> splitType('video/3gpp; codecs="mp4v.20.3, mp4a.40.2"') => ('audio/webm', 'mp4v.20.3, mp4a.40.2')
		/// </summary>
		internal static Tuple<string, string> splitType(string mimeType)
		{
			return Helpers.DoRegex(mimeTypeSplit, mimeType, 1, 2);
		}
	}
}