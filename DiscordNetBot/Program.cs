using System;

namespace DiscordNetBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
