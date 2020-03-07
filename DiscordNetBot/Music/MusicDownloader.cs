using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using VideoLibrary;
using System.Linq;
using YoutubeExtractor;

namespace DiscordNetBot
{
    /// <summary>
    /// MusicDownloader
    /// </summary>
    public static class MusicDownloader
    {
        #region Public Properties

        public static YouTubeService YTService;

        public const string ToDelete = " - YouTube";

        #endregion

        public static void Initialize()
        {
            YTService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = ConfigurationManager.AppSettings["YTApiKey"],
                ApplicationName = "Discord.Net Bot",
            });
        }

        public static async Task<MusicYT> DownloadMusicFromYT(string URL)
        {
            var yt = YouTube.Default;
            var video = await yt.GetVideoAsync(URL).ConfigureAwait(false);
            var path = MusicHelpers.TitleToPath(video.Title.Remove(video.Title.IndexOf(ToDelete), ToDelete.Count()));
            await File.WriteAllBytesAsync(path, await video.GetBytesAsync()).ConfigureAwait(false);

            Console.WriteLine($"Downloaded {video.Title} ({video.Resolution})\n to {path}");

            return await GetMusicFromTitle(video.Title);
        }

        public static async Task<MusicYT> DownloadAudioFromYT(string URL)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(URL);
            VideoInfo video = videoInfos.Where(info => info.CanExtractAudio).OrderByDescending(info => info.AudioBitrate).First();

            if (video.RequiresDecryption)
                DownloadUrlResolver.DecryptDownloadUrl(video);

            var audioDownloader = new AudioDownloader(video, MusicHelpers.TitleToPath(video.Title));

            Console.WriteLine($"Started downloading {video.Title}. To download {audioDownloader.BytesToDownload / 1000000}MB.");
            // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
            // because the download will take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

            await Task.Run(() => audioDownloader.Execute());

            return await GetMusicFromTitle(video.Title);
        }

        public static async Task<List<MusicYT>> GetSearchResults(string search)
        {
            var searchListRequest = YTService.Search.List("snippet");
            searchListRequest.Q = search;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 5;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            var musicsResutls = new List<MusicYT>();

            foreach (var video in searchListResponse.Items)
            {
                musicsResutls.Add(video.SearchResultToMusicYT());
            }

            return musicsResutls;
        }

        public static async Task<MusicYT> GetMusicFromTitle(string title)
        {
            var searchRequest = YTService.Search.List("snippet");
            searchRequest.Q = title;
            searchRequest.MaxResults = 1;

            var searchListResponse = await searchRequest.ExecuteAsync();

            return searchListResponse.Items.First().SearchResultToMusicYT();
        }

        private static string VideoIdToURL(string Id)
        {
            return $"https://www.youtube.com/watch?v={Id}";
        }

        private static MusicYT SearchResultToMusicYT(this SearchResult video)
        {
            return new MusicYT
            {
                VideoID = video.Id.VideoId,
                Title = video.Snippet.Title,
                URL = VideoIdToURL(video.Id.VideoId),
                Path = MusicHelpers.TitleToPath(video.Snippet.Title),
                Channel = video.Snippet.ChannelTitle,
                ChannelID = video.Snippet.ChannelId,
                PublishedAt = video.Snippet.PublishedAt,
                ThumbnailURL = video.Snippet.Thumbnails.Default__.Url,
            };
        }
    }
}
