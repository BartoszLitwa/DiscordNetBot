using System;

namespace DiscordNetBot
{
    /// <summary>
    /// MusicHelpers
    /// </summary>
    public static class MusicHelpers
    {
        public static string TitleToPath(string Title) => $"{Environment.CurrentDirectory}\\Musics\\{Title}.mp3";
    }
}
