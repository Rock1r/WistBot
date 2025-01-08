using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(u => u.TelegramId);
            builder.HasMany(u => u.WishLists)
                .WithOne(wl => wl.Owner)
                .HasForeignKey(wl => wl.OwnerId);
        }
    }
}
