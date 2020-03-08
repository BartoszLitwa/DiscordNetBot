using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetBot.DataBase;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        #region Private Properties

        private readonly DatabaseContext Database;

        #endregion

        #region Constrcutor

        public InfoCommands(DatabaseContext context)
        {
            Database = context;
        }

        #endregion

        #region Public Commands

        [Command(nameof(Info), true, RunMode = RunMode.Async)]
        [Summary("Gets the info about the given user")]
        public async Task Info(SocketUser DiscordUser = null)
        {
            var user = DiscordUser ?? Context.User;

            var game = user.Activity.Name != null ? $"{user.Activity.Type} {user.Activity.Name}" : "User is currently not playing any game";
            var roles = new StringBuilder();
            if (user != null)
            {
                foreach (var role in (user as IGuildUser).RoleIds)
                {
                    var roleName = Context.Guild.GetRole(role).Name;

                    if (roleName != Context.Guild.EveryoneRole.Name)
                        roles.Append($"{roleName}({role})\n");
                }
            }
            else
                roles.Append("User doesnt not have any roles currently");

            var infoEmbed = new EmbedBuilder
            {
                Title = $"Info about {user.Username}",
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = Color.Green,
            };

            infoEmbed.AddField("User", $"{user.Username}");
            infoEmbed.AddField("Discriminator(4 digits)", user.Discriminator);
            infoEmbed.AddField("Status", user.Status.ToString());
            infoEmbed.AddField("Game", game);
            infoEmbed.AddField("Avatar URL", user.GetAvatarUrl());
            infoEmbed.AddField("Default URL", user.GetDefaultAvatarUrl());
            infoEmbed.AddField("Created At", user.CreatedAt.ToLocalTime().ToString());
            infoEmbed.AddField("Joined At", (user as IGuildUser).JoinedAt?.ToLocalTime().ToString());
            infoEmbed.AddField("Roles", roles.ToString());
            infoEmbed.AddField("Is Owner", (user as IGuildUser).Guild.OwnerId == user.Id);
            infoEmbed.AddField("Is Bot", user.IsBot.ToString());

            await ReplyAsync(embed: infoEmbed.Build()).ConfigureAwait(false);
        }

        [Command(nameof(Ping), true, RunMode = RunMode.Async)]
        [Summary("Gets the info about the given user")]
        public async Task Ping()
        {
            var embed = new EmbedBuilder
            {
                Title = "Ping",
                Color = Color.Green,
                Description = $"{Context.Client.Latency}ms Pong",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        #endregion
    }
}
