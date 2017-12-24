using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CSTube_Win
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public List<MediaItem> curMediaList = new List<MediaItem>();
		private bool startedMassDownload;

		private static Logger logHandler;

		public MainWindow()
		{
			InitializeComponent();
			UpdateMediaList();

			logHandler = new UILogger(logPanel);
			CSTube.CSTube.SetLogger(logHandler);
			Debug.SetLogger(logHandler);

		}

		/// <summary>
		/// Update the UI media list with either the current working media list or the whole database
		/// </summary>
		public void UpdateMediaList()
		{
			downloadList.ItemsSource = null;
			downloadList.ItemsSource = curMediaList;
		}


		// --- BUTTON HANDLERS ---

		private void OnClearData(object sender, RoutedEventArgs e)
		{
			//curMediaList.Clear ();
			UpdateMediaList();
			Debug.ClearLog();
		}

		private async void OnFetchFromURL(object sender, RoutedEventArgs e)
		{
			string URL = URLInputTextBox.Text;
			if (CSTube.Extract.isVideoURL(URL))
			{ // Record as music item
				CSTube.Video video = new CSTube.Video(URL);
				MediaItem item = MediaItem.fromCTVideo(video);
				curMediaList.Add(item);
				UpdateMediaList();
				// Fetch music data and update list once ready
				await video.FetchInformation();
				item.UpdateFromCTVideo(video);
				UpdateMediaList();
			}
			else if (CSTube.Extract.isPlaylistURL(URL))
			{ // Fetch playlist items and record them, then continuously update them
				Debug.Log("Cannot load all playlist videos yet, Google API implementation missing!");
			}
			else
				Debug.Log("ERROR: URL does not point to a valid youtube video!");

		}

		private void OnStartMassDownloadAudio(object sender, RoutedEventArgs e)
		{
			if (curMediaList == null || curMediaList.Count == 0)
			{
				Debug.Log("No videos to download!");
				return;
			}

			// Initiate mass downloading
			startedMassDownload = true;
			ContinueMassDownloadAudio();
		}

		private void ContinueMassDownloadAudio()
		{
			if (!startedMassDownload)
				return;

			while (curMediaList.Count(m => m.isDownloading) < 5)
			{
				// Try to fetch next music item to download
				MediaItem nextItem = curMediaList.FirstOrDefault (m => !m.isDownloaded && !m.isDownloading);

				if (nextItem == null)
				{ // Finished initiating mass downloads (still some downloading)
					startedMassDownload = false;
					break;
				}

				// Start task to populate all media items with additional video information
				YouTubeUtil.DownloadAudioAsync(nextItem, null, OnDownloadProgressChanged, OnDownloadCompleted);
			}
		}

		private void OnFetchVideoData(object sender, RoutedEventArgs e)
		{
			MediaItem item = (sender as Button).DataContext as MediaItem;
			if (item != null) // Get item as button data context
			{ // Start asynchronous data download with callbacks
				YouTubeUtil.DownloadAudioAsync(item, null, OnDownloadProgressChanged, OnDownloadCompleted);
			}
			else
				Debug.Log("INTERNAL ERROR: Button context is not a MediaItem!");
		}

		private void OnDownloadProgressChanged(MediaItem item, long bytesRead, long totalBytes)
		{
			item.downloadProgress = (double)bytesRead / totalBytes;
		}

		private void OnDownloadCompleted(MediaItem item, bool success)
		{
			if (!success)
				Debug.Log("ERROR: Failed to download " + item.Title + "!");
			else
				Debug.Log("Successfully downloaded " + item.Title + "!");
			ContinueMassDownloadAudio();
		}

		private void OnRemoveMediaItem(object sender, RoutedEventArgs e)
		{
			MediaItem item = (sender as Button).DataContext as MediaItem;
			if (item != null) // Get item as button data context
			{
				curMediaList.Remove(item);
				UpdateMediaList();
			}
			else
				Debug.Log("INTERNAL ERROR: Button context is not a MediaItem!");
		}
	}
}
