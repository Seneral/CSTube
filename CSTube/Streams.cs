using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace CSTube
{
	/// <summary>
	/// Contains decoded information of a single video or audio stream of a YouTube video.
	/// Also provides option to directly download the stream.
	/// </summary>
	public class Stream
	{
		// Raw stream data
		private ObscuredContainer stream;

		// High-Level decoded stream info
		public string ITag, Abr, URL;
		public FormatProfile format;

		// Decorded stream type
		public string mimeType;
		public string type, subType;

		// Decorded stream codec
		public List<string> codecs;
		public string videoCodec, audioCodec;

		// Whether stream contains audio and video combined (progressive) or not (adaptive)
		public bool isAdaptive { get { return codecs.Count % 2 == 1; } }
		public bool isProgressive { get { return !isAdaptive; } }

		// Whether stream contains a specific track type
		public bool hasAudioTrack { get { return isProgressive || type == "audio"; } }
		public bool hasVideoTrack { get { return isProgressive || type == "video"; } }

		/// <summary>
		/// Decodes stream data found in the specified raw container into an object.
		/// </summary>
		internal Stream(ObscuredContainer streamContainer)
		{
			stream = streamContainer;

			// Read parameters ITag, abr and URL
			ReadAttributeValues(stream);

			// Read format information from tables using ITag
			format = ITags.getFormatProfile(ITag);

			// 'video/webm; codecs="vp8, vorbis"' -> 'video/webm', 'vp8, vorbis'
			Tuple<string, string> typeSplit = Extract.splitType(type);

			// Seperate type info
			mimeType = typeSplit.Item1;
			string[] typeInfo = mimeType.Split('/');
			type = typeInfo[0];
			subType = typeInfo[1];

			// Read and parse codecs (seperate audio and video)
			codecs = typeSplit.Item2.Split(',').Select(c => c.Trim()).ToList();
			videoCodec = isProgressive ? codecs[0] : (type == "video" ? codecs[0] : null);
			audioCodec = isProgressive ? codecs[1] : (type == "audio" ? codecs[0] : null);
		}

		/// <summary>
		/// Read value of the give attribute collection into the class members
		/// Affects type, URL, ITag and Abr
		/// </summary>
		private void ReadAttributeValues(ObscuredContainer attributes)
		{
			if (attributes.ContainsKey("type"))
				type = attributes.GetValue<string>("type");

			if (attributes.ContainsKey("url"))
				URL = attributes.GetValue<string>("url");

			if (attributes.ContainsKey("itag"))
				ITag = attributes.GetValue<string>("itag");

			if (attributes.ContainsKey("abr"))
				Abr = attributes.GetValue<string>("abr");
		}

		/// <summary>
		/// Retrieves an arbitrary attribute from the raw stream manifest.
		/// </summary>
		public string getAttribute(string path)
		{
			return Helpers.TryGetAttribute<string>(stream, path);
		}


		/// <summary>
		/// Fetches the file size in bytes from the URL.
		/// </summary>
		public int fetchFileSize ()
		{
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
			req.Method = "HEAD";
			using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
				return Convert.ToInt32(response.Headers["content-length"]);
		}

		/// <summary>
		/// Generate a filename based on the video title.
		/// </summary>
		public string getFileName (string title)
		{
			return Helpers.checkFilename(title) + "." + subType;
		}

		/// <summary>
		/// Write the media stream to the specified path.
		/// </summary>
		public void Download(string path)
		{
			CSTube.Log(string.Format("Downloading File to {0}", path));

			Directory.CreateDirectory(Path.GetDirectoryName(path));

			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
			using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
			{
				CSTube.Log(string.Format("Total file size in bytes: {0}", response.Headers["content-length"]));
				using (System.IO.Stream input = response.GetResponseStream())
				using (System.IO.Stream output = File.OpenWrite(path))
					input.CopyToAsync(output).Wait();
			}
		}

		/// <summary>
		/// Converts this stream's info into a human readable format.
		/// </summary>
		public override string ToString()
		{
			List<string> parts = new List<string> {
				"itag=\"{0}\"",
				"mime_type=\"{1}\""
			};
			if (hasVideoTrack)
			{
				parts.Add("res =\"{2}\"");
				parts.Add("fps=\"{3}fps\"");
				parts.Add("vcodec=\"{4}\"");
			}
			if (hasAudioTrack)
			{
				if (!hasVideoTrack)
					parts.Add("abr=\"{6}\"");
				parts.Add("acodec=\"{5}\"");
			}
			parts.Add("url=\"{7}\"");
			return "<Stream: " + string.Format(string.Join(" ", parts), 
				ITag, mimeType, format.resolution, format.fps, videoCodec, audioCodec, Abr, URL) + ">";
		}
	}
}