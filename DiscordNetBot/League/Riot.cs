using RiotSharp;
using RiotSharp.Endpoints.ChampionEndpoint;
using RiotSharp.Endpoints.ChampionMasteryEndpoint;
using RiotSharp.Endpoints.LeagueEndpoint;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Endpoints.MatchEndpoint.Enums;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using RiotSharp.Endpoints.StaticDataEndpoint.ProfileIcons;
using RiotSharp.Endpoints.SummonerEndpoint;
using RiotSharp.Misc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    public static class Riot
    {
        public static RiotApi Api { get; private set; }

        public static ProfileIconListStatic IconList { get; private set; }
        public static ChampionListStatic ChampionList { get; private set; }

        public static string CurrentVersion { get; private set; }

        public static async void Initialize()
        {
            var key = ConfigurationManager.AppSettings["RiotApiKey"];
            Api = RiotApi.GetInstance(key, 20, 100);
            CurrentVersion = await GetCurrentVersion();
            IconList = await GetProfileIconListStatic(CurrentVersion);
            ChampionList = await GetChampionListStatic(CurrentVersion);
        }

        public static async Task<Summoner> GetSummoner(string nick, Region region = Region.Eune)
        {
            try
            {
                return await Api.Summoner.GetSummonerByNameAsync(region, nick);
            }
            catch (RiotSharpException ex) {
                Console.WriteLine($"{DateTime.Now} GetSummoner: {ex.Message}");
                return null;
            }
        }

        public static async Task<ChampionRotation> GetCurrentChampionRotation(Region region = Region.Eune)
        {
            return await Api.Champion.GetChampionRotationAsync(region);
        }

        #region Masteries

        public static async Task<int> GetTotalMasteryScore(string summonerID, Region region = Region.Eune)
        {
            return await Api.ChampionMastery.GetTotalChampionMasteryScoreAsync(region, summonerID);
        }

        public static async Task<List<ChampionMastery>> GetChampionsMasteries(string summonerID, Region region = Region.Eune)
        {
            return await Api.ChampionMastery.GetChampionMasteriesAsync(region, summonerID);
        }

        public static async Task<ChampionMastery> GetChampionMastery(string summonerID, long championID, Region region = Region.Eune)
        {
            return await Api.ChampionMastery.GetChampionMasteryAsync(region, summonerID, championID);
        }

        #endregion

        #region League Positions

        public static async Task<List<LeaguePosition>> GetLeaguePositions(string summonerID, Region region = Region.Eune)
        {
            try
            {
                var x = await Api.League.GetLeaguePositionsAsync(region, summonerID);
                return x;
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine($"{DateTime.Now} GetLeaguePositions: {ex.Message}");
                return null;
            }
        }

        public static async Task<League> GetChallengerLeaguePositions(string summonerID, Region region = Region.Eune)
        {
            return await Api.League.GetChallengerLeagueAsync(region, summonerID);
        }

        public static async Task<League> GetMasterLeaguePositions(string summonerID, Region region = Region.Eune)
        {
            return await Api.League.GetMasterLeagueAsync(region, summonerID);
        }

        #endregion

        #region Match

        public static async Task<Match> GetMatch(long matchID, Region region = Region.Eune)
        {
            return await Api.Match.GetMatchAsync(region, matchID);
        }

        public static async Task<MatchList> GetMatchList(string accountID, Region region = Region.Eune, List<int> championsIDs = null, List<int> queues = null, List<Season> seasons = null)
        {
            return await Api.Match.GetMatchListAsync(region, accountID, championsIDs, queues, seasons);
        }

        #endregion

        public static async Task<ProfileIconListStatic> GetProfileIconListStatic(string version, Language language = Language.en_US)
        {
            return await Api.StaticData.ProfileIcons.GetAllAsync(version, language);
        }

        public static async Task<ChampionListStatic> GetChampionListStatic(string version, Language language = Language.en_US)
        {
            return await Api.StaticData.Champions.GetAllAsync(CurrentVersion, language);
        }

        public static async Task<string> GetCurrentVersion()
        {
            return (await Api.StaticData.Versions.GetAllAsync()).First();
        }

        public static string GetIconImageURL(string summoner, Region region = Region.Eune)
        {
            return $"http://avatar.leagueoflegends.com/{region.ToString().ToLower()}/{summoner}.png";
        }

        public static string GetChampionName(long champID, Language language = Language.en_US)
        {
            return ChampionList.Champions.Where(x => x.Value.Id == champID).First().Value.Name;
        }
    }
}
