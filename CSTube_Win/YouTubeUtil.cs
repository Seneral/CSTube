using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CSTube_Win
{
	public static class YouTubeUtil
    {
		private static PostLogger postLogger;

		#region CSTube

		public static async Task<CSTube.Video> FetchPTVideoDataAsync(MediaItem item)
		{
			CSTube.Video videoData = await CSTube.Video.Fetch (item.URL, false);
			item.UpdateFromCTVideo(videoData);
			return videoData;
		}

		public static Task FetchPTVideosDataAsync(List<MediaItem> music)
		{
			StartPostLogging(true);
			Task fetchTask = Task.Factory.StartNew(() =>
			{
				Parallel.ForEach(music, (MediaItem item) =>
				{
					CSTube.Video video = new CSTube.Video(item.URL);
					video.FetchInformation().Wait();
					item.UpdateFromCTVideo(video);
					postLogger.Log(video.ToString());
				});
			});
			return fetchTask.ContinueWith((Task t) =>
			{
				EndPostLogging();
				Debug.Log("Finished asynchronous PT video data fetch for " + music.Count + " videos!");
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<bool> DownloadAudioAsync(MediaItem media, string path = null, 
			Action<MediaItem, long, long> onProgressChanged = null, Action<MediaItem, bool> onComplete = null)
		{
			media.isDownloading = true;
			media.downloadProgress = 0;

			// Fetch video data first
			if (media._videoData == null)
				await FetchPTVideoDataAsync(media);
			CSTube.Video videoData = media._videoData;

			if (videoData.streams.Count() == 0)
			{
				Debug.Log("ERROR: No streams to download found for '" + media.Title + "'!");
				return false;
			}

			// Sort video streams to find audio stream of best quality
			CSTube.Stream bestAudio = videoData.streams.FilterStreams(onlyAudio: true).All()
				.OrderByDescending(s => Convert.ToInt32(s.getAttribute("bitrate"))).FirstOrDefault();
			if (bestAudio == null)
			{
				Debug.Log("ERROR: No single audio stream found for '" + media.Title + "'!");
				return false;
			}

			if (string.IsNullOrEmpty(path))
			{ // Get default path
				string filename = bestAudio.getFileName(media.Title);
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Downloaded_CSTube", filename);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(path));


			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(bestAudio.URL);
			using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
			{ // Start streaming to disk
				long totalBytes = response.ContentLength;
				Debug.Log(string.Format("Starting to download {0} ({1} total MB)", media.Title, (float)totalBytes / 1000 / 1000));

				// Setup wrapper for callback
				Action<long> progressReport = null;
				if (onProgressChanged != null)
					progressReport = (long bytesRead) => onProgressChanged.Invoke(media, bytesRead, totalBytes);

				using (Stream input = response.GetResponseStream())
				using (Stream output = File.OpenWrite(path))
				{ // Start streaming with progress report
					await CopyToAsync(input, output, progressReport, 8 * 0x1000);
				}
			}

			media.isDownloading = false;
			media.isDownloaded = true;
			onComplete?.Invoke(media, true);

			return true;
		}

		#endregion

		#region Utility

		public static async Task CopyToAsync(Stream source, Stream destination, Action<long> progress, int bufferSize = 0x1000)
		{
			byte[] buffer = new byte[bufferSize];
			int bytesRead;
			long totalRead = 0;
			while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				await destination.WriteAsync(buffer, 0, bytesRead);
				totalRead += bytesRead;
				progress?.Invoke(totalRead);
			}
		}

		public static void StartPostLogging(bool enclosed = false)
		{ // Replace logging with a PostLogger that holds back logging until it's back on the main thread
			if (postLogger == null)
				postLogger = new PostLogger();
			postLogger.SetEmbeddedLogger (Debug.logHandler);
			Debug.SetLogger(enclosed ? null : postLogger);
			CSTube.CSTube.SetLogger(enclosed ? null : postLogger);
		}

		public static void EndPostLogging()
		{ // Post stored logging from other threads and restore intial log handler
			postLogger.PostLog();
			Debug.SetLogger(postLogger.logHandler);
			CSTube.CSTube.SetLogger(postLogger.logHandler);
		}

		#endregion
	}
}
