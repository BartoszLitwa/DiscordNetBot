using System;
using System.IO;

namespace DiscordNetBot
{
    /// <summary>
    /// MusicHelpers
    /// </summary>
    public static class MusicHelpers
    {
        public static string TitleToPath(string Title) => $"{Environment.CurrentDirectory}\\Musics\\{Title}.mp3";

        public static string PathToTitle(string path)
        {
            path = path.Remove(0, $"{Environment.CurrentDirectory}\\Musics\\".Length);
            return path.Remove(path.IndexOf(".mp3"), 4);
        } 

        public static void DeleteMusicStored(string toSkipped = null)
        {
            toSkipped = toSkipped == null ? "NothingToSkip" : toSkipped;
            foreach (var music in Directory.GetFiles($"{Environment.CurrentDirectory}\\Musics\\"))
            {
                if (File.Exists(music) && toSkipped != PathToTitle(music))
                    File.Delete(music);
            }
        }
    }
}
