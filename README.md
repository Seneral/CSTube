CStube
======

*CSTube* is a lightweight, boilerplate-code-free port of the python library pytube for downloading YouTube Videos

Description
===========

See pytube repo for API and usage.
CSTube offers the same functionality as pytube but in the C# API using the .NET Framework.
It comes with a sample WPF application able to download audio from multiple videos at once.
(You need to enter each single URL though because I removed the Google API part for playlist fetching).

Features
--------

- Support for Both Progressive & DASH Streams
- Complete async interface
- As for complete download interfaces inlcuding progress/complete callbacks, see CSTube_Win/YouTubeUtil.cs
- Caption Track Support
- Outputs Caption Tracks to .srt format (SubRip Subtitle)
- Ability to Capture Thumbnail URL.
- Extensively Documented Source Code

Compared to pytube:
- Adds dependency to NewtonSoft JSON 
	(can be worked around but it is needed either way for Google API)
- No command line interface
- Complete WPF sample application

Installation
------------

Clone repository.
Now you can embed CSTube as a shared application in any other VS project or just copy out the source files and embed them directly.
No fancy setup procedure, sorry:)

Getting started
---------------

See pytube readme - it is a similar interface.
Also refer to the sample application!

Basically, create a new CSTube.Video by passing the URL into the constructor, this only initializes the object and video ID.
To parse it and fetch all information:
	
	CSTube.Video = new CSTube.Video(URL);
	// Async:
	await Video.FetchInformation();
	// Sync:
	Video.FetchInformation.Wait();

Then you can access all video information. Similar to pytube, to get the best-quality qudio stream:

	CSTube.Stream bestAudio = video.streams.FilterStreams(onlyAudio: true).All()
		.OrderByDescending(s => Convert.ToInt32(s.getAttribute("bitrate"))).FirstOrDefault();
