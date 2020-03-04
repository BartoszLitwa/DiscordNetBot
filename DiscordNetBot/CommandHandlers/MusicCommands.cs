using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DiscordNetBot.Emojis;

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

        public static AudioOutStream Discord;

        public static MusicYT CurrentlyPlaying;

        #endregion

        #region Constructor

        public MusicCommands()
        {
            foreach (var music in Directory.GetFiles($"{Environment.CurrentDirectory}\\Musics\\"))
            {
                if (File.Exists(music))
                    File.Delete(music);
            }
        }

        #endregion

        #region Public Commands

        [Command(nameof(Join), RunMode = RunMode.Async)]
        [Summary("Joins the given Voice Channel")]
        public async Task Join()
        {
            var audio = await JoinChannel().ConfigureAwait(false);
        }

        [Command(nameof(Leave), RunMode = RunMode.Async)]
        [Summary("Leaves the voice channel that it is in")]
        public async Task Leave()
        {
            EmbedBuilder embed;

            var voiceChannel = (Context.Client as IGuildUser)?.VoiceChannel;

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

            await voiceChannel.DisconnectAsync();

            embed = new EmbedBuilder
            {
                Title = $"Left the Voice Channel",
                Color = Color.Blue,
                ThumbnailUrl = Context.User.GetAvatarUrl(),
                Description = $"Successfully left from the {voiceChannel.Name} Voice Channel requested by {Context.User.Username}."
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command(nameof(Play), RunMode = RunMode.Async)]
        [Summary("Plays the music in the voice channel that it is in")]
        public async Task Play([Remainder] string Url)
        {
            var audioClient = await JoinChannel((Context.User as IGuildUser).VoiceChannel);

            MusicYT video;

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

            var embed = new EmbedBuilder
            {
                Title = "Cleared the Queue",
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command(nameof(Add), RunMode = RunMode.Async)]
        [Summary("Adds the song to the playlist")]
        public async Task Add([Remainder] string URL)
        {
            var video = await GetMusicFromURL(URL);

            var EmbedBuilder = new EmbedBuilder
            {
                Title = "Added To Queue",
                Description = $"{video.URL}",
                ThumbnailUrl = video.URL,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Channel",
                        Value = video.Channel
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Position in Queue",
                        Value = MusicQueue.Count
                    }
                }
            };

            await ReplyAsync(embed: EmbedBuilder.Build()).ConfigureAwait(false);

            MusicQueue.Enqueue(video);

            if(!MusicPlaying)
                await PlayMusicAsync(await JoinChannel((Context.User as IGuildUser).VoiceChannel), video);
        }

        [Command(nameof(Skip), RunMode = RunMode.Async)]
        [Summary("Adds the song to the playlist")]
        public async Task Skip()
        {
            if (MusicPlaying)
            {
                await Discord.DisposeAsync();

                var MusicEmbed = new EmbedBuilder
                {
                    Title = $"Skipped {CurrentlyPlaying.Title}",
                    ThumbnailUrl = CurrentlyPlaying.ThumbnailURL,
                };

                await ReplyAsync(embed: MusicEmbed.Build()).ConfigureAwait(false);

                if (MusicQueue.Count > 0)
                {
                    await PlayMusicAsync(await JoinChannel(), MusicQueue.Dequeue());
                }
            }
        }

        #endregion

        #region Private Helpers

        private async Task<MusicYT> ShowResultsFromSearch(string searchText)
        {
            var musicList = await MusicDownloader.GetSearchResults(searchText);

            var fieldEmbeds = new List<EmbedFieldBuilder>();

            var emojis = new IEmote[] { new Emoji("\U0001F44D"), //Thumbs Up
                                       new Emoji("\U0001F44E"),  //Thumbs Down
                                       new Emoji("\U0001F923"),  //Rofl
                                       new Emoji("\U0001F911"),  //Money
                                       new Emoji("\U0001F975"),};//Hot

            var index = 0;
            foreach (var music in musicList)
            {
                fieldEmbeds.Add(new EmbedFieldBuilder
                {
                    Name = $"{index + 1}. Result {emojis[index]}",
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

            await message.AddReactionsAsync( emojis, new RequestOptions { Timeout = 10 });

            await Task.Delay(5000);

            var reactions = new List<IEnumerable<IUser>>();

            int max = 0;
            int Result = 0;
            for (int i = 0; i < 5; i++)
            {
                var react = await message.GetReactionUsersAsync(emojis[i], 10).FlattenAsync();
                reactions.Add(react);
                if (reactions.ElementAt(i).Count() > max)
                {
                    max = reactions.ElementAt(i).Count();
                    Result = i;
                }
            }

            await MusicDownloader.DownloadMusicFromYT(musicList[Result].URL);

            return musicList[Result];
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
            using (Discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    MusicPlaying = true;

                    await output.CopyToAsync(Discord);

                    CurrentlyPlaying = video;

                    MusicEmbed = new EmbedBuilder
                    {
                        Title = $"Playing {video.Title}",
                        ThumbnailUrl = video.ThumbnailURL,
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Title",
                                Value = video.Title
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "URL",
                                Value = video.URL
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Channel",
                                Value = video.Channel
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Published",
                                Value = video.PublishedAt
                            },
                        }
                    };

                    await ReplyAsync(embed: MusicEmbed.Build()).ConfigureAwait(false);
                }
                finally
                {
                    await Discord.FlushAsync();

                    MusicQueue.Dequeue();

                    CurrentlyPlaying = null;

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

        private async Task<IAudioClient> JoinChannel(IAudioChannel channel = null)
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

        #endregion
    }
}
