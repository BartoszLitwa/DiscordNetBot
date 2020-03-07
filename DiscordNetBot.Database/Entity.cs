using System.ComponentModel.DataAnnotations;

namespace DiscordNetBot.DataBase
{
    /// <summary>
    /// Entity
    /// </summary>
    public abstract class Entity
    {
        [Key]
        public  int ID { get; set; }
    }
}
