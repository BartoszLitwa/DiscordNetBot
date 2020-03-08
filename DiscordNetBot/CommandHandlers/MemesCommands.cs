using Discord;
using Discord.Commands;
using DiscordNetBot.DataBase;
using System;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    public class MemesCommands : ModuleBase<SocketCommandContext>
    {
        #region Private Properties

        private readonly DatabaseContext Database;

        #endregion

        #region Constrcutor

        public MemesCommands(DatabaseContext context)
        {
            Database = context;
        }

        #endregion

        #region Public Commands

        [Command(nameof(Meme), true, RunMode = RunMode.Async)]
        [Summary("Gets the Meme")]
        public async Task Meme()
        {
            var meme = MemeApi.GetMeme();

            var embed = new EmbedBuilder
            {
                Title = meme.Title,
                Color = Color.Gold,
                ImageUrl = meme.URL,
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        #endregion

        #region Private helpers

        

        #endregion
    }
}
