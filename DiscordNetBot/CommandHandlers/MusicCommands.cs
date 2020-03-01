using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        #region Public Properties

        public static Queue<YouTubeVideo> MusicQueue = new Queue<YouTubeVideo>();

        #endregion

        #region Public Commands

        [Command(nameof(Join), RunMode = RunMode.Async)]
        [Summary("Joins the given Voice Channel")]
        public async Task<IAudioClient> Join(SocketVoiceChannel channel = null)
        {
            EmbedBuilder embed;

            if ((Context.Client as IGuildUser)?.VoiceChannel != null)
            {
                embed = new EmbedBuilder
                {
                    Title = $"Already in Voice Channel",
                    Color = Color.Blue,
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Description = $"Failed to join the Voice Channel requested by {Context.User.Username}\n because I'm already in another Voice Channel."
                };

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                return null;
            }

            var channelToJoin = channel ?? (Context.User as IGuildUser).VoiceChannel;

            if (channelToJoin == null)
            {
                embed = new EmbedBuilder
                {
                    Title = $"Failed to join",
                    Color = Color.Blue,
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Description = $"You have to be in Voice Channel or pass the VC as argument. Requested by the {Context.User.Username}."
                };

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                return null;
            }

            embed = new EmbedBuilder
            {
                Title = $"Joined the Voice Channel",
                Color = Color.Blue,
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Description = $"Successfully joined to the {channelToJoin.Name} Voice Channel requested by {Context.User.Username}."
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);

            return await channelToJoin.ConnectAsync();
        }

        [Command(nameof(Leave), RunMode = RunMode.Async)]
        [Summary("Leaves the voice channel that it is in")]
        public async Task Leave()
        {
            EmbedBuilder embed;

            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;

            if (voiceChannel == null)
            {
                embed = new EmbedBuilder
                {
                    Title = $"Curently in none Voice Channel",
                    Color = Color.Blue,
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Description = $"I'm not currently in any of the Voice Channels."
                };

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            embed = new EmbedBuilder
            {
                Title = $"Joined the Voice Channel",
                Color = Color.Blue,
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Description = $"Successfully left from the {voiceChannel.Name} Voice Channel requested by {Context.User.Username}."
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);

            await voiceChannel.DisconnectAsync();
        }

        [Command(nameof(Play), RunMode = RunMode.Async)]
        [Summary("Plays the music in the voice channel that it is in")]
        public async Task Play([Remainder] string Url)
        {
            var audioClient = await Join();

            var video = await MusicDownloader.DownloadMusicFromYT(Url);

            MusicQueue.Enqueue(video);

            await PlayMusicAsync(audioClient, video);
        }

        [Command(nameof(Clear), RunMode = RunMode.Async)]
        [Summary("Clears the song request queue")]
        public async Task Clear()
        {
            foreach (var music in MusicQueue)
            {
                File.Delete(MusicHelpers.TitleToPath(MusicQueue.Dequeue().Title));
            }
        }

        #endregion

        #region Private Helpers

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task PlayMusicAsync(IAudioClient client, YouTubeVideo video)
        {
            EmbedBuilder MusicEmbed;

            using (var ffmpeg = CreateStream(MusicHelpers.TitleToPath(video.Title)))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await output.CopyToAsync(discord);
                }
                finally
                {
                    await discord.FlushAsync();

                    if(File.Exists(video.Title))
                    {
                        File.Delete(video.Title);
                        Console.WriteLine($"Successfully deleted {video.Title}");
                    }

                    if (MusicQueue.Count > 0)
                        await PlayMusicAsync(client, MusicQueue.Dequeue());
                    else
                    {

                        await Leave();
                    }
                }
            }
        }

        #endregion
    }
}
