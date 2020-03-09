using Discord;
using Discord.Commands;
using DiscordNetBot.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    /// <summary>
    /// InfoCommands
    /// </summary>
    public class LeagueCommands : ModuleBase<SocketCommandContext>
    {
        #region Private Properties

        private readonly DatabaseContext Database;

        #endregion

        #region Constrcutor

        public LeagueCommands(DatabaseContext context)
        {
            Database = context;
        }

        #endregion

        #region Public Commands

        [Command(nameof(League), true, RunMode = RunMode.Async)]
        [Summary("Gets the information about the given summoner")]
        public async Task League([Remainder] string Nick)
        {
            var summoner = await Riot.GetSummoner(Nick);

            if(summoner == null)
            {
                var embedError = new EmbedBuilder
                {
                    Title = $"There was an Error",
                    Color = Color.Red,
                    Description = "An Error occured while geting the League Data. Try again!",
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                };

                await ReplyAsync(embed: embedError.Build()).ConfigureAwait(false);
            }

            var masteries = await Riot.GetChampionsMasteries(summoner.Id);
            masteries.OrderByDescending(x => x.ChampionPoints);
            var mostPlayedChamp = masteries.First();
            var MPCName = Riot.GetChampionName(mostPlayedChamp.ChampionId);
            //var rank = await Riot.GetLeaguePositions(summoner.Id);

            var fields = new List<EmbedFieldBuilder>
            {
                //new EmbedFieldBuilder
                //{
                //    IsInline = true,
                //    Name = rank.First().QueueType,
                //    Value = rank.First().Tier
                //},
                //new EmbedFieldBuilder
                //{
                //    IsInline = true,
                //    Name = "Wins",
                //    Value = rank.First().Wins
                //},
                //new EmbedFieldBuilder
                //{
                //    IsInline = true,
                //    Name = "Losses",
                //    Value = rank.First().Losses
                //},
                new EmbedFieldBuilder
                {
                    Name = "Level",
                    Value = summoner.Level
                },
                new EmbedFieldBuilder
                {
                    Name = "Region",
                    Value = summoner.Region
                },
                new EmbedFieldBuilder
                {
                    Name = "Mastery Score",
                    Value = await Riot.GetTotalMasteryScore(summoner.Id)
                },
                new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Most Played Champion",
                    Value = MPCName
                },
                new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Level",
                    Value = mostPlayedChamp.ChampionLevel,
                },
                new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Points",
                    Value = mostPlayedChamp.ChampionPoints,
                },
                new EmbedFieldBuilder
                {
                    Name = "Last modified",
                    Value = summoner.RevisionDate
                },
            };

            var embed = new EmbedBuilder
            {
                Title = $"{summoner.Name} LOL Stats",
                Color = Color.DarkBlue,
                Fields = fields,
                ThumbnailUrl = Riot.GetIconImageURL(summoner.Name),
            };

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        #endregion

        #region Private helpers

        

        #endregion
    }
}
