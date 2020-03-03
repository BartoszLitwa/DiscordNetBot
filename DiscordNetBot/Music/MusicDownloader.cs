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

            Console.WriteLine($"Downloaded {video.Title} ({video.Resolution}) to {path}");

            return await GetMusicFromTitle(video.Title);
        }

        public static async Task<List<MusicYT>> GetSearchResults(string search)
        {
            var searchListRequest = YTService.Search.List("snippet");
            searchListRequest.Q = search;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
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
