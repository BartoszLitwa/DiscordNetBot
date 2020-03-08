using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordNetBot.DataBase
{
    /// <summary>
    /// User
    /// </summary>
    public class User : Entity
    {
        [Key]
        public string Username { get; set; }

        public int Reputation { get; set; }
    }
}
