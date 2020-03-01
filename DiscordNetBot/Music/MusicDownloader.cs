using System;
using System.IO;
using System.Threading.Tasks;
using VideoLibrary;

namespace DiscordNetBot
{
    /// <summary>
    /// MusicDownloader
    /// </summary>
    public static class MusicDownloader
    {

        public static async Task<YouTubeVideo> DownloadMusicFromYT(string URL)
        {
            var yt = YouTube.Default;
            var video = await yt.GetVideoAsync(URL).ConfigureAwait(false);
            var path = MusicHelpers.TitleToPath(video.Title);
            await File.WriteAllBytesAsync(path, await video.GetBytesAsync());

            Console.WriteLine($"Downloaded {video.Title} ({video.Resolution}) to {path}");

            return video;
        }
    }
}
