using System;
using System.ComponentModel;

namespace CSTube_Win
{
    public partial class MediaItem : INotifyPropertyChanged
	{
		public string URL { get; set; }
		public string Source { get; set; }
		public int Number { get; set; }
		public DateTime AddedDate { get; set; }

		public string Title { get; set; }
		public string Producer { get; set; }
		public string Singer { get; set; }
		public DateTime ReleaseDate { get; set; }

		public string Tags { get; set; }
		public string Related { get; set; }

		// Non-serialized temporal remote data
		public CSTube.Video _videoData;


		public double _downloadProgress;
		public bool _isDownloading;
		public bool _isDownloaded;

		public double downloadProgress
		{
			get { return _downloadProgress; }
			set
			{
				if (value != _downloadProgress)
				{
					_downloadProgress = value;
					OnPropertyChanged("downloadProgress");
				}
			}
		}
		public bool isDownloading
		{
			get { return _isDownloading; }
			set
			{
				if (value != _isDownloading)
				{
					_isDownloading = value;
					OnPropertyChanged("isDownloading");
				}
			}
		}
		public bool isDownloaded
		{
			get { return _isDownloaded; }
			set
			{
				if (value != _isDownloaded)
				{
					_isDownloaded = value;
					OnPropertyChanged("isDownloaded");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}


		public MediaItem() { }

		public MediaItem(MediaItem other)
		{
			URL = other.URL;
			Source = other.Source;
			Number = other.Number;
			AddedDate = other.AddedDate;

			Title = other.Title;
			Producer = other.Producer;
			Singer = other.Singer;
			ReleaseDate = other.ReleaseDate;

			Tags = other.Tags;
			Related = other.Related;
		}

		public static MediaItem fromCTVideo(CSTube.Video video)
		{
			MediaItem music = new MediaItem();
			music.UpdateFromCTVideo(video);
			return music;
		}

		public void UpdateFromCTVideo(CSTube.Video videoData)
		{
			_videoData = videoData;
			URL = "v=" + videoData.videoID;
			Source = "YouTube";
			
			if (!_videoData.videoDataAvailable)
			{
				Title = "No video data";
			}
			else
			{
				Title = videoData.title;
				Tags = string.Format("{0} streams; {1} captions", videoData.streams.Count(), videoData.captions.Count());
			}

			OnPropertyChanged("Title");
			OnPropertyChanged("Tags");
		}
	}
}
