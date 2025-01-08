using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Configuration;
using Telegram.Bot.Types;
using WistBot.Data.Configurations;
using WistBot.Data.Models;

namespace WistBot.Data
{
    public class WistBotDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<WishListEntity> WishLists { get; set; }
        public DbSet<WishListItemEntity> WishListItems { get; set; }

        public WistBotDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new WishListConfiguration());
            modelBuilder.ApplyConfiguration(new WishListItemConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoSizeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString(nameof(WistBotDbContext)));
        }
    }
}
