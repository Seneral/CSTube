using System;
using System.Collections.Generic;

namespace CSTube
{
	/// <summary>
	/// This module contains a lookup table of YouTube's itag values.
	/// </summary>
	public static class ITags
	{
		/// <summary>
		/// Get dditional format information for a given itag.
		/// </summary>
		public static FormatProfile getFormatProfile(string ITagRaw)
		{
			int itag = Convert.ToInt32(ITagRaw);

			FormatProfile profile = new FormatProfile();

			if (ITAGS.ContainsKey(itag))
			{
				Tuple<string, string> data = ITAGS[itag];
				profile.resolution = data.Item1;
				profile.bitrate = data.Item2;
			}

			profile.isLive = LIVE.Contains(itag);
			profile.is3D = _3D.Contains(itag);
			profile.fps = _60FPS.Contains(itag) ? 60 : 30;

			return profile;

		}

		private static Dictionary<int, Tuple<string, string>> ITAGS = new Dictionary<int, Tuple<string, string>>
		{
			{
				5,
				Tuple.Create("240p", "64kbps")},
			{
				6,
				Tuple.Create("270p", "64kbps")},
			{
				13,
				Tuple.Create("144p", "")},
			{
				17,
				Tuple.Create("144p", "24kbps")},
			{
				18,
				Tuple.Create("360p", "96kbps")},
			{
				22,
				Tuple.Create("720p", "192kbps")},
			{
				34,
				Tuple.Create("360p", "128kbps")},
			{
				35,
				Tuple.Create("480p", "128kbps")},
			{
				36,
				Tuple.Create("240p", "")},
			{
				37,
				Tuple.Create("1080p", "192kbps")},
			{
				38,
				Tuple.Create("3072p", "192kbps")},
			{
				43,
				Tuple.Create("360p", "128kbps")},
			{
				44,
				Tuple.Create("480p", "128kbps")},
			{
				45,
				Tuple.Create("720p", "192kbps")},
			{
				46,
				Tuple.Create("1080p", "192kbps")},
			{
				59,
				Tuple.Create("480p", "128kbps")},
			{
				78,
				Tuple.Create("480p", "128kbps")},
			{
				82,
				Tuple.Create("360p", "128kbps")},
			{
				83,
				Tuple.Create("480p", "128kbps")},
			{
				84,
				Tuple.Create("720p", "192kbps")},
			{
				85,
				Tuple.Create("1080p", "192kbps")},
			{
				91,
				Tuple.Create("144p", "48kbps")},
			{
				92,
				Tuple.Create("240p", "48kbps")},
			{
				93,
				Tuple.Create("360p", "128kbps")},
			{
				94,
				Tuple.Create("480p", "128kbps")},
			{
				95,
				Tuple.Create("720p", "256kbps")},
			{
				96,
				Tuple.Create("1080p", "256kbps")},
			{
				100,
				Tuple.Create("360p", "128kbps")},
			{
				101,
				Tuple.Create("480p", "192kbps")},
			{
				102,
				Tuple.Create("720p", "192kbps")},
			{
				132,
				Tuple.Create("240p", "48kbps")},
			{
				151,
				Tuple.Create("720p", "24kbps")},
			{
				133,
				Tuple.Create("240p", "")},
			{
				134,
				Tuple.Create("360p", "")},
			{
				135,
				Tuple.Create("480p", "")},
			{
				136,
				Tuple.Create("720p", "")},
			{
				137,
				Tuple.Create("1080p", "")},
			{
				138,
				Tuple.Create("2160p", "")},
			{
				160,
				Tuple.Create("144p", "")},
			{
				167,
				Tuple.Create("360p", "")},
			{
				168,
				Tuple.Create("480p", "")},
			{
				169,
				Tuple.Create("720p", "")},
			{
				170,
				Tuple.Create("1080p", "")},
			{
				212,
				Tuple.Create("480p", "")},
			{
				218,
				Tuple.Create("480p", "")},
			{
				219,
				Tuple.Create("480p", "")},
			{
				242,
				Tuple.Create("240p", "")},
			{
				243,
				Tuple.Create("360p", "")},
			{
				244,
				Tuple.Create("480p", "")},
			{
				245,
				Tuple.Create("480p", "")},
			{
				246,
				Tuple.Create("480p", "")},
			{
				247,
				Tuple.Create("720p", "")},
			{
				248,
				Tuple.Create("1080p", "")},
			{
				264,
				Tuple.Create("144p", "")},
			{
				266,
				Tuple.Create("2160p", "")},
			{
				271,
				Tuple.Create("144p", "")},
			{
				272,
				Tuple.Create("2160p", "")},
			{
				278,
				Tuple.Create("144p", "")},
			{
				298,
				Tuple.Create("720p", "")},
			{
				299,
				Tuple.Create("1080p", "")},
			{
				302,
				Tuple.Create("720p", "")},
			{
				303,
				Tuple.Create("1080p", "")},
			{
				308,
				Tuple.Create("1440p", "")},
			{
				313,
				Tuple.Create("2160p", "")},
			{
				315,
				Tuple.Create("2160p", "")},
			{
				139,
				Tuple.Create("", "48kbps")},
			{
				140,
				Tuple.Create("", "128kbps")},
			{
				141,
				Tuple.Create("", "256kbps")},
			{
				171,
				Tuple.Create("", "128kbps")},
			{
				172,
				Tuple.Create("", "256kbps")},
			{
				249,
				Tuple.Create("", "50kbps")},
			{
				250,
				Tuple.Create("", "70kbps")},
			{
				251,
				Tuple.Create("", "160kbps")},
			{
				256,
				Tuple.Create("", "")},
			{
				258,
				Tuple.Create("", "")},
			{
				325,
				Tuple.Create("", "")},
			{
				328,
				Tuple.Create("", "")}
		};

		private static List<int> _60FPS = new List<int> {
			298,
			299,
			302,
			303,
			308,
			315
		};

		private static List<int> _3D = new List<int> {
			82,
			83,
			84,
			85,
			100,
			101,
			102
		};

		private static List<int> LIVE = new List<int> {
			91,
			92,
			93,
			94,
			95,
			96,
			132,
			151
		};
	}
}