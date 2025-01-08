using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class WishListConfiguration : IEntityTypeConfiguration<WishListEntity>
    {
        public void Configure(EntityTypeBuilder<WishListEntity> builder)
        {
            builder.HasKey(wl => wl.Id);
            builder.HasMany(wl => wl.Items)
                .WithOne(wli => wli.List)
                .HasForeignKey(wli => wli.ListId);

            builder.HasOne(wl => wl.Owner)
                .WithMany(u => u.WishLists)
                .HasForeignKey(wl => wl.OwnerId);
        }
    }
}
