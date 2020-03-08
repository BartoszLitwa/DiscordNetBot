using Discord;
using Discord.Commands;
using DiscordNetBot.DataBase;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    [RequireOwner]
    public class DeveloperCommands : ModuleBase<SocketCommandContext>
    {
        #region Private Properties

        private readonly DatabaseContext Database;

        #endregion

        #region Constrcutor

        public DeveloperCommands(DatabaseContext context)
        {
            Database = context;
        }

        #endregion

        #region Public Commands

        [Command(nameof(Exit), true, RunMode = RunMode.Async)]
        [Summary("Turns off the discord bot. Can be only accessed by the developer = CRNYY")]
        public async Task Exit()
        {
            var embed = new EmbedBuilder
            {
                Title = "Bot will be turned off",
                Description = $"Bot is going to be turned off. Requested by the developer {Context.User.Username} at {DateTime.UtcNow}",
                Color = Color.Red,
                ThumbnailUrl = Context.User.GetAvatarUrl(),
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);

            Console.WriteLine($"Bot is going to be turned off. Requested by the developer {Context.User.Username} at {DateTime.UtcNow}");

            Environment.Exit(0);
        }

        [Command(nameof(Shutdown), true, RunMode = RunMode.Async)]
        [Summary("Shutdowns the computer that the bot is being ran on. Can be only accessed by the developer = CRNYY")]
        public async Task Shutdown(int time = 0)
        {
            var embed = new EmbedBuilder
            {
                Title = "The computer will be Shutdown",
                Description = $"Bot and PC is going to be turned off. Requested by the developer {Context.User.Username} at {DateTime.UtcNow}",
                Color = Color.Red,
                ThumbnailUrl = Context.User.GetAvatarUrl(),
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);

            Console.WriteLine($"Bot and PC is going to be turned off. Requested by the developer {Context.User.Username} at {DateTime.UtcNow}");

            Process.Start("shutdown", $"/s /t {time}");
        }

        [Command(nameof(AddUser), true, RunMode = RunMode.Async)]
        [Summary("Adds the new user to database. Can be only accessed by the developer = CRNYY")]
        public async Task AddUser(string name, int rep)
        {
            await Database.Users.AddAsync(new User
            {
                Username = name,
                Reputation = rep
            }).ConfigureAwait(false);

            await Database.SaveChangesAsync();
        }

        [Command(nameof(GetUsers), true, RunMode = RunMode.Async)]
        [Summary("Gets every user in database. Can be only accessed by the developer = CRNYY")]
        public async Task GetUsers()
        {
            var users = string.Empty;
            foreach (var user in Database.Users)
            {
                users += $"ID:{user.ID} User:{user.Username} Reputation:{user.Reputation}\n";
            }

            await ReplyAsync(users).ConfigureAwait(false);
        }

        #endregion
    }
}
