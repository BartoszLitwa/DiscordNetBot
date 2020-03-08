using Discord;
using Discord.Commands;
using DiscordNetBot.DataBase;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    public class ReputationCommands : ModuleBase<SocketCommandContext>
    {
        #region Private Properties

        private readonly DatabaseContext Database;

        #endregion

        #region Constrcutor

        public ReputationCommands(DatabaseContext context)
        {
            Database = context;
        }

        #endregion

        #region Public Commands

        [Command(nameof(Rep), true, RunMode = RunMode.Async)]
        [Summary("Adds the reputation to the user")]
        public async Task Rep(IGuildUser Guilduser)
        {
            var user = await GetRepUser(Guilduser);

            user.Reputation += 1;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var embed = new EmbedBuilder
            {
                Title = $"{Guilduser.Username} got the rep!",
                Color = Color.Purple,
                Description = $"{Guilduser.Mention} got the reputation from {(Context.User as IGuildUser).Mention}.\n" +
                $"{Guilduser.Username} now has {user.Reputation} reputation!",
                ThumbnailUrl = Guilduser.GetAvatarUrl(),
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command(nameof(GetRep), true, RunMode = RunMode.Async)]
        [Summary("Gets the reputation points for given user")]
        public async Task GetRep(IGuildUser Guilduser = null)
        {
            Guilduser = Guilduser ?? (Context.User as IGuildUser);

            var user = await GetRepUser(Guilduser);

            var embed = new EmbedBuilder
            {
                Title = $"{Guilduser.Username} {user.Reputation} has reputation points!",
                Color = Color.Purple,
                ThumbnailUrl = Guilduser.GetAvatarUrl(),
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        #endregion

        #region Private helpers

        private async Task<User> GetRepUser(IGuildUser GuildUser)
        {
            try
            {
                return await Database.Users.FindAsync(GuildUser.Username);
            }
            catch (Exception)
            {
                var newUser = new User
                {
                    Username = GuildUser.Username,
                    Reputation = 0
                };

                await Database.Users.AddAsync(newUser);

                await Database.SaveChangesAsync();

                return newUser;
            }
        }

        #endregion
    }
}
