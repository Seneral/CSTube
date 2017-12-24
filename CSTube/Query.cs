using System;
using System.Collections.Generic;
using System.Linq;

namespace CSTube
{
	/// <summary>
	/// This module provides a query interface for media streams.
	/// </summary>
	public class StreamQuery
	{
		private List<Stream> Streams;
		private Dictionary<int, Stream> ITagIndex;

		/// <summary>
		/// Create a new stream query to filter streams.
		/// </summary>
		public StreamQuery(List<Stream> streams)
		{
			Streams = streams;
			ITagIndex = Streams.ToDictionary(s => Convert.ToInt32(s.ITag), s => s);
		}

		/// <summary>
		/// Apply the given filtering criteria or custom filters
		/// </summary>
		public StreamQuery FilterStreams(
			int? fps = null,
			string resolution = null,
			string mimeType = null,
			string type = null,
			string subType = null,
			string bitrate = null,
			string videoCodec = null,
			string audioCodec = null,
			bool? onlyVideo = null,
			bool? onlyAudio = null,
			bool? adaptive = null,
			params Func<Stream, bool>[] customFilters)
		{
			List<Func<Stream, bool>> filters = new List<Func<Stream, bool>>();

			if (fps != null)
				filters.Add(s => s.format.fps == fps);
			if (resolution != null)
				filters.Add(s => s.format.resolution == resolution);
			if (mimeType != null)
				filters.Add(s => s.mimeType == mimeType);
			if (type != null)
				filters.Add(s => s.type == type);
			if (subType != null)
				filters.Add(s => s.subType == subType);
			if (bitrate != null)
				filters.Add(s => s.format.bitrate == bitrate);

			if (videoCodec != null)
				filters.Add(s => s.videoCodec == videoCodec);
			if (audioCodec != null)
				filters.Add(s => s.audioCodec == audioCodec);
			if (onlyVideo == true)
				filters.Add(s => s.hasVideoTrack && !s.hasAudioTrack);
			if (onlyAudio == true)
				filters.Add(s => !s.hasVideoTrack && s.hasAudioTrack);

			if (adaptive != null)
				filters.Add(s => ((bool)adaptive && s.isAdaptive) || (!(bool)adaptive && s.isProgressive));

			filters.AddRange(customFilters);

			return new StreamQuery(Streams.Where(s => filters.All(f => f(s))).ToList());
		}

		/// <summary>
		/// Get the Stream in the query for a given ITag if available.
		/// </summary>
		public Stream getByITag(int ITag)
		{
			if (ITagIndex.ContainsKey(ITag))
				return ITagIndex[ITag];
			return null;
		}

		/// <summary>
		/// Get the first Stream in the results.
		/// </summary>
		public Stream First()
		{
			return Streams.FirstOrDefault();
		}

		/// <summary>
		/// Get the last Stream in the results.
		/// </summary>
		public Stream Last()
		{
			return Streams.LastOrDefault();
		}

		/// <summary>
		/// Get the count of streams in the query.
		/// </summary>
		public int Count()
		{
			return Streams.Count;
		}

		/// <summary>
		/// Get all the results represented by this query as a list.
		/// </summary>
		public List<Stream> All()
		{
			return Streams;
		}
	}

	/// <summary>
	/// This module provides a query interface for captions.
	/// </summary>
	public class CaptionQuery
	{
		private List<Caption> Captions;
		private Dictionary<string, Caption> LangCodeIndex;

		/// <summary>
		/// Create a new caption query to filter captions.
		/// </summary>
		public CaptionQuery(List<Caption> captions)
		{
			Captions = captions;
			LangCodeIndex = Captions.ToDictionary(c => c.code, c => c);
		}

		/// <summary>
		/// Get the caption for a specified language code if available.
		/// </summary>
		public Caption getByLangCode(string langCode)
		{
			if (LangCodeIndex.ContainsKey(langCode))
				return LangCodeIndex[langCode];
			return null;
		}

		/// <summary>
		/// Get all the results represented by this query as a list.
		/// </summary>
		public List<Caption> All()
		{
			return Captions;
		}

		/// <summary>
		/// Get the count of captions in the query.
		/// </summary>
		public int Count()
		{
			return Captions.Count;
		}
	}
}