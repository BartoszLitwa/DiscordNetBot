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

        public static Queue<MusicYT> MusicQueue = new Queue<MusicYT>();

        public static bool MusicPlaying = false;

        #endregion

        #region Constructor

        public MusicCommands()
        {
            foreach (var music in Directory.GetFiles($"{Environment.CurrentDirectory}\\Musics\\"))
            {
                File.Delete(music);
            }
        }

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

            var video = new MusicYT();

            if (Url.Contains("youtube.com/"))
                video = await MusicDownloader.DownloadMusicFromYT(Url);
            else
                video = await ShowResultsFromSearch(Url);

            MusicQueue.Enqueue(video);

            await PlayMusicAsync(audioClient, video);
        }

        [Command(nameof(Clear), RunMode = RunMode.Async)]
        [Summary("Clears the song request queue")]
        public async Task Clear()
        {
            foreach (var music in MusicQueue)
            {
                File.Delete(MusicHelpers.TitleToPath(music.Title));
            }
            MusicQueue.Clear();
        }

        [Command(nameof(Add), RunMode = RunMode.Async)]
        [Summary("Adds the song to the playlist")]
        public async Task Add([Remainder] string URL)
        {
            var video = await GetMusicFromURL(URL);

            MusicQueue.Enqueue(video);

            if(!MusicPlaying)
                await PlayMusicAsync(await Join(), video);
        }

        #endregion

        #region Private Helpers

        private async Task<MusicYT> ShowResultsFromSearch(string searchText)
        {
            var musicList = await MusicDownloader.GetSearchResults(searchText);

            var fieldEmbeds = new List<EmbedFieldBuilder>();

            var index = 1;
            foreach (var music in musicList)
            {
                fieldEmbeds.Add(new EmbedFieldBuilder
                {
                    Name = $"{index}. Result",
                    Value = $"{music.Title} by {music.Channel}."
                });
                index++;
            }

            var resultEmbed = new EmbedBuilder
            {
                Title = $"Search results of {searchText}",
                Color = Color.Green,
                Fields = fieldEmbeds,
            };

            var message = await ReplyAsync(embed: resultEmbed.Build());
            var digitOne = new Emoji("\U00000031");
            await message.AddReactionsAsync(new IEmote[] { digitOne });

            return musicList[0];
        }

        private async Task<MusicYT> GetMusicFromURL(string URL)
        {
            return URL.Contains("youtube.com/") ? await MusicDownloader.DownloadMusicFromYT(URL) : await ShowResultsFromSearch(URL);
        }

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

        private async Task PlayMusicAsync(IAudioClient client, MusicYT video)
        {
            EmbedBuilder MusicEmbed;

            using (var ffmpeg = CreateStream($"Musics\\{video.Title}.mp3"))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    MusicPlaying = true;

                    await output.CopyToAsync(discord);
                }
                finally
                {
                    await discord.FlushAsync();

                    MusicPlaying = false;

                    if (File.Exists(MusicHelpers.TitleToPath(video.Title)))
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
