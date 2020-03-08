using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace DiscordNetBot
{
    public static class MemeApi
    {
        public static Meme GetMeme()
        {
            using(var web = new WebClient())
            {
                var json = web.DownloadString(ConfigurationManager.AppSettings["MemeApiLink"]);
                return JsonConvert.DeserializeObject<Meme>(json);
            }
        }
    }
}
