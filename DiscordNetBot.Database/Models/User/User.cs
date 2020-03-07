namespace DiscordNetBot.DataBase
{
    /// <summary>
    /// User
    /// </summary>
    public class User : Entity
    {
        public string Username { get; set; }

        public int Reputation { get; set; }
    }
}
