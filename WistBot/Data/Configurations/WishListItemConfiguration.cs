using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class WishListItemConfiguration : IEntityTypeConfiguration<WishListItemEntity>
    {
        public void Configure(EntityTypeBuilder<WishListItemEntity> builder)
        {
            builder.HasKey(wli => wli.Id);
            builder.HasOne(wli => wli.List)
                .WithMany(wl => wl.Items)
                .HasForeignKey(wli => wli.ListId);

            builder.HasOne(ph => ph.Photo)
                .WithOne(i => i.WishListItem)
                .HasForeignKey<PhotoSizeEntity>(ph => ph.Id);

            builder.HasOne(v => v.Video)
                .WithOne(i => i.WishListItem)
                .HasForeignKey<VideoEntity>(v => v.Id);
        }
    }
}
