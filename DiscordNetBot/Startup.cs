using DiscordNetBot.DataBase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace DiscordNetBot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer($"Server=.;Database=DiscordNetBot;User ID={ConfigurationManager.AppSettings["SqlLogin"]};pwd={ConfigurationManager.AppSettings["SqlPassword"]};");
            });

            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider);

            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
