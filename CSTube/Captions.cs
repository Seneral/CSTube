using System;
using System.Collections.Generic;
using System.Xml;
using System.Web;
using System.Threading.Tasks;

namespace CSTube
{
	/// <summary>
	/// This module contrains a container for caption tracks.
	/// </summary>
	public class Caption
	{
		public string url, name, code;

		private string cachedXMLCaptions = null;

		/// <summary>
		/// Construct a Caption and reads meta-data (no HTTP fetch)
		/// </summary>
		public Caption(ObscuredContainer captionTrack)
		{
			url = captionTrack.GetValue<string>("baseUrl");
			name = captionTrack.GetValue<ObscuredContainer>("name").GetValue<string>("simpleText");
			code = captionTrack.GetValue<string>("languageCode");
		}


		/// <summary>
		/// Converts this caption's info into a human readable format.
		/// </summary>
		public override string ToString()
		{
			return string.Format("<Caption lang=\"{0}\" code=\"{1}\">", name, code);
		}


		/// <summary>
		/// Loads and caches the XML captions (HTTP Fetch)
		/// </summary>
		public async Task<string> loadXMLCaptions()
		{
			return cachedXMLCaptions ?? (cachedXMLCaptions = await Helpers.ReadHTML(url));
		}

		/// <summary>
		/// Load XML captions (HTTP Fetch) and convert them to "SubRip Subtitle" captions
		/// </summary>
		public string generateSRTCaptions()
		{
			return XMLtoSRTCaption(loadXMLCaptions().Result);
		}



		/// <summary>
		/// Convert xml caption tracks to "SubRip Subtitle (srt)".
		/// </summary>
		public static string XMLtoSRTCaption(string xmlCaptions)
		{
			List<string> segments = new List<string>();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlCaptions);

			for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
			{
				XmlNode child = doc.DocumentElement.ChildNodes.Item(i);
				string caption = HttpUtility.UrlDecode(child.InnerText.Replace("\n", " ").Replace("  ", " "));
				float duration = float.Parse(child.Attributes["dur"].Value);
				float start = float.Parse(child.Attributes["start"].Value);
				float end = start + duration;
				string line = string.Format("{0}\n{1} --> {2}\n{3}\n",
					i + 1, ToSRTTimeFormat(start), ToSRTTimeFormat(end), caption);
				segments.Add(line);
			}
			return string.Join("/n", segments).Trim();
		}

		/// <summary>
		/// Convert decimal durations into proper SubRip Subtitle (str) format.
		/// 3.89 --> '00:00:03,890'
		/// </summary>
		private static string ToSRTTimeFormat(double d)
		{
			TimeSpan time = TimeSpan.FromSeconds(d);
			return time.ToString(@"hh\:mm\:ss,fff");
		}
	}
}