using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetBot.DataBase;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    public class Bot
    {
        #region Public Properties

        public DiscordSocketClient Client;

        public CommandService CmdService;

        public IServiceProvider ServiceProvider;

        #endregion

        #region Constructor

        public Bot(IServiceProvider services)
        {
            ServiceProvider = services;

            Task.Run(RunAsync).ConfigureAwait(false);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
        .AddSingleton(Client)
        .AddSingleton(CmdService)
        .AddSingleton(ServiceProvider)
        .AddSingleton<DatabaseContext>()
        .BuildServiceProvider();

        #endregion

        public async Task RunAsync()
        {
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MessageCacheSize = 100,
            };

            Client = new DiscordSocketClient(config);

            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["Token"]);

            await Client.StartAsync();

            Client.Ready += Client_Ready;

            var commandConfig = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
            };

            CmdService = new CommandService(commandConfig);

            Client.MessageReceived += HandleCommandAsync;

            await CmdService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

            //CmdService.AddTypeReader(typeof(bool), new BooleanTypeReader());

            // Wait forever
            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync("Developed by CRNYY", type: ActivityType.Playing);

            Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Musics");

            MusicDownloader.Initialize();

            MusicHelpers.DeleteMusicStored();

            Console.WriteLine("Bot is Ready");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(ConfigurationManager.AppSettings["Prefix"].ToCharArray()[0], ref argPos) ||
                message.HasMentionPrefix(Client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(Client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await CmdService.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: ServiceProvider);
        }
    }
}
