using Microsoft.EntityFrameworkCore;

namespace DiscordNetBot.DataBase
{
    /// <summary>
    /// DatabaseContext
    /// </summary>
    public class DatabaseContext : DbContext
    {
        #region Public Properties

        public DbSet<User> Users { get; set; }

        #endregion

        #region Constructor

        public DatabaseContext(DbContextOptions<DatabaseContext> options = null) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API
            modelBuilder.Entity<User>().HasIndex(x => x.ID);
        }

        #endregion
    }
}
