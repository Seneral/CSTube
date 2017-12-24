using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSTube
{

	/// <summary>
	/// Main class of CSTube serving as the main developer API.
	/// An instance represents a youtube video, capable of fetching all information on construction (or later).
	/// Most work is outsourced, 
	/// </summary>
	public class Video
	{
		// Public video information
		public string videoID, watchURL;
		public bool videoDataAvailable = false;
		public YTPlayerConfig playerConfig;

		// Internal intermediate video data
		private string watchHTML;
		private string jsSRC;
		//private string videoInfoRAW;
		//private ObscuredContainer videoInfo;

		// Extracted and parsed video data
		private List<Stream> formatStreams = new List<Stream>();
		private List<Caption> captionTracks = new List<Caption>();

		// Wrapper for video data for immediate querying
		public StreamQuery streams { get { return new StreamQuery(formatStreams); } }
		public CaptionQuery captions { get { return new CaptionQuery(captionTracks); } }

		// Additional information about the video
		public string title { get { return !videoDataAvailable ? "Video data unavailable!" : Regex.Unescape(playerConfig.args.GetValue<string>("title")); } }
		public string thumbnailURL { get { return !videoDataAvailable ? "" : Regex.Unescape(playerConfig.args.GetValue<string>("thumbnail_url")); } }


		/// <summary>
		/// Creates a new Youtube video inspector.
		/// If defeFetch is false, automatically fetches video information (many HTTP fetches) synchronously.
		/// </summary>
		public Video(string url)
		{
			videoID = Extract.getVideoID(url);
			watchURL = Extract.getWatchURL(videoID);
		}

		/// <summary>
		/// Creates a new Youtube video inspector and automatically fetches video information asynchronously.
		/// </summary>
		public static async Task<Video> Fetch(string url, bool log = true)
		{
			CSTube.SetLogState(log);
			Video video = new Video(url);
			await video.FetchInformation();
			CSTube.SetLogState(true);
			return video;
		}

		/// <summary>
		/// Fetches the information about this video (including available streams and captions).
		/// Conains two synchronous HTTP fetches and several descrambling and signing algorithms.
		/// </summary>
		public async Task FetchInformation()
		{
			CSTube.Log("Fetching information of video " + videoID);

			// Page HTML
			watchHTML = await Helpers.ReadHTML(watchURL);
			if (string.IsNullOrEmpty(watchHTML))
				return;

			// PlayerConfig JSON
			playerConfig = Extract.getYTPlayerConfig(watchHTML);

			if (playerConfig.args == null || playerConfig.assets == null)
				return;

			// JavaScript Source
			string jsURL = Extract.getJSURL(playerConfig);
			jsSRC = await Helpers.ReadHTML(jsURL);

			// VideoInfo Raw Data
			//string videoInfoURL = Extract.getVideoInfoURL(videoID, watchURL, watchHTML);
			//videoInfoRAW = Helpers.ReadHTML(videoInfoURL);

			CSTube.Log("Finished downloading information! Continuing to parse!");

			// Parse Raw video info data
			//System.Collections.Specialized.NameValueCollection p = HttpUtility.ParseQueryString(videoInfoRAW);
			//videoInfo = ObscuredContainer.FromDictionary(p.AllKeys.ToDictionary(k => k, k => (object)p[k]));


			// Get all stream formats this video has (progressive and adaptive)
			List<string> streamMaps = new List<string>();
			streamMaps.Add("url_encoded_fmt_stream_map");
			if (playerConfig.args.ContainsKey("adaptive_fmts"))
				streamMaps.Add("adaptive_fmts");
			foreach (string streamFormat in streamMaps)
			{
				// Descramble stream data in player args
				ObscuredContainer streamBundle = Mixins.ApplyDescrambler(playerConfig.args, streamFormat);

				// If required, apply signature to the stream URLs
				Mixins.ApplySignature(streamBundle, jsSRC);

				// Write stream data into Stream objects
				foreach (object streamData in streamBundle.Values)
					formatStreams.Add(new Stream((ObscuredContainer)streamData));
			}


			// Try to read out captionTracks if existant
			ObscuredContainer captionTrackBundle =
				Helpers.TryGetAttribute<ObscuredContainer>(playerConfig.args,
					"player_response/captions/playerCaptionsTracklistRenderer/captionTracks");
			if (captionTrackBundle != null)
			{ // Write caption tracks into Caption objects
				foreach (object captionTrack in captionTrackBundle.Values)
					captionTracks.Add(new Caption((ObscuredContainer)captionTrack));
			}

			videoDataAvailable = true;

			// Log success!
			CSTube.Log(string.Format(
				"Finished parsing video data! Found {0} video streams and {1} caption tracks for video '{2}'!",
				formatStreams.Count, captionTracks.Count, title
				));
			CSTube.Log(string.Format("Video Streams: \n \t " +
				string.Join(" \n \t ", formatStreams.Select(s => s.ToString()))
			));
			if (captionTracks.Count > 0)
			{
				CSTube.Log(string.Format("Caption Tracks: \n \t " +
					string.Join(" \n \t ", captionTracks.Select(s => s.ToString()))
				));
			}
		}


		/// <summary>
		/// Converts this video's info into a human readable format.
		/// </summary>
		public override string ToString()
		{
			return !videoDataAvailable ? "Video " + videoID + " unavailable!" :
				string.Format(
				"Video: '{0}'; {1} streams and {2} caption tracks",
				title, formatStreams.Count, captionTracks.Count
			);
		}
	}
}