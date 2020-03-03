using System;

namespace DiscordNetBot
{
    /// <summary>
    /// MusicYT
    /// </summary>
    public class MusicYT
    {
        #region Public Properties

        public string VideoID { get; set; }

        public string Title { get; set; }

        public string URL { get; set; }

        public string Path { get; set; }

        public string Channel { get; set; }

        public string ChannelID { get; set; }

        public DateTime? PublishedAt { get; set; }

        public string ThumbnailURL { get; set; }

        #endregion

        #region Constructor

        public MusicYT()
        {

        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MusicYT(string videoID, string title, string url, string path, string channel, string channelID, DateTime? published, string thumbnailUrl)
        {
            VideoID = videoID;
            Title = title;
            URL = url;
            Path = path;
            Channel = channel;
            ChannelID = channelID;
            PublishedAt = published;
            ThumbnailURL = thumbnailUrl;
        }

        #endregion
    }
}
